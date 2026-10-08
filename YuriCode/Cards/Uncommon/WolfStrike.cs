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
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Uncommon;

public class WolfStrike() : YuriCard(0, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new PowerVar<FatalStrikePower>(10m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.BaseArte,
        HoverTipFactory.FromPower<FatalStrikePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        bool enemyIsClose = false;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    play.Target
                );
            
            if (distance < 300f)
                enemyIsClose = true;
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            AudioHelper.PlayRandomAttackHard();
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f);
            float duration = yuri.PlayAnimation(ownerCreature, "wolf_strike").total;
            await Task.Delay((int)(0.2f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, null, "res://Yuri/sfx/hit_3.wav", "hit");
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
        if (enemyIsClose)
        {
            await PowerCmd.Apply<FatalStrikePower>(choiceContext, play.Target,
                DynamicVars["FatalStrikePower"].BaseValue, Owner.Creature, this);
            await FatalStrikePower.CheckAfterCardApplication(
                play.Target
            );
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["FatalStrikePower"].UpgradeValueBy(2);
    }
}