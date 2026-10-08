using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Powers;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Cards.Common;

public class VitalSlash() : YuriCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(10, ValueProp.Move)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FatalStrikePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    play.Target
                );
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 250f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            
            await yuri.YuriDashTo(ownerCreature, play.Target);
            AudioHelper.PlayRandomAttackHard();
            yuri.PlayAnimation(ownerCreature, "fatal_strike");
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            yuri.YuriDashTo(ownerCreature, play.Target, distance: -250f, forceMove: true, durationSeconds: 0.1f,
                overrideAnim: "fatal_strike");
            await Task.Delay((int)(0.05f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "fatal_strike"
            );
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
                .Execute(choiceContext);
            await Task.Delay((int)(0.25f * 1000f));
            await yuri.Retreat(ownerCreature);
        }
        else await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
            .Execute(choiceContext);
        
        var enemyFatalStrike = play.Target.GetPowerAmount<FatalStrikePower>();
        await PowerCmd.Apply<FatalStrikePower>(choiceContext, play.Target, enemyFatalStrike, Owner.Creature, this);
        await FatalStrikePower.CheckAfterCardApplication(
            play.Target
        );
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}