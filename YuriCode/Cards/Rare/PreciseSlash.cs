using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Rare;

public class PreciseSlash() : YuriCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(6, ValueProp.Move)
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
        var damage = play.Target.GetPowerAmount<FatalStrikePower>();
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
            yuri.PlayAnimation(ownerCreature, "fatal_strike", true);
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            yuri.YuriDashTo(ownerCreature, play.Target, distance: -250f, forceMove: true, durationSeconds: 0.1f,
                overrideAnim: null);
            await Task.Delay((int)(0.05f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "fatal_strike"
            );
            await DamageCmd.Attack(damage).Targeting(play.Target).FromCard(this, play).WithValueProp(ValueProp.Move)
                .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
                .Execute(choiceContext);
            await Task.Delay((int)(0.250f * 1000f));
            await yuri.ReturnToCombatHome(Owner.Creature);
        }
        else
            await DamageCmd.Attack(damage).Targeting(play.Target).FromCard(this, play).WithValueProp(ValueProp.Move)
                .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
                .Execute(choiceContext);
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}