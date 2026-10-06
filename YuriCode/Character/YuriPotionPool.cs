using BaseLib.Abstracts;
using Yuri.YuriCode.Extensions;
using Godot;

namespace Yuri.YuriCode.Character;

public class YuriPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Yuri.Color;


    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}