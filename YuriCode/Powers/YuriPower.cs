using BaseLib.Abstracts;
using BaseLib.Extensions;
using Yuri.YuriCode.Extensions;
using Godot;

namespace Yuri.YuriCode.Powers;

public abstract class YuriPower : CustomPowerModel
{
    //Loads from Yuri/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}