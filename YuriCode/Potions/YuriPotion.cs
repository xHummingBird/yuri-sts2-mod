using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Yuri.YuriCode.Character;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Potions;

[Pool(typeof(YuriPotionPool))]
public abstract class YuriPotion : CustomPotionModel
{
    protected string PotionFileName =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    protected string PotionOutlineFileName =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png";

    public override string? CustomPackedImagePath =>
        PotionFileName.PotionImagePath();

    public override string? CustomPackedOutlinePath =>
        PotionOutlineFileName.PotionImagePath();
}