using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Yuri.YuriCode.Powers;

public sealed class LuckySoulPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        CardLocation location)
    {
        if (card.Owner.Creature != Owner)
        {
            return location;
        }

        if (card.Type != CardType.Attack)
        {
            return location;
        }

        if (location.pileType != PileType.Discard)
        {
            return location;
        }

        int attacksPlayedThisTurn =
            CombatManager.Instance.History.CardPlaysStarted.Count(
                (CardPlayStartedEntry entry) =>
                    entry.HappenedThisTurn(CombatState) &&
                    entry.CardPlay.Card.Type == CardType.Attack &&
                    entry.CardPlay.Player == Owner.Player
            );

        if (attacksPlayedThisTurn >= Amount)
        {
            return location;
        }

        location.pileType = PileType.Draw;
        location.position = CardPilePosition.Top;

        return location;
    }

    public override Task AfterModifyingCardPlayResultLocation(
        CardModel card,
        CardLocation location)
    {
        if (card.Owner.Creature != Owner)
        {
            return Task.CompletedTask;
        }

        Flash();

        return Task.CompletedTask;
    }
}