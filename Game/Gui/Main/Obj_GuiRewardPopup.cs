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
        InputGate.ConsumeMouse();

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            (int panelX, int panelY, int panelW, int panelH) = GetPanelRect();
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();
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

        Hud.Overlay(overlayColor);

        (int panelX, int panelY, int panelW, int panelH) = GetPanelRect();

        Hud.Panel(panelX, panelY, panelW, panelH, new Hud.PanelOptions
        {
            Bg = panelBg, Border = panelBorder, Roundness = 0.1f
        });

        Hud.Panel(panelX, panelY, panelW, 22, new Hud.PanelOptions
        {
            Bg = headerBg, Roundness = 0.25f, BorderThickness = 0
        });

        Hud.Label(panelX, panelY + 6, panelW, "RICOMPENSE", new Hud.LabelOptions
        {
            FontSize = 14, Color = textColor, Align = TextAlign.Center
        });

        int rowH = 24;
        int rowY = panelY + 32;
        int rowX = panelX + 12;
        int rowW = panelW - 24;

        foreach (MailReward reward in rewards)
        {
            Hud.Panel(rowX, rowY, rowW, rowH, new Hud.PanelOptions
            {
                Bg = rewardBg, Border = rewardBorder, Roundness = 0.3f, BorderThickness = 1
            });

            Hud.Label(rowX + 10, rowY + 7, MailSystem.FormatReward(reward), new Hud.LabelOptions
            {
                FontSize = 11, Color = rewardTextColor
            });

            rowY += rowH + 4;
        }

        int btnW = 90;
        int btnH = 24;
        int btnX = panelX + (panelW - btnW) / 2;
        int btnY = panelY + panelH - btnH - 10;

        Hud.Button(btnX, btnY, btnW, btnH, "Ottimo!", () => Hide(), new Hud.ButtonOptions
        {
            Bg = buttonColor, HoverBg = buttonHoverColor,
            TextColor = textColor, FontSize = 12, Roundness = 0.3f,
            BorderThickness = 0
        });
    }
}
