using MegaCrit.Sts2.Core.Entities.Powers;

namespace Yuri.YuriCode.Powers;

public class KingOfCombosPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}