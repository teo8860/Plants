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

    public void Hide() { isVisible = false; }
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
        InputGate.ConsumeMouse();

        if (Game.guiRewardPopup != null && Game.guiRewardPopup.IsVisible)
            return;

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

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
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

            var (panelX, panelY, panelW, panelH) = GetPanelRect();
            bool insidePanel = mx >= panelX && mx <= panelX + panelW &&
                               my >= panelY && my <= panelY + panelH;
            if (!insidePanel) Hide();
        }
    }

    public override void Draw()
    {
        if (!isVisible) return;

        Hud.Overlay(overlayColor);

        var (panelX, panelY, panelW, panelH) = GetPanelRect();

        Hud.Panel(panelX, panelY, panelW, panelH, new Hud.PanelOptions
        {
            Bg = panelBg, Border = panelBorder, Roundness = 0.1f
        });

        Hud.Panel(panelX, panelY, panelW, HEADER_H, new Hud.PanelOptions
        {
            Bg = headerBg, Roundness = 0.25f, BorderThickness = 0
        });

        string title = $"POSTA ({MailSystem.UnreadCount})";
        Hud.Label(panelX, panelY + 6, panelW, title, new Hud.LabelOptions
        {
            FontSize = 14, Color = textColor, Align = TextAlign.Center
        });

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
            int ew = GuiTheme.MeasureText(empty);
            GuiTheme.DrawText(empty, listX + (listW - ew) / 2, listY + listH / 2 - 5, 10, mutedTextColor);
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

        GuiTheme.DrawText(mail.title, textX, y + 4, 11, titleColor);

        string meta = $"{mail.senderName}  -  {FormatDate(mail.receivedAt)}";
        GuiTheme.DrawText(meta, textX, y + 16, 8, descColor);

        string desc = mail.description;
        if (desc.Length > 42) desc = desc.Substring(0, 40) + "...";
        GuiTheme.DrawText(desc, textX, y + 26, 8, descColor);

        string statusText = mail.claimed ? "Riscattato" : "Nuovo";
        Color statusColor = mail.claimed ? statusClaimedColor : statusNewColor;
        int statusW = GuiTheme.MeasureText(statusText, 9);
        GuiTheme.DrawText(statusText, x + w - statusW - 8, y + 4, 9, statusColor);
    }

    private void DrawBottomButtons()
    {
        var (panelX, panelY, panelW, panelH) = GetPanelRect();
        var (_, _, _, listH) = GetListRect();

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

        Hud.Button(claimAllX, btnY, btnW, btnH, "Riscatta tutto", () => {
            List<MailReward> all = MailSystem.ClaimAll();
            if (all.Count > 0 && Game.guiRewardPopup != null)
                Game.guiRewardPopup.Show(all);
        }, new Hud.ButtonOptions {
            Bg = claimAllColor, HoverBg = claimAllHoverColor,
            Disabled = !hasUnclaimed, DisabledBg = buttonDisabledColor,
            DisabledTextColor = mutedTextColor,
            TextColor = textColor, FontSize = 10, Roundness = 0.3f,
            BorderThickness = 0
        });

        Hud.Button(clearX, btnY, btnW, btnH, "Elimina letti", () => {
            MailSystem.ClearClaimed();
            int contentH = MailSystem.Inbox.Count * (ROW_H + ROW_SPACING);
            int maxScroll = Math.Max(0, contentH - listH);
            scrollY = Math.Clamp(scrollY, 0, maxScroll);
        }, new Hud.ButtonOptions {
            Bg = clearColor, HoverBg = clearHoverColor,
            Disabled = !hasClaimed, DisabledBg = buttonDisabledColor,
            DisabledTextColor = mutedTextColor,
            TextColor = textColor, FontSize = 10, Roundness = 0.3f,
            BorderThickness = 0
        });

        Hud.Button(closeX, btnY, btnW, btnH, "Chiudi", () => Hide(), new Hud.ButtonOptions {
            Bg = closeColor, HoverBg = closeHoverColor,
            TextColor = textColor, FontSize = 10, Roundness = 0.3f,
            BorderThickness = 0
        });
    }

    private static string FormatDate(DateTime dt)
    {
        DateTime today = DateTime.Now.Date;
        if (dt.Date == today) return "oggi";
        if (dt.Date == today.AddDays(-1)) return "ieri";
        return dt.ToString("dd/MM");
    }
}
