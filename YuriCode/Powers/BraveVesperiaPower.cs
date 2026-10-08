using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Yuri.YuriCode.Powers;

public class BraveVesperiaPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [
            new DynamicVar("Overlimit", 0m)
        ];


    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != base.Owner.Player)
        {
            return amount;
        }
        return amount + (decimal)base.Amount;
    }
    
    public void IncrementOverlimitPerTurn()
    {
        AssertMutable();
        base.DynamicVars["Overlimit"].BaseValue += 5;
    }
}