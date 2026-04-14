using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;

namespace Plants;

public class Obj_GuiPostaPopup : GameElement
{
    private bool isVisible = false;
    private int scrollY = 0;
    private int hoveredRow = -1;
    private bool claimAllHovered = false;
    private bool clearHovered = false;
    private bool closeHovered = false;

    private const int PANEL_W = 280;
    private const int PANEL_H = 230;
    private const int HEADER_H = 24;
    private const int ROW_H = 40;
    private const int ROW_SPACING = 3;
    private const int BUTTONS_AREA_H = 36;

    private readonly Color overlayColor = new Color(0, 0, 0, 170);
    private readonly Color panelBg = new Color(35, 40, 30, 248);
    private readonly Color panelBorder = new Color(120, 160, 100, 255);
    private readonly Color headerBg = new Color(55, 80, 45, 255);
    private readonly Color textColor = new Color(230, 235, 225, 255);
    private readonly Color subTextColor = new Color(160, 170, 150, 255);
    private readonly Color mutedTextColor = new Color(110, 120, 105, 255);
    private readonly Color rowBg = new Color(50, 58, 45, 255);
    private readonly Color rowHoverBg = new Color(70, 90, 60, 255);
    private readonly Color rowClaimedBg = new Color(42, 45, 40, 255);
    private readonly Color rowBorder = new Color(90, 110, 80, 255);
    private readonly Color rowClaimedBorder = new Color(70, 75, 68, 255);
    private readonly Color statusNewColor = new Color(230, 200, 80, 255);
    private readonly Color statusClaimedColor = new Color(110, 120, 105, 255);
    private readonly Color claimAllColor = new Color(70, 140, 60, 255);
    private readonly Color claimAllHoverColor = new Color(90, 180, 80, 255);
    private readonly Color clearColor = new Color(130, 80, 60, 255);
    private readonly Color clearHoverColor = new Color(170, 100, 75, 255);
    private readonly Color buttonDisabledColor = new Color(70, 75, 65, 255);
    private readonly Color closeColor = new Color(80, 90, 85, 255);
    private readonly Color closeHoverColor = new Color(110, 120, 115, 255);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    public Obj_GuiPostaPopup()
    {
        this.guiLayer = true;
        this.depth = -2000;
        this.persistent = true;
    }

    public void Show()
    {
        MailSystem.RefreshRecurringMails();
        isVisible = true;
        scrollY = 0;
    }

    public void Hide()
    {
        isVisible = false;
    }

    public bool IsVisible => isVisible;

    private (int x, int y, int w, int h) GetPanelRect()
    {
        int panelX = (sw - PANEL_W) / 2;
        int panelY = (sh - PANEL_H) / 2;
        return (panelX, panelY, PANEL_W, PANEL_H);
    }

    private (int x, int y, int w, int h) GetListRect()
    {
        var (px, py, pw, _) = GetPanelRect();
        int listX = px + 8;
        int listY = py + HEADER_H + 6;
        int listW = pw - 16;
        int listH = PANEL_H - HEADER_H - BUTTONS_AREA_H - 14;
        return (listX, listY, listW, listH);
    }

    public override void Update()
    {
        if (!isVisible) return;

        if (Game.guiRewardPopup != null && Game.guiRewardPopup.IsVisible)
            return;

        var (panelX, panelY, panelW, panelH) = GetPanelRect();
        var (listX, listY, listW, listH) = GetListRect();

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        List<MailMessage> inbox = MailSystem.Inbox;

        // Scroll
        if (mx >= listX && mx <= listX + listW && my >= listY && my <= listY + listH)
        {
            float wheel = Input.GetMouseWheelMove();
            if (wheel != 0)
            {
                scrollY -= (int)(wheel * 20);
                int contentH = inbox.Count * (ROW_H + ROW_SPACING);
                int maxScroll = Math.Max(0, contentH - listH);
                scrollY = Math.Clamp(scrollY, 0, maxScroll);
            }
        }

        // Hover righe
        hoveredRow = -1;
        for (int i = 0; i < inbox.Count; i++)
        {
            int ry = listY + i * (ROW_H + ROW_SPACING) - scrollY;
            if (ry + ROW_H < listY || ry > listY + listH) continue;
            if (mx >= listX && mx <= listX + listW && my >= ry && my <= ry + ROW_H)
            {
                hoveredRow = i;
                break;
            }
        }

        // Bottoni in basso
        int btnY = panelY + panelH - 30;
        int btnH = 22;
        int totalW = panelW - 16;
        int btnW = (totalW - 8) / 3;

        int claimAllX = panelX + 8;
        int clearX = claimAllX + btnW + 4;
        int closeX = clearX + btnW + 4;

        bool hasUnclaimed = false;
        bool hasClaimed = false;
        foreach (MailMessage m in inbox)
        {
            if (m.claimed) hasClaimed = true;
            else hasUnclaimed = true;
        }

        claimAllHovered = hasUnclaimed && mx >= claimAllX && mx <= claimAllX + btnW && my >= btnY && my <= btnY + btnH;
        clearHovered = hasClaimed && mx >= clearX && mx <= clearX + btnW && my >= btnY && my <= btnY + btnH;
        closeHovered = mx >= closeX && mx <= closeX + btnW && my >= btnY && my <= btnY + btnH;

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (closeHovered) { Hide(); return; }

            if (claimAllHovered)
            {
                List<MailReward> all = MailSystem.ClaimAll();
                if (all.Count > 0 && Game.guiRewardPopup != null)
                    Game.guiRewardPopup.Show(all);
                return;
            }

            if (clearHovered)
            {
                MailSystem.ClearClaimed();
                int contentH = MailSystem.Inbox.Count * (ROW_H + ROW_SPACING);
                int maxScroll = Math.Max(0, contentH - listH);
                scrollY = Math.Clamp(scrollY, 0, maxScroll);
                return;
            }

            if (hoveredRow >= 0 && hoveredRow < inbox.Count)
            {
                MailMessage mail = inbox[hoveredRow];
                if (!mail.claimed)
                {
                    List<MailReward> applied = MailSystem.ClaimMail(mail);
                    if (applied.Count > 0 && Game.guiRewardPopup != null)
                        Game.guiRewardPopup.Show(applied);
                }
                return;
            }

            bool insidePanel = mx >= panelX && mx <= panelX + panelW &&
                               my >= panelY && my <= panelY + panelH;
            if (!insidePanel) Hide();
        }
    }

    public override void Draw()
    {
        if (!isVisible) return;

        Graphics.DrawRectangle(0, 0, sw, sh, overlayColor);

        var (panelX, panelY, panelW, panelH) = GetPanelRect();

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, panelH), 0.1f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelW, panelH), 0.1f, 8, 2, panelBorder);

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, HEADER_H), 0.25f, 8, headerBg);

        string title = $"POSTA ({MailSystem.UnreadCount})";
        int titleW = title.Length * 7;
        Graphics.DrawText(title, panelX + (panelW - titleW) / 2, panelY + 6, 14, textColor);

        DrawList();
        DrawBottomButtons();
    }

    private void DrawList()
    {
        var (listX, listY, listW, listH) = GetListRect();
        List<MailMessage> inbox = MailSystem.Inbox;

        if (inbox.Count == 0)
        {
            string empty = "Nessuna posta.";
            int ew = empty.Length * 5;
            Graphics.DrawText(empty, listX + (listW - ew) / 2, listY + listH / 2 - 5, 10, mutedTextColor);
            return;
        }

        Graphics.BeginScissorMode(listX, listY, listW, listH);

        for (int i = 0; i < inbox.Count; i++)
        {
            int ry = listY + i * (ROW_H + ROW_SPACING) - scrollY;
            if (ry + ROW_H < listY - 4 || ry > listY + listH + 4) continue;

            MailMessage mail = inbox[i];
            DrawRow(listX, ry, listW, mail, i == hoveredRow);
        }

        Graphics.EndScissorMode();

        // Scrollbar
        int contentH = inbox.Count * (ROW_H + ROW_SPACING);
        if (contentH > listH)
        {
            int sbX = listX + listW - 3;
            int sbH = Math.Max(12, listH * listH / contentH);
            int sbY = listY + (int)((float)scrollY / (contentH - listH) * (listH - sbH));
            Graphics.DrawRectangle(sbX, sbY, 2, sbH, new Color(120, 140, 100, 200));
        }
    }

    private void DrawRow(int x, int y, int w, MailMessage mail, bool hovered)
    {
        Color bg = mail.claimed ? rowClaimedBg : (hovered ? rowHoverBg : rowBg);
        Color border = mail.claimed ? rowClaimedBorder : rowBorder;

        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, ROW_H), 0.2f, 6, bg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, w, ROW_H), 0.2f, 6, 1, border);

        int iconSize = 8;
        int iconX = x + 8;
        int iconY = y + (ROW_H - iconSize) / 2;
        Color iconColor = mail.claimed ? statusClaimedColor : statusNewColor;
        Graphics.DrawRectangleRounded(new Rectangle(iconX, iconY, iconSize, iconSize), 1f, 6, iconColor);

        int textX = x + 22;
        Color titleColor = mail.claimed ? mutedTextColor : textColor;
        Color descColor = mail.claimed ? mutedTextColor : subTextColor;

        Graphics.DrawText(mail.title, textX, y + 4, 11, titleColor);

        string meta = $"{mail.senderName}  -  {FormatDate(mail.receivedAt)}";
        Graphics.DrawText(meta, textX, y + 16, 8, descColor);

        string desc = mail.description;
        if (desc.Length > 42) desc = desc.Substring(0, 40) + "...";
        Graphics.DrawText(desc, textX, y + 26, 8, descColor);

        string statusText = mail.claimed ? "Riscattato" : "Nuovo";
        Color statusColor = mail.claimed ? statusClaimedColor : statusNewColor;
        int statusW = statusText.Length * 5;
        Graphics.DrawText(statusText, x + w - statusW - 8, y + 4, 9, statusColor);
    }

    private void DrawBottomButtons()
    {
        var (panelX, panelY, panelW, panelH) = GetPanelRect();

        int btnY = panelY + panelH - 30;
        int btnH = 22;
        int totalW = panelW - 16;
        int btnW = (totalW - 8) / 3;

        int claimAllX = panelX + 8;
        int clearX = claimAllX + btnW + 4;
        int closeX = clearX + btnW + 4;

        List<MailMessage> inbox = MailSystem.Inbox;
        bool hasUnclaimed = false;
        bool hasClaimed = false;
        foreach (MailMessage m in inbox)
        {
            if (m.claimed) hasClaimed = true;
            else hasUnclaimed = true;
        }

        DrawBottomButton(claimAllX, btnY, btnW, btnH, "Riscatta tutto",
            hasUnclaimed, claimAllHovered, claimAllColor, claimAllHoverColor);

        DrawBottomButton(clearX, btnY, btnW, btnH, "Elimina letti",
            hasClaimed, clearHovered, clearColor, clearHoverColor);

        DrawBottomButton(closeX, btnY, btnW, btnH, "Chiudi",
            true, closeHovered, closeColor, closeHoverColor);
    }

    private void DrawBottomButton(int x, int y, int w, int h, string label,
        bool enabled, bool hovered, Color baseColor, Color hoverColor)
    {
        Color bg = !enabled ? buttonDisabledColor : (hovered ? hoverColor : baseColor);
        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), 0.3f, 8, bg);

        int textW = label.Length * 5;
        Color lc = enabled ? textColor : mutedTextColor;
        Graphics.DrawText(label, x + (w - textW) / 2, y + 6, 10, lc);
    }

    private static string FormatDate(DateTime dt)
    {
        DateTime today = DateTime.Now.Date;
        if (dt.Date == today) return "oggi";
        if (dt.Date == today.AddDays(-1)) return "ieri";
        return dt.ToString("dd/MM");
    }
}
