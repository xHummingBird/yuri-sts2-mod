using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Powers;

public sealed class VigilantePower : YuriPower
{
    private sealed class Data
    {
        public int BaseArtesPlayedThisTurn;
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

        if (cardPlay.Card is not IBaseArte)
        {
            return;
        }

        Data data = GetInternalData<Data>();

        if (data.BaseArtesPlayedThisTurn >= 1)
        {
            return;
        }

        data.BaseArtesPlayedThisTurn++;

        Flash();

        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player, false);
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner))
        {
            GetInternalData<Data>().BaseArtesPlayedThisTurn = 0;
        }

        return Task.CompletedTask;
    }
}