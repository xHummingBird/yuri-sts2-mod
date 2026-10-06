using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Cards.Common;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Powers;

public class OverlimitPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner.Creature != Owner)
        {
            return false;
        }

        if (card is not CrushingEagle)
        {
            return false;
        }

        if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
        {
            return false;
        }

        // ArcaneArteBoostPower takes priority, preventing both powers
        // from independently reducing Crushing Eagle's cost.
        if (Owner.GetPower<ArcaneArteBoostPower>() != null)
        {
            return false;
        }

        modifiedCost = decimal.Max(0m, originalCost - 1m);
        return modifiedCost != originalCost;
    }
    
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;
        
        if (Owner.GetPowerAmount<OverlimitPower>() <= 1)
            OverlimitManager.SetOverlimit(Owner.Player, 0);
        
        await PowerCmd.Decrement(this);
    }
}