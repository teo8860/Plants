using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class Obj_GuiPostaBadge : GameElement
{
    public int PostaButtonIndex { get; set; } = -1;

    private readonly Color badgeBg = new Color(210, 50, 50, 255);
    private readonly Color badgeBorder = new Color(255, 220, 220, 255);
    private readonly Color badgeText = new Color(255, 255, 255, 255);

    public Obj_GuiPostaBadge()
    {
        this.guiLayer = true;
        this.depth = -650;
        this.persistent = true;
    }

    public override void Draw()
    {
        if (Game.toolbarBottom == null || !Game.toolbarBottom.active) return;
        if (PostaButtonIndex < 0) return;

        GuiIconButton btn;
        try { btn = Game.toolbarBottom.GetButton(PostaButtonIndex); }
        catch { return; }

        if (btn == null || !btn.IsVisible || btn.CurrentAlpha < 100) return;

        int count = MailSystem.UnreadCount;
        if (count <= 0) return;

        string text = count > 9 ? "9+" : count.ToString();
        int badgeSize = 11;
        int bx = btn.X + btn.Size - badgeSize + 2;
        int by = (int)btn.Y - 3;

        Graphics.DrawCircle(bx + badgeSize / 2, by + badgeSize / 2, badgeSize / 2f + 0.8f, badgeBorder);
        Graphics.DrawCircle(bx + badgeSize / 2, by + badgeSize / 2, badgeSize / 2f, badgeBg);

        int textW = text.Length * 5;
        Graphics.DrawText(text, bx + (badgeSize - textW) / 2 + 1, by + 2, 8, badgeText);
    }
}
