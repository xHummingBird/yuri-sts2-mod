using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Yuri.YuriCode.Extensions;

public static class YuriStaticHoverTips
{
    public static readonly IHoverTip Overlimit = new HoverTip(
        new LocString(
            "static_hover_tips",
            "YURI_OVERLIMIT.title"
        ),
        new LocString(
            "static_hover_tips",
            "YURI_OVERLIMIT.description"
        )
    );

    public static readonly IHoverTip BaseArte = new HoverTip(
        new LocString(
            "static_hover_tips",
            "YURI_BASE_ARTE.title"
        ),
        new LocString(
            "static_hover_tips",
            "YURI_BASE_ARTE.description"
        )
    );

    public static readonly IHoverTip ArcaneArte = new HoverTip(
        new LocString(
            "static_hover_tips",
            "YURI_ARCANE_ARTE.title"
        ),
        new LocString(
            "static_hover_tips",
            "YURI_ARCANE_ARTE.description"
        )
    );

    public static readonly IHoverTip Combo = new HoverTip(
        new LocString(
            "static_hover_tips",
            "YURI_COMBO.title"
        ),
        new LocString(
            "static_hover_tips",
            "YURI_COMBO.description"
        )
    );
}