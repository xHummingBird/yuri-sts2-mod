using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Uncommon;

public class BrutalFang() : YuriCard(0, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IArcaneArte
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<OverlimitPower>() || Owner.HasPower<ArcaneArteBoostPower>();
    
    public int ComboGain => 9;
    protected override bool HasEnergyCostX => true;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new EnergyVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.ArcaneArte
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        
        int energySpent = ResolveEnergyXValue();

        if (Owner.HasPower<OverlimitPower>() || Owner.HasPower<ArcaneArteBoostPower>())
            energySpent += 2;
        
        if (energySpent == 0)
            return;

        decimal totalDamage =
            DynamicVars.Damage.BaseValue * energySpent;
        
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            float distance = yuri.DistanceToTarget(ownerCreature, play.Target);
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f);
            SfxCmd.Play("res://Yuri/sounds/artes/brutal_fang.wav");
            yuri.PlayAnimation(ownerCreature, "brutal_fang");
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_1.wav");
            await Task.Delay((int)(0.033f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_1.wav", null, "hit_red");
            
            await Task.Delay((int)(0.15f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_2.wav");
            await Task.Delay((int)(0.034f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_2.wav", null, "hit_red");
            
            await Task.Delay((int)(0.083f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_3.wav");
            await Task.Delay((int)(0.034f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_3.wav", null, "hit_red");
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_2.wav");
            await Task.Delay((int)(0.033f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_2.wav", null, "hit_red");
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_3.wav");
            await Task.Delay((int)(0.033f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_3.wav", null, "hit_red");
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_2.wav");
            await Task.Delay((int)(0.033f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_2.wav", null, "hit_red");
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_3.wav");
            await Task.Delay((int)(0.034f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_3.wav", null, "hit_red");
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_2.wav");
            await Task.Delay((int)(0.033f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_hit_2.wav", null, "hit_red");
            
            AudioHelper.PlayRandomPhrase();
            await Task.Delay((int)(0.250f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_1.wav");
            await Task.Delay((int)(0.050f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit_red"
            );
        }
        await DamageCmd.Attack(totalDamage).Targeting(play.Target).FromCard(this, play).WithValueProp(ValueProp.Move)
            .WithHitFx(null, "res://Yuri/sfx/FromTifa/punch_critical.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}