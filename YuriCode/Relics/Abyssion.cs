using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Yuri.YuriCode.Relics;

public class Abyssion : YuriRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    
    public async Task OnFatalStrikeKill(
        IReadOnlyList<Creature> creatures)
    {
        int count = creatures.Count(c =>
            c != Owner.Creature &&
            c.Powers.All(p => p.ShouldOwnerDeathTriggerFatal()));

        if (count == 0)
        {
            return;
        }

        Flash();

        await CreatureCmd.Heal(
            Owner.Creature,
            3 * count);
    }
}