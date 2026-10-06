using System;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Mechanics;

public partial class ComboDisplayOverlay : Control
{
    public static ComboDisplayOverlay? Instance { get; private set; }

    private Control? _comboDisplay;
    private RichTextLabel? _comboLabel;
    private Player? _player;

    private IHoverTip? _comboHoverTip;

    private int _lastCombo = -1;

    private Tween? _comboPopTween;

    private bool _exiting;

    private static readonly Color ComboGainColor =
        new Color(0.55f, 0.85f, 1f);

    public override void _Ready()
    {
        Instance = this;

        Name = "ComboDisplayOverlay";

        MouseFilter = MouseFilterEnum.Pass;

        CallDeferred(nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        for (int i = 0; i < 60; i++)
        {
            if (_exiting || !IsInsideTree())
                return;

            var state =
                CombatManager.Instance?.DebugOnlyGetState();

            var player = state?.Players
                .FirstOrDefault(p => LocalContext.IsMe(p));

            if (player != null)
            {
                if (player.Character is not Character.Yuri)
                {
                    QueueFree();
                    return;
                }

                _player = player;
                break;
            }

            var tree = GetTree();

            if (tree == null)
                return;

            await ToSignal(
                tree,
                SceneTree.SignalName.ProcessFrame
            );
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        var scene = GD.Load<PackedScene>(
            "res://Yuri/scenes/combo_display.tscn"
        );

        if (scene == null)
        {
            GD.PushError(
                "[Yuri Combo] Failed to load combo_display.tscn"
            );

            QueueFree();
            return;
        }

        _comboDisplay =
            scene.Instantiate<Control>();

        AddChild(_comboDisplay);

        _comboDisplay.SetAnchorsPreset(
            LayoutPreset.TopRight
        );

        _comboDisplay.Position =
            new Vector2(-120, -50);

        _comboLabel =
            _comboDisplay.GetNodeOrNull<RichTextLabel>(
                "%ComboLabel"
            );

        if (_comboLabel == null)
        {
            GD.PushError(
                "[Yuri Combo] Could not find %ComboLabel"
            );

            QueueFree();
            return;
        }

        var font =
            GD.Load<FontFile>(
                "res://Yuri/font/Reggae Std B.otf"
            );

        if (font != null)
        {
            _comboLabel.AddThemeFontOverride(
                "font",
                font
            );

            _comboLabel.AddThemeFontOverride(
                "normal_font",
                font
            );
        }

        _comboLabel.BbcodeEnabled = true;

        _comboLabel.AddThemeColorOverride(
            "default_color",
            Colors.White
        );

        _comboLabel.AddThemeColorOverride(
            "font_outline_color",
            new Color(0.15f, 0.15f, 0.15f)
        );

        _comboLabel.AddThemeConstantOverride(
            "outline_size",
            10
        );

        _comboLabel.AddThemeFontSizeOverride(
            "normal_font_size",
            40
        );

        _comboHoverTip =
            YuriStaticHoverTips.Combo;

        _comboLabel.MouseFilter =
            MouseFilterEnum.Stop;

        _comboLabel.MouseEntered +=
            OnComboHovered;

        _comboLabel.MouseExited +=
            OnUnhovered;
    }

    public override void _Process(double delta)
    {
        if (_exiting)
            return;

        if (_player == null)
            return;

        if (!CombatManager.Instance.IsInProgress)
            return;

        RefreshDisplay();
    }

    private ArteRelicBase? GetArteRelic()
    {
        return _player?.Relics
            .OfType<ArteRelicBase>()
            .FirstOrDefault();
    }

    private int GetCurrentCombo()
    {
        return GetArteRelic()?.Combo ?? 0;
    }

    private void RefreshDisplay()
    {
        UpdateComboDisplay(
            GetCurrentCombo()
        );
    }

    private void UpdateComboDisplay(int value)
    {
        var label = _comboLabel;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label))
            return;

        if (label.IsQueuedForDeletion())
            return;

        try
        {
            label.Text =
                $"[center]{value} Hits[/center]";
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (_lastCombo >= 0 &&
            value > _lastCombo)
        {
            PlayPop();
        }
        else
        {
            label.Scale = Vector2.One;
            label.Modulate = Colors.White;
        }

        _lastCombo = value;
    }

    private void PlayPop()
    {
        var label = _comboLabel;

        if (label == null)
            return;

        if (_comboPopTween != null &&
            GodotObject.IsInstanceValid(_comboPopTween))
        {
            _comboPopTween.Kill();
        }

        label.Scale =
            Vector2.One;

        label.Modulate =
            ComboGainColor;

        _comboPopTween =
            label.CreateTween();

        _comboPopTween
            .TweenProperty(
                label,
                "scale",
                new Vector2(1.35f, 1.35f),
                0.10f
            )
            .SetTrans(
                Tween.TransitionType.Quad
            )
            .SetEase(
                Tween.EaseType.Out
            );

        _comboPopTween
            .TweenProperty(
                label,
                "scale",
                Vector2.One,
                0.35f
            )
            .SetTrans(
                Tween.TransitionType.Quad
            )
            .SetEase(
                Tween.EaseType.Out
            );

        _comboPopTween
            .Parallel()
            .TweenProperty(
                label,
                "modulate",
                Colors.White,
                0.35f
            );
    }

    private void OnComboHovered()
    {
        if (_comboHoverTip == null)
            return;

        NHoverTipSet.Clear();

        var tip =
            NHoverTipSet.CreateAndShow(
                this,
                _comboHoverTip
            );

        tip.GlobalPosition =
            GlobalPosition +
            new Vector2(-75f, -350f);

        tip.MouseFilter =
            MouseFilterEnum.Ignore;
    }

    private void OnUnhovered()
    {
        NHoverTipSet.Remove(this);
    }

    public override void _ExitTree()
    {
        _exiting = true;

        if (_comboPopTween != null &&
            GodotObject.IsInstanceValid(
                _comboPopTween))
        {
            _comboPopTween.Kill();
        }

        NHoverTipSet.Remove(this);

        _comboLabel = null;
        _comboDisplay = null;
        _player = null;

        if (Instance == this)
            Instance = null;
    }
}

[HarmonyPatch(
    typeof(NEnergyCounter),
    nameof(NEnergyCounter._Ready)
)]
public static class ComboDisplayOverlayPatch
{
    public static void Postfix(
        NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(__instance))
            return;

        if (__instance.IsQueuedForDeletion())
            return;

        if (__instance.GetNodeOrNull<ComboDisplayOverlay>(
                "ComboDisplayOverlay") != null)
        {
            return;
        }

        __instance.AddChild(
            new ComboDisplayOverlay
            {
                Name = "ComboDisplayOverlay"
            }
        );
    }
}