using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Powers;

public sealed class DarkLionPower : YuriPower
{
    private sealed class Data
    {
        public readonly Dictionary<CardModel, int> AmountsForPlayedCards = [];
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return Task.CompletedTask;
        }

        if (cardPlay.Card is not IArcaneArte)
        {
            return Task.CompletedTask;
        }

        GetInternalData<Data>().AmountsForPlayedCards[cardPlay.Card] = Amount;

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        if (!GetInternalData<Data>().AmountsForPlayedCards.Remove(
                cardPlay.Card,
                out int damage))
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        await Cmd.CustomScaledWait(0.1f, 0.2f);

        Flash();
        
            await CreatureCmd.Damage(
                choiceContext,
                CombatState.HittableEnemies,
                damage,
                ValueProp.Unpowered,
                Owner
            );
        
    }
}