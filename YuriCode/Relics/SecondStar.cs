using MegaCrit.Sts2.Core.Entities.Relics;

namespace Yuri.YuriCode.Relics;

public class SecondStar : ArteRelicBase
{
    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    protected override int BaseOverlimitPerTurn => 5;

    protected override int OverlimitPerCard => 2;
}