using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Yuri.YuriCode.Cards.Ancient;
using Yuri.YuriCode.Cards.Common;
using Yuri.YuriCode.Cards.Rare;
using Yuri.YuriCode.Cards.Uncommon;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Powers;

public class ArcaneArteBoostPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner)
            return;

        if (cardPlay.Card.Type != CardType.Attack)
            return;
        
        if (cardPlay.Card is IArcaneArte)
        {
            if (Owner.HasPower<KingOfCombosPower>())
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Owner.GetPowerAmount<KingOfCombosPower>(), null, null);
        }

        // Base Artes keep the buff.
        if (cardPlay.Card is IBaseArte)
            return;
        
        if (cardPlay.Card is ThreeHitCombo)
            return;

        if (cardPlay.Card is DragonSwarm)
            return;

        if (cardPlay.Card is StepCombo)
            return;
        
        await PowerCmd.Remove(this);
    }
    
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

        modifiedCost = decimal.Max(0m, originalCost - 1m);
        return modifiedCost != originalCost;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
            return;

        await PowerCmd.Remove(this);
    }
}