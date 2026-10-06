using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Yuri.YuriCode.Powers;

public sealed class ComboVoltagePower : YuriPower
{
    private sealed class Data
    {
        public int EnergySpent;
        public int TriggerCount;
    }

    private const int EnergyIncrement = 4;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount =>
        EnergyIncrement -
        GetInternalData<Data>().EnergySpent % EnergyIncrement;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.ForEnergy(this)
    ];

    public override PowerInstanceType InstanceType =>
        PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(EnergyIncrement)
    ];

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (card.Owner.Creature != Owner || amount <= 0)
        {
            return;
        }

        Data data = GetInternalData<Data>();

        data.EnergySpent += amount;

        int triggers =
            data.EnergySpent / EnergyIncrement -
            data.TriggerCount;

        if (triggers > 0)
        {
            Flash();

            await PlayerCmd.GainEnergy(
                Amount * triggers,
                Owner.Player);

            data.TriggerCount += triggers;
        }

        InvokeDisplayAmountChanged();
    }
}