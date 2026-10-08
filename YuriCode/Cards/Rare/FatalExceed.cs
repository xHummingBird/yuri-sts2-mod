using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Rare;

public class FatalExceed() : YuriCard(3, CardType.Skill,
    CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<FatalStrikePower>(25m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FatalStrikePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<FatalStrikePower>(choiceContext, CombatState.HittableEnemies, DynamicVars["FatalStrikePower"].BaseValue, Owner.Creature, this);
        
        var ownerCreature = Owner?.Creature;
        
        foreach (var enemy in CombatState.HittableEnemies)
            await FatalStrikePower.CheckAfterCardApplication(
                enemy
            );;
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars["FatalStrikePower"].UpgradeValueBy(7);
    }
}