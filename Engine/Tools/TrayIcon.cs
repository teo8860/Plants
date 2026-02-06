
namespace Plants;

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

public class NativeTrayIcon : IDisposable
{
    private const int WM_USER = 0x0400;
    private const int WM_NOTIFYICON = WM_USER + 1;

    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;

    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_DELETE = 0x00000002;

    private const uint TPM_RIGHTBUTTON = 0x0002;

    private IntPtr _hWnd;
    private int _iconId = 1;
    private IntPtr _hIcon;
    private IntPtr _hMenu;

    public event Action OnClickLeft;
    public event Action OnExit;

    public NativeTrayIcon(Icon hIcon, string tip)
    {
        _hIcon = hIcon.Handle;
        _hWnd = CreateMessageWindow();

        // Aggiungo l'icona
        var nid = new NOTIFYICONDATA();
        nid.cbSize = (uint)Marshal.SizeOf(nid);
        nid.hWnd = _hWnd;
        nid.uID = (uint)_iconId;
        nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
        nid.uCallbackMessage = WM_NOTIFYICON;
        nid.hIcon = _hIcon;
        nid.szTip = tip;

        Shell_NotifyIcon(NIM_ADD, ref nid);

        // Crea il menu contestuale
        _hMenu = CreatePopupMenu();
        const uint MF_STRING = 0x0000;

        // Aggiungo “Esci” con ID 1000
        AppendMenu(_hMenu, MF_STRING, 1000, "Esci");
    }

    private IntPtr CreateMessageWindow()
    {
        var wc = new WNDCLASS();
        wc.lpszClassName = "NativeTrayIconWndClass";
        wc.lpfnWndProc = WindowProc;
        RegisterClass(ref wc);

        IntPtr hwnd = CreateWindowEx(
            0, wc.lpszClassName, "TrayWindow",
            0, 0, 0, 0, 0,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero
        );
        return hwnd;
    }

    private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        const int WM_LBUTTONUP = 0x0202;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_COMMAND = 0x0111;

        if (msg == WM_NOTIFYICON)
        {
            int m = lParam.ToInt32();
            if (m == WM_LBUTTONUP)
            {
                OnClickLeft?.Invoke();
            }
            else if (m == WM_RBUTTONUP)
            {
                ShowContextMenu();
            }
        }
        else if (msg == WM_COMMAND)
        {
            int id = wParam.ToInt32() & 0xFFFF;
            if (id == 1000)
            {
                // Esci è stato cliccato
                OnExit?.Invoke();
            }
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void ShowContextMenu()
    {
        // Posizione del cursore
        POINT pt;
        GetCursorPos(out pt);

        // La finestra deve essere foreground per far scomparire il menu correttamente :contentReference[oaicite:0]{index=0}
        SetForegroundWindow(_hWnd);

        TrackPopupMenu(_hMenu, TPM_RIGHTBUTTON, pt.X, pt.Y, 0, _hWnd, IntPtr.Zero);
    }

    public void Dispose()
    {
        var nid = new NOTIFYICONDATA();
        nid.cbSize = (uint)Marshal.SizeOf(nid);
        nid.hWnd = _hWnd;
        nid.uID = (uint)_iconId;
        Shell_NotifyIcon(NIM_DELETE, ref nid);
        DestroyWindow(_hWnd);
    }

    /// <summary> 
    /// Fai partire un loop di lettura eventi per non far chiudere il programma
    /// </summary>
    public void LoopEvent()
    {
        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

    }
    
    public void LoopEventRender()
    {
        MSG msg;
        while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 1))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }

    }

    // P/Invoke

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam
    );

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll")]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    // Struct e delegati

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        // campi extra se servono
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASS
    {
        public uint style;
        public WndProc lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    
    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint asdasd );
    
    [DllImport("user32.dll")]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
       // public POINT pt;
    }
}
