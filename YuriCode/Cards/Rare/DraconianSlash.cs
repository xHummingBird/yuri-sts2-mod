using BaseLib.Extensions;
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
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Rare;

public class DraconianSlash() : YuriCard(1, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
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
            
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: 200f);
            AudioHelper.PlayRandomAttackHard();
            float duration = yuri.PlayAnimation(ownerCreature, "draconian_slash").total;
            await Task.Delay((int)(0.167f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/punch_hit_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/FromTifa/punch_critical.wav", "hit");
            await Task.Delay((int)(0.300f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.033f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Yuri/sfx/hit_1.wav")
                .BeforeDamage(async delegate { yuri.BounceEnemy(play.Target, 400, 0.3f); })
                .Execute(choiceContext);
        }
        
        await Task.Delay((int)(0.3f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        await PowerCmd.Apply<FreeAttackPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }
}