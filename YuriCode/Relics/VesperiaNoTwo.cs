using MegaCrit.Sts2.Core.Entities.Relics;

namespace Yuri.YuriCode.Relics;

public class VesperiaNoTwo : ArteRelicBase
{
    public override RelicRarity Rarity =>
        RelicRarity.Ancient;
    
    protected override int BaseOverlimitPerTurn => 7;
}