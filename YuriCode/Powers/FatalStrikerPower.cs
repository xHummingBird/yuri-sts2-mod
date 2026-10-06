using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Yuri.YuriCode.Powers;

public sealed class FatalStrikerPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FatalStrikePower>()
    ];

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer == null)
        {
            return;
        }

        if (dealer != Owner && dealer.PetOwner?.Creature != Owner)
        {
            return;
        }

        if (!props.IsPoweredAttack())
        {
            return;
        }

        if (result.TotalDamage <= 0)
        {
            return;
        }

        Flash();

        await PowerCmd.Apply<FatalStrikePower>(
            choiceContext,
            target,
            result.TotalDamage * Amount,
            Owner,
            null
        );
    }
}