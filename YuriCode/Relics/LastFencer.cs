using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Relics;

public class LastFencer : YuriRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
        {
            return Task.CompletedTask;
        }

        if (cardPlay.Card is IBaseArte or IArcaneArte)
        {
            CardCmd.Upgrade(cardPlay.Card);
        }

        return Task.CompletedTask;
    }
}