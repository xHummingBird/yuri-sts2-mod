using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Cards.Uncommon;

public class GuardCancel() : YuriCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner?.Character is Character.Yuri yuri)
        {
            await yuri.ReturnToCombatHome(Owner.Creature);
        }
        
        CardModel? arte =
            CardFactory.GetDistinctForCombat(
                    Owner,
                    from card in Owner.Character.CardPool.GetUnlockedCards(
                        Owner.UnlockState,
                        Owner.RunState.CardMultiplayerConstraint)
                    where card is IBaseArte ||
                          card is IArcaneArte
                    select card,
                    1,
                    Owner.RunState.Rng.CombatCardGeneration)
                .FirstOrDefault();

        if (arte == null)
            return;

        arte.SetToFreeThisTurn();

        await CardPileCmd.AddGeneratedCardToCombat(
            arte,
            PileType.Hand,
            Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}