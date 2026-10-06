using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Powers;

public sealed class TrueKnightPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }

        if (cardPlay.Card is not IBaseArte)
        {
            return;
        }

        Flash();

        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            Owner,
            Amount,
            Owner,
            null
        );
    }
}