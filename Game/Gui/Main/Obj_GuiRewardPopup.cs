using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;

namespace Plants;

public class Obj_GuiRewardPopup : GameElement
{
    private bool isVisible = false;
    private List<MailReward> rewards = new();
    private bool closeHovered = false;

    private readonly Color overlayColor = new Color(0, 0, 0, 180);
    private readonly Color panelBg = new Color(40, 45, 35, 250);
    private readonly Color panelBorder = new Color(200, 200, 120, 255);
    private readonly Color headerBg = new Color(90, 80, 35, 255);
    private readonly Color textColor = new Color(240, 240, 230, 255);
    private readonly Color rewardBg = new Color(55, 60, 45, 255);
    private readonly Color rewardBorder = new Color(140, 140, 90, 255);
    private readonly Color rewardTextColor = new Color(230, 230, 200, 255);
    private readonly Color buttonColor = new Color(90, 130, 70, 255);
    private readonly Color buttonHoverColor = new Color(120, 170, 90, 255);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    public Obj_GuiRewardPopup()
    {
        this.guiLayer = true;
        this.depth = -3000;
        this.persistent = true;
    }

    public void Show(List<MailReward> rewards)
    {
        if (rewards == null || rewards.Count == 0) return;
        this.rewards = AggregateRewards(rewards);
        isVisible = true;
    }

    public bool IsVisible => isVisible;

    public void Hide()
    {
        isVisible = false;
        rewards.Clear();
    }

    private static List<MailReward> AggregateRewards(List<MailReward> input)
    {
        Dictionary<MailRewardType, int> sums = new();
        foreach (MailReward r in input)
        {
            if (!sums.ContainsKey(r.type)) sums[r.type] = 0;
            sums[r.type] += r.amount;
        }
        List<MailReward> result = new();
        foreach (var kv in sums)
            result.Add(new MailReward { type = kv.Key, amount = kv.Value });
        return result;
    }

    public override void Update()
    {
        if (!isVisible) return;

        (int panelX, int panelY, int panelW, int panelH) = GetPanelRect();
        int btnW = 90;
        int btnH = 24;
        int btnX = panelX + (panelW - btnW) / 2;
        int btnY = panelY + panelH - btnH - 10;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        closeHovered = mx >= btnX && mx <= btnX + btnW && my >= btnY && my <= btnY + btnH;

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (closeHovered) { Hide(); return; }
            bool inside = mx >= panelX && mx <= panelX + panelW &&
                          my >= panelY && my <= panelY + panelH;
            if (!inside) Hide();
        }
    }

    private (int x, int y, int w, int h) GetPanelRect()
    {
        int rowH = 24;
        int panelW = 240;
        int panelH = 70 + rewards.Count * (rowH + 4) + 40;
        int panelX = (sw - panelW) / 2;
        int panelY = (sh - panelH) / 2;
        return (panelX, panelY, panelW, panelH);
    }

    public override void Draw()
    {
        if (!isVisible) return;

        Graphics.DrawRectangle(0, 0, sw, sh, overlayColor);

        (int panelX, int panelY, int panelW, int panelH) = GetPanelRect();

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, panelH), 0.1f, 8, panelBg);
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelW, panelH), 0.1f, 8, 2, panelBorder);

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, 22), 0.25f, 8, headerBg);

        string title = "RICOMPENSE";
        int titleW = title.Length * 7;
        Graphics.DrawText(title, panelX + (panelW - titleW) / 2, panelY + 6, 14, textColor);

        int rowH = 24;
        int rowY = panelY + 32;
        int rowX = panelX + 12;
        int rowW = panelW - 24;

        foreach (MailReward reward in rewards)
        {
            Graphics.DrawRectangleRounded(
                new Rectangle(rowX, rowY, rowW, rowH), 0.3f, 6, rewardBg);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(rowX, rowY, rowW, rowH), 0.3f, 6, 1, rewardBorder);

            string label = MailSystem.FormatReward(reward);
            Graphics.DrawText(label, rowX + 10, rowY + 7, 11, rewardTextColor);

            rowY += rowH + 4;
        }

        int btnW = 90;
        int btnH = 24;
        int btnX = panelX + (panelW - btnW) / 2;
        int btnY = panelY + panelH - btnH - 10;

        Color bc = closeHovered ? buttonHoverColor : buttonColor;
        Graphics.DrawRectangleRounded(new Rectangle(btnX, btnY, btnW, btnH), 0.3f, 8, bc);
        string btnText = "Ottimo!";
        int btnTextW = btnText.Length * 6;
        Graphics.DrawText(btnText, btnX + (btnW - btnTextW) / 2, btnY + 6, 12, textColor);
    }
}
