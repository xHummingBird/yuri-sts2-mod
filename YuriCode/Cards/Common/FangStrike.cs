using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Cards.Common;

public class FangStrike() : YuriCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new EnergyVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.BaseArte
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        
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
            AudioHelper.PlayRandomAttackHard();
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f);
            float duration = yuri.PlayAnimation(ownerCreature, "fang_strike").total;
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, null, "res://Yuri/sfx/hit_2.wav", "hit");
            await Task.Delay((int)(0.350f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit_red"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/FromTifa/punch_critical.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        base.EnergyCost.SetThisCombat(0);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}