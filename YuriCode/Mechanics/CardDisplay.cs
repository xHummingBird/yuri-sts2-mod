using Godot;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Mechanics;

public partial class CardDisplay : Control
{
    private RichTextLabel _label = null!;

    private static readonly Texture2D BaseArte =
        GD.Load<Texture2D>(
            "res://Yuri/images/charui/baseArte.png"
        );

    private static readonly Texture2D ArcaneArte =
        GD.Load<Texture2D>(
            "res://Yuri/images/charui/arcaneArte.png"
        );

    private TextureRect _icon = null!;

    public override void _Ready()
    {
        _icon =
            GetNode<TextureRect>("Icon");

        _label =
            GetNode<RichTextLabel>("%IconLabel");

        var font = GD.Load<Font>(
            "res://themes/kreon_bold_shared.tres"
        );

        if (font != null)
        {
            _label.AddThemeFontOverride(
                "font",
                font
            );

            _label.AddThemeFontOverride(
                "normal_font",
                font
            );

            _label.OffsetTop = 13;
            _label.OffsetBottom = 0;
        }

        _label.AddThemeFontSizeOverride(
            "normal_font_size",
            32
        );

        _label.AddThemeColorOverride(
            "default_color",
            Colors.White
        );

        _label.AddThemeColorOverride(
            "font_outline_color",
            Colors.Black
        );

        _label.AddThemeConstantOverride(
            "outline_size",
            8
        );

        _label.FitContent = true;
        _label.BbcodeEnabled = true;

        _label.Text = "[center][/center]";
    }

    public void SetCard(CardModel model)
    {
        switch (model)
        {
            // Arcane first in case Arcane Artes also implement IBaseArte.
            case IArcaneArte arcaneArte:
                _icon.Texture = ArcaneArte;

                _label.Visible = true;

                _label.Modulate = Colors.White;

                _label.Text =
                    $"[center]{arcaneArte.ComboGain}[/center]";
                break;

            case IBaseArte baseArte:
                _icon.Texture = BaseArte;

                _label.Visible = true;

                _label.Modulate = Colors.White;

                _label.Text =
                    $"[center]{baseArte.ComboGain}[/center]";
                break;

            default:
                _icon.Texture = null;
                _label.Visible = false;
                break;
        }
    }
}