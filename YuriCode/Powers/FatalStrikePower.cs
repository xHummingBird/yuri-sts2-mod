using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Cards.Common;
using Yuri.YuriCode.Cards.Rare;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Powers;

public sealed class FatalStrikePower : YuriPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Color AmountLabelColor =>
        PowerModel._normalAmountLabelColor;

    /// <summary>
    /// Checks whether Fatal Strike should execute the target.
    ///
    /// This must only be called immediately after a card applies
    /// Fatal Strike. Relics and other effects should apply the power
    /// normally without calling this method.
    /// </summary>
    public static async Task CheckAfterCardApplication(
        Creature target)
    {
        if (target == null)
            return;

        await CheckAfterCardApplication(
        [
            target
        ]);
    }

    /// <summary>
    /// Convenience method for checking multiple targets after a card
    /// applies Fatal Strike to all of them.
    /// </summary>
    public static async Task CheckAfterCardApplication(
        IReadOnlyList<Creature> targets)
    {
        if (targets == null || targets.Count == 0)
            return;

        var targetsToKill =
            new List<Creature>();

        foreach (var target in targets)
        {
            if (target == null || target.IsDead)
                continue;

            var fatalStrike =
                target.GetPower<FatalStrikePower>();

            if (fatalStrike == null)
                continue;

            if (target.CurrentHp <= fatalStrike.Amount)
                targetsToKill.Add(target);
        }

        if (targetsToKill.Count == 0)
        {
            return;
        }

        var player = targetsToKill[0].CombatState?
            .Players
            .FirstOrDefault(p => p.Character is Character.Yuri);

        if (player != null)
        {
            var relic = player.GetRelic<Abyssion>();

            if (relic != null)
            {
                await relic.OnFatalStrikeKill(targetsToKill);
            }
        }

        await DoomPower.DoomKill(targetsToKill);
    }
    
    public static bool WillExecute(Creature target)
    {
        if (target == null || target.IsDead)
            return false;

        var fatalStrike =
            target.GetPower<FatalStrikePower>();

        if (fatalStrike == null)
            return false;

        return target.CurrentHp <= fatalStrike.Amount;
    }
    
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // Fatal Strike must be on the creature receiving damage.
        if (target != Owner)
        {
            return 1m;
        }

        // Only Severing Fang receives the damage bonus.
        if (cardSource is not SeveringFang)
        {
            return 1m;
        }

        // Either power activates the same single multiplier.
        bool hasRequiredPower = dealer.HasPower<ArcaneArteBoostPower>() || dealer.HasPower<OverlimitPower>();

        if (!hasRequiredPower)
        {
            return 1m;
        }

        return 2m;
    }
    
    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // Fatal Strike must be on the creature receiving damage.
        if (target != Owner)
        {
            return 0m;
        }

        // Only Severing Fang receives the damage bonus.
        if (cardSource is not ShiningDragonSwarm)
        {
            return 0m;
        }

        // Either power activates the same single multiplier.
        bool hasRequiredPower = dealer.HasPower<ArcaneArteBoostPower>() || dealer.HasPower<OverlimitPower>();

        if (!hasRequiredPower)
        {
            return 0m;
        }

        return Amount;
    }
}
