using BaseLib.Extensions;
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

namespace Yuri.YuriCode.Cards.Common;

public class CerberusStrike() : YuriCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 3;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new CardsVar(1)
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
            SfxCmd.Play("res://Yuri/sounds/artes/cerberus_strike.wav");
            yuri.PlayAnimation(ownerCreature, "cerberus_strike");
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
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit_red"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/FromTifa/punch_hit_3.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}