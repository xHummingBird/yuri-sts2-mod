using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Yuri.YuriCode.Extensions;

public class YuriExtensions
{
    public static class CombatHelpers
    {
        private const string DefaultHitVfxScene =
            "res://Yuri/scenes/vfx.tscn";

        private const string DefaultHitVfx = "hit";

        public const string NoHitVfx = "__NONE__";

        public static async Task YuriFakeHit(
            Creature attacker,
            Creature target,
            string? attackSfx = null,
            string? hitSfx = null,
            string? hitVfx = null,
            bool playHitVfx = true)
        {
            if (target == null)
                return;

            if (attackSfx != null)
                SfxCmd.Play(attackSfx);

            if (hitSfx != null)
                SfxCmd.Play(hitSfx);

            var yuri = attacker.Player.Character as Character.Yuri;

            if (playHitVfx)
            {
                yuri.PlayVfxOnTarget(
                    target,
                    DefaultHitVfxScene,
                    hitVfx ?? DefaultHitVfx);
            }

            await CreatureCmd.TriggerAnim(target, "Hit", 0f);

            if (target.Monster?.HasHurtSfx == true)
                SfxCmd.Play(target.Monster.HurtSfx);
        }
    }
}