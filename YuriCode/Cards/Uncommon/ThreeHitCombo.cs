using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Cards.Uncommon;

public class ThreeHitCombo() : YuriCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(3, ValueProp.Move),
        new RepeatVar(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        var damage = DynamicVars.Damage.PreviewValue;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            float distance = yuri.DistanceToTarget(ownerCreature, play.Target);
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 250f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            
            await yuri.YuriDashTo(ownerCreature, play.Target);
            AudioHelper.PlayRandomAttack();
            yuri.PlayAnimation(ownerCreature, "combo", true);
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.033f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
            await DamageCmd.Attack(damage).Targeting(play.Target).FromCard(this, play).WithValueProp(ValueProp.Unpowered).WithHitFx(null, "res://Yuri/sfx/hit_3.wav").Execute(choiceContext);
            await Task.Delay((int)(0.120f * 1000f));
            AudioHelper.PlayRandomAttack();
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
            await DamageCmd.Attack(damage).Targeting(play.Target).FromCard(this, play).WithValueProp(ValueProp.Unpowered).WithHitFx(null, "res://Yuri/sfx/hit_2.wav").Execute(choiceContext);
            AudioHelper.PlayRandomAttack();
            await Task.Delay((int)(0.087f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/hit_1.wav")
            .Execute(choiceContext);
        await Task.Delay((int)(0.1f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        CardModel? attack = PileType.Draw.GetPile(Owner).Cards
            .Where(c =>
                c.Type == CardType.Attack &&
                c is IBaseArte &&
                !c.Keywords.Contains(CardKeyword.Unplayable))
            .ToList()
            .StableShuffle(Owner.RunState.Rng.Shuffle)
            .FirstOrDefault();

        if (attack != null)
        {
            if (play.Target != null)
                await CardCmd.AutoPlay(
                    choiceContext,
                    attack,
                    play.Target
                );
            else
                await CardCmd.AutoPlay(
                    choiceContext,
                    attack,
                    null)
                    ;
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}