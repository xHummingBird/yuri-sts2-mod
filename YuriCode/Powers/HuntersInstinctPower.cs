using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Yuri.YuriCode.Powers;

public class HuntersInstinctPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(base.Owner))
        {
            Flash();
            Creature creature = base.Owner.Player.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
            if (creature != null)
            {
                await PowerCmd.Apply<FatalStrikePower>(new ThrowingPlayerChoiceContext(), creature, base.Amount, base.Owner, null);
            }
        }
    }
}