using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Yuri.YuriCode.Character;
using Yuri.YuriCode.Extensions;
using Godot;

namespace Yuri.YuriCode.Relics;

[Pool(typeof(YuriRelicPool))]
public abstract class YuriRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".BigRelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}