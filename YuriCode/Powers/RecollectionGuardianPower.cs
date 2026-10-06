using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Yuri.YuriCode.Powers;

public sealed class RecollectionGuardianPower : YuriPower
{
    private sealed class Data
    {
        public int CardsPlayedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (ShouldSkip(card))
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }

    public override bool TryModifyStarCost(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (ShouldSkip(card))
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner &&
            !cardPlay.IsAutoPlay &&
            cardPlay.IsLastInSeries)
        {
            GetInternalData<Data>().CardsPlayedThisTurn++;
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner))
        {
            return Task.CompletedTask;
        }

        GetInternalData<Data>().CardsPlayedThisTurn = 0;

        return Task.CompletedTask;
    }

    private bool ShouldSkip(CardModel card)
    {
        if (card.Owner.Creature != Owner)
        {
            return true;
        }

        if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
        {
            return true;
        }

        return GetInternalData<Data>().CardsPlayedThisTurn >= Amount;
    }
}