using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Powers;

public sealed class ComboForcePower : YuriPower
{
    private sealed class Data
    {
        public int AttacksPlayedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }

        if (cardPlay.IsAutoPlay)
        {
            return;
        }

        if (!cardPlay.IsLastInSeries)
        {
            return;
        }

        if (cardPlay.Card.Type is not CardType.Attack)
        {
            return;
        }

        Data data = GetInternalData<Data>();

        if (data.AttacksPlayedThisTurn >= Amount)
        {
            return;
        }

        data.AttacksPlayedThisTurn++;

        IReadOnlyList<CardModel> baseArtePool =
            Owner.Player.Character.CardPool
                .GetUnlockedCards(
                    Owner.Player.UnlockState,
                    Owner.Player.RunState.CardMultiplayerConstraint)
                .Where(card => card is IBaseArte)
                .ToList();

        if (baseArtePool.Count == 0)
        {
            return;
        }

        Rng combatCardGeneration =
            Owner.Player.RunState.Rng.CombatCardGeneration;

        CardModel randomBaseArte =
            CardFactory.GetDistinctForCombat(
                Owner.Player,
                baseArtePool,
                1,
                combatCardGeneration)
            .First();

        Flash();

        await CardPileCmd.AddGeneratedCardsToCombat(
            [randomBaseArte],
            PileType.Hand,
            Owner.Player);
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

        GetInternalData<Data>().AttacksPlayedThisTurn = 0;

        return Task.CompletedTask;
    }
}