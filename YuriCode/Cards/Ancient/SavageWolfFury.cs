using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Ancient;

public class SavageWolfFury() : YuriCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(40m),
        new ExtraDamageVar(18m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
            {
                return card.Owner.HasPower<MysticArteBoostPower>()
                    ? 1m
                    : 0m;
            }),

        new RepeatVar(19)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var targetNode =
            NCombatRoom.Instance?.GetCreatureNode(play.Target);
        var positionX = targetNode.Position.X;
        var positionY = targetNode.Position.Y;

        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            yuri.PlayAnimation(ownerCreature, "cast_mystic_arte", false);
            SfxCmd.Play("res://Yuri/sfx/mystic_arte_activate_2.wav");
            SfxCmd.Play("res://Yuri/sounds/hiougi_1.wav");
            await Task.Delay((int)(1.4f * 1000f));
            yuri.PlayAnimation(ownerCreature, "savage_wolf_fury", false);
            SfxCmd.Play("res://Yuri/sounds/hiougi_2.wav");
            await MysticArteCinematic.Start(RunManager.Instance.NetService.NetId, ownerCreature);
            if (targetNode != null)
            {
                targetNode.Position =
                    new Vector2(
                        0,
                        200
                    );
            } // 0.3
            await yuri.ReturnToCombatHome(ownerCreature, "savage_wolf_fury", false, durationSeconds: 0.01f);
            await yuri.YuriDashTo(ownerCreature, play.Target, distance: 250f, durationSeconds: 0.04f, forceMove: true, tolerance: 0f, overrideAnim: "savage_wolf_fury"); //0.35
            await MysticArteCinematic.FadeTarget
            (
                play.Target,
                true
            ); //0.45
            await Task.Delay((int)(0.15f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit"); //0.7
            
            await Task.Delay((int)(0.35f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit"); //1.1
            
            await Task.Delay((int)(0.45f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit_2"); //1.6
            
            await Task.Delay((int)(0.35f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit_2"); //2.0
            
            await Task.Delay((int)(0.7f * 1000f));
            SfxCmd.Play("res://Yuri/sounds/hiougi_3.wav");
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.1f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit"); //2.8
            
            await Task.Delay((int)(0.383f * 1000f));//3.183
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));//3.233
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit"); //3.233
            
            await Task.Delay((int)(1.017f * 1000f)); //4.25
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit_2"); //4.3
            
            await Task.Delay((int)(0.467f * 1000f)); //4.767
            SfxCmd.Play("res://Yuri/sounds/hiougi_4.wav");
            await Task.Delay((int)(0.116f * 1000f)); //4.883
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.067f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit"); //4.95
            
            await Task.Delay((int)(0.4f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit_2"); //5.4
            
            await Task.Delay((int)(0.2f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit_2"); //5.65
            
            await Task.Delay((int)(0.25f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit"); //5.95
            
            await Task.Delay((int)(0.25f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit"); //6.25
            
            await Task.Delay((int)(0.30f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit_2"); //6.6
            
            await Task.Delay((int)(0.30f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit"); //6.95
            
            await Task.Delay((int)(0.2f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit_2"); //7.2
            SfxCmd.Play("res://Yuri/sounds/hiougi_5.wav");
            
            await Task.Delay((int)(0.25f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit"); //7.5
            
            await Task.Delay((int)(0.3f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit_2"); //7.85
            
            await Task.Delay((int)(0.3f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_3.wav", null,"savage_wolf_hit"); //8.2
            
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sounds/hiougi_6.wav");
            
            await Task.Delay((int)(0.15f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, play.Target, "res://Yuri/sfx/hit_2.wav", null,"savage_wolf_hit"); //8.5
            
            await Task.Delay((int)(0.1f * 1000f));//8.6 seconds
            //Cut In for 1.0 seconds
            yuri.PlayCutIn("yuri_cutin");
            await Task.Delay((int)(0.2f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "savage_wolf_lasthit"
            );
            await Task.Delay((int)(0.6f * 1000f));
            SfxCmd.Play("res://Yuri/sounds/hiougi_7.wav");
            await Task.Delay((int)(0.2f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "savage_wolf_hit_2"
            );
        }
        await CommonActions.CardAttack(this, play.Target, 1)
            .WithHitFx(null, "res://Yuri/sfx/hit_1.wav")
            .Execute(choiceContext);
        await MysticArteCinematic.FadeTarget
        (
            play.Target,
            false
        );
        await Task.Delay((int)(0.3f * 1000f));
        if (targetNode != null)
        {
            targetNode.Position =
                new Vector2(
                    positionX,
                    positionY
                );
        }
        await MysticArteCinematic.End(
            RunManager.Instance.NetService.NetId
        );
        OverlimitManager.SetOverlimit(Owner, 0);
        await PowerCmd.Remove<OverlimitPower>(Owner.Creature);
        await PowerCmd.Remove<MysticArteBoostPower>(Owner.Creature);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(15);
        DynamicVars.ExtraDamage.UpgradeValueBy(5);
    }
}