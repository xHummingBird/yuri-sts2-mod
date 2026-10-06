using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Cards.Rare;

public class ShiningFang() : YuriCard(1, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 6;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9m, ValueProp.Move),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.BaseArte,
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var blockAmount = DynamicVars.Damage.PreviewValue;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    play.Target
                );
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f);
            SfxCmd.Play("res://Yuri/sounds/artes/shining_fang.wav");
            float duration = yuri.PlayAnimation(ownerCreature, "shining_fang").total;
            await Task.Delay((int)(0.033f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", "hit_red");
            await Task.Delay((int)(0.600f * 1000f));
            
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay(50);
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_1.wav", "hit_red");
            await Task.Delay((int)(0.167f * 1000f));
            
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay(33);
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_1.wav", "hit_red");
            await Task.Delay((int)(0.117f * 1000f));
            
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay(33);
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_1.wav", "hit_red");
            await Task.Delay((int)(0.117f * 1000f));
            
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay(33);
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_1.wav", "hit_red");
            await Task.Delay((int)(0.167f * 1000f));
            
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay(33);
            await Task.Delay((int)(0.050f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit_red"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/hit_1.wav")
            .Execute(choiceContext);
        await Task.Delay((int)(0.2f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        await CreatureCmd.GainBlock(Owner.Creature, blockAmount, ValueProp.Unpowered, play);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}