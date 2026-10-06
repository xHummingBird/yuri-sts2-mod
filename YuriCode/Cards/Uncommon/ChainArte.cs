using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Cards.Uncommon;

public class ChainArte() : YuriCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        CardModel? arcaneArte =
            CardFactory.GetDistinctForCombat(
                    Owner,
                    from card in Owner.Character.CardPool.GetUnlockedCards(
                        Owner.UnlockState,
                        Owner.RunState.CardMultiplayerConstraint)
                    where card is IArcaneArte
                    select card,
                    1,
                    Owner.RunState.Rng.CombatCardGeneration)
                .FirstOrDefault();

        if (arcaneArte == null)
            return;

        /*
         * Chain Arte+ plays an Upgraded Arcane Arte.
         */
        if (IsUpgraded)
        {
            CardCmd.Upgrade(arcaneArte);
        }

        /*
         * The generated Arcane Arte is exhausted after being played.
         */
        arcaneArte.AddKeyword(
            CardKeyword.Exhaust);

        await CardCmd.AutoPlay(
            choiceContext,
            arcaneArte,
            cardPlay.Target);
    }

    protected override void OnUpgrade()
    {
    }
}