using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Mechanics;

public partial class OverlimitDisplayOverlay : Control
{
    public static OverlimitDisplayOverlay? Instance { get; private set; }

    private Control? _overlimitDisplay;
    private TextureRect? _overlimitTexture;
    private RichTextLabel? _label;

    private Player? _player;
    private IHoverTip? _hoverTip;

    private int _lastValue = -1;

    private Tween? _popTween;

    private bool _exiting;

    private const int OverlimitMax = 100;

    private static readonly Color OverlimitGainColor =
        new Color(0.35f, 0.8f, 1f);

    private static readonly Color OverlimitFullColor =
        new Color(1f, 0.75f, 0.25f);

    public override void _Ready()
    {
        Instance = this;

        Name = "OverlimitDisplayOverlay";

        MouseFilter = MouseFilterEnum.Pass;

        CallDeferred(nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        /*
         * Wait for CombatManager and the local player.
         * This prevents a race condition with NEnergyCounter._Ready().
         */
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
                SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        if (_exiting || !IsInsideTree())
            return;

        var scene = GD.Load<PackedScene>(
            "res://Yuri/scenes/overlimit_display.tscn");

        if (scene == null)
        {
            GD.PushError(
                "[Yuri Overlimit] Failed to load " +
                "res://Yuri/scenes/overlimit_display.tscn");

            QueueFree();
            return;
        }

        _overlimitDisplay =
            scene.Instantiate<Control>();

        AddChild(_overlimitDisplay);

        _overlimitDisplay.MouseFilter =
            MouseFilterEnum.Pass;

        /*
         * Use Killia's alignment display location.
         */
        _overlimitDisplay.SetAnchorsPreset(
            LayoutPreset.BottomLeft);

        _overlimitDisplay.Position =
            new Vector2(-40f, 80f);

        _overlimitDisplay.Visible = true;

        _overlimitTexture =
            _overlimitDisplay.GetNodeOrNull<TextureRect>(
                "Overlimit");

        /*
         * Your scene currently calls this AlignmentLabel.
         * Make sure Unique Name in Owner is enabled.
         */
        _label =
            _overlimitDisplay.GetNodeOrNull<RichTextLabel>(
                "%OverlimitLabel");

        if (_overlimitTexture == null)
        {
            GD.PushError(
                "[Yuri Overlimit] Could not find Overlimit TextureRect");

            QueueFree();
            return;
        }

        if (_label == null)
        {
            GD.PushError(
                "[Yuri Overlimit] Could not find %OverlimitLabel");

            QueueFree();
            return;
        }

        _label.TreeExiting += OnLabelTreeExiting;

        var font = GD.Load<FontFile>(
            "res://Yuri/font/Reggae Std B.otf");

        if (font != null)
        {
            _label.AddThemeFontOverride(
                "font",
                font);

            _label.AddThemeFontOverride(
                "normal_font",
                font);
        }
        else
        {
            GD.PushWarning(
                "[Yuri Overlimit] Failed to load " +
                "res://Yuri/font/Reggae Std B.otf");
        }

        _label.AddThemeColorOverride(
            "default_color",
            Colors.White);

        _label.AddThemeColorOverride(
            "font_outline_color",
            new Color(0.2f, 0.2f, 0.2f));

        _label.AddThemeConstantOverride(
            "outline_size",
            10);

        _label.AddThemeFontSizeOverride(
            "normal_font_size",
            26);

        _label.Position +=
            new Vector2(0f, 31f);

        _label.PivotOffset =
            _label.Size * 0.5f;

        _overlimitTexture.MouseFilter =
            MouseFilterEnum.Ignore;

        _label.MouseFilter =
            MouseFilterEnum.Ignore;

        /*
         * Hover the entire medallion rather than only the number.
         */
        _overlimitDisplay.Connect(
            SignalName.MouseEntered,
            Callable.From(OnHovered));

        _overlimitDisplay.Connect(
            SignalName.MouseExited,
            Callable.From(OnUnhovered));

        MouseFilter =
            MouseFilterEnum.Pass;

        _hoverTip =
            YuriStaticHoverTips.Overlimit;

        var data =
            OverlimitManager.GetDataForUI(_player);

        data.OnOverlimitChanged +=
            OnOverlimitChanged;

        int currentValue =
            OverlimitManager.GetOverlimit(_player);

        UpdateDisplay(
            currentValue,
            allowAnimation: false);
    }

    private void OnLabelTreeExiting()
    {
        KillTween(ref _popTween);

        _label = null;
    }

    private void OnOverlimitChanged(int value)
    {
        UpdateDisplay(
            value,
            allowAnimation: true);
    }

    private void UpdateDisplay(
        int value,
        bool allowAnimation)
    {
        if (_exiting)
            return;

        var label = _label;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label))
            return;

        if (label.IsQueuedForDeletion())
            return;

        value = Math.Clamp(
            value,
            0,
            OverlimitMax);

        try
        {
            label.Text =
                $"[center]{value}[/center]";
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        bool isMaxed =
            value >= OverlimitMax;

        if (_lastValue >= 0 &&
            value > _lastValue &&
            allowAnimation)
        {
            PlayGainPop(isMaxed);
        }
        else
        {
            label.Scale =
                Vector2.One;

            label.Modulate =
                isMaxed
                    ? OverlimitFullColor
                    : Colors.White;
        }

        _lastValue = value;
    }

    private void PlayGainPop(bool stayFullColor)
    {
        if (_exiting)
            return;

        var label = _label;

        if (label == null)
            return;

        if (!GodotObject.IsInstanceValid(label))
            return;

        if (label.IsQueuedForDeletion())
            return;

        KillTween(ref _popTween);

        label.PivotOffset =
            label.Size * 0.5f;

        label.Scale =
            Vector2.One;

        label.Modulate =
            stayFullColor
                ? OverlimitFullColor
                : OverlimitGainColor;

        _popTween =
            label.CreateTween();

        _popTween
            .TweenProperty(
                label,
                "scale",
                new Vector2(1.25f, 1.25f),
                0.10f)
            .SetTrans(
                Tween.TransitionType.Quad)
            .SetEase(
                Tween.EaseType.Out);

        _popTween
            .TweenProperty(
                label,
                "scale",
                Vector2.One,
                0.40f)
            .SetTrans(
                Tween.TransitionType.Quad)
            .SetEase(
                Tween.EaseType.Out);

        _popTween
            .Parallel()
            .TweenProperty(
                label,
                "modulate",
                stayFullColor
                    ? OverlimitFullColor
                    : Colors.White,
                0.40f)
            .SetTrans(
                Tween.TransitionType.Quad)
            .SetEase(
                Tween.EaseType.Out);
    }

    private void OnHovered()
    {
        if (_exiting)
            return;

        if (_hoverTip == null)
            return;

        NHoverTipSet.Clear();

        var tip =
            NHoverTipSet.CreateAndShow(
                this,
                _hoverTip);

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

    private static void KillTween(
        ref Tween? tween)
    {
        if (tween != null &&
            GodotObject.IsInstanceValid(tween))
        {
            tween.Kill();
        }

        tween = null;
    }

    public override void _ExitTree()
    {
        _exiting = true;

        KillTween(ref _popTween);

        if (_player != null)
        {
            var data =
                OverlimitManager.GetDataForUI(
                    _player);

            data.OnOverlimitChanged -=
                OnOverlimitChanged;
        }

        NHoverTipSet.Remove(this);

        _label = null;
        _overlimitTexture = null;
        _overlimitDisplay = null;
        _hoverTip = null;
        _player = null;

        if (Instance == this)
            Instance = null;
    }
}

[HarmonyPatch(
    typeof(NEnergyCounter),
    nameof(NEnergyCounter._Ready))]
public static class YuriOverlimitDisplayOverlayPatch
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

        if (__instance
            .GetNodeOrNull<OverlimitDisplayOverlay>(
                "OverlimitDisplayOverlay") != null)
        {
            return;
        }

        var overlay =
            new OverlimitDisplayOverlay
            {
                Name =
                    "OverlimitDisplayOverlay"
            };

        __instance.AddChild(overlay);
    }
}