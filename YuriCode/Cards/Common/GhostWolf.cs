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

namespace Yuri.YuriCode.Cards.Common;

public class GhostWolf() : YuriCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.BaseArte,
        HoverTipFactory.FromPower<VulnerablePower>()
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
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f);
            SfxCmd.Play("res://Yuri/sounds/artes/ghost_wolf.wav");
            float duration = yuri.PlayAnimation(ownerCreature, "ghost_wolf").total;
            await Task.Delay((int)(0.183f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_swing_1.wav");
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, null, "res://Yuri/sfx/FromTifa/punch_critical.wav", "hit");
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: -250f, durationSeconds: 0.067f, forceMove: true, overrideAnim: null);
            await Task.Delay((int)(0.250f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}