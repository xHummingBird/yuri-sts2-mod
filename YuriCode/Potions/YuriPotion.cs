using BaseLib.Abstracts;
using BaseLib.Utils;
using Yuri.YuriCode.Character;

namespace Yuri.YuriCode.Potions;

[Pool(typeof(YuriPotionPool))]
public abstract class YuriPotion : CustomPotionModel;