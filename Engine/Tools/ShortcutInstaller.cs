using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Plants;

// Creates a per-user Start Menu shortcut with an AppUserModelID set in the PropertyStore.
// Required for Windows Toast Notifications to work when running from a raw exe
// (without MSIX packaging or COM activator registration).
// No admin rights required — writes to %APPDATA%\Microsoft\Windows\Start Menu\Programs.
public static class ShortcutInstaller
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

    public static void SetProcessAumid(string aumid)
    {
        try
        {
            SetCurrentProcessExplicitAppUserModelID(aumid);
        }
        catch (Exception ex)
        {
            CrashLogger.LogError("ShortcutInstaller.SetProcessAumid", ex);
        }
    }

    public static void EnsureShortcut(string aumid, string shortcutName, string exePath)
    {
        try
        {
            string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string programsDir = Path.Combine(startMenu, "Programs");
            string shortcutPath = Path.Combine(programsDir, shortcutName + ".lnk");

            if (File.Exists(shortcutPath))
                return;

            if (!Directory.Exists(programsDir))
                Directory.CreateDirectory(programsDir);

            CreateShortcut(shortcutPath, exePath, aumid);
            CrashLogger.LogInfo("ShortcutInstaller", $"Shortcut created: {shortcutPath} (AUMID={aumid})");
        }
        catch (Exception ex)
        {
            CrashLogger.LogError("ShortcutInstaller.EnsureShortcut", ex);
        }
    }

    private static void CreateShortcut(string shortcutPath, string targetExe, string aumid)
    {
        var link = (IShellLinkW)new CShellLink();
        link.SetPath(targetExe);
        link.SetArguments("");
        link.SetWorkingDirectory(Path.GetDirectoryName(targetExe) ?? "");
        link.SetIconLocation(targetExe, 0);

        var store = (IPropertyStore)link;
        var appIdKey = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

        var variant = new PropVariant();
        try
        {
            InitPropVariantFromString(aumid, ref variant);
            store.SetValue(ref appIdKey, ref variant);
            store.Commit();
        }
        finally
        {
            PropVariantClear(ref variant);
        }

        ((IPersistFile)link).Save(shortcutPath, true);
    }

    [DllImport("propsys.dll", CharSet = CharSet.Unicode)]
    private static extern int InitPropVariantFromString(string psz, ref PropVariant ppropvar);

    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PropVariant pvar);

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class CShellLink { }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszFile, int cch, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszName, int cch);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszDir, int cch);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszArgs, int cch);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out ushort pwHotkey);
        void SetHotkey(ushort wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszIconPath, int cch, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    private interface IPropertyStore
    {
        void GetCount(out uint cProps);
        void GetAt(uint iProp, out PropertyKey pkey);
        void GetValue(ref PropertyKey key, out PropVariant pv);
        void SetValue(ref PropertyKey key, ref PropVariant pv);
        void Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PropertyKey
    {
        public Guid fmtid;
        public uint pid;
        public PropertyKey(Guid fmtid, uint pid) { this.fmtid = fmtid; this.pid = pid; }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PropVariant
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(8)] public IntPtr pointerValue;
        [FieldOffset(8)] public long longValue;
    }
}
