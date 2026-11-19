using BaseLib.Config;

namespace Yuri.YuriCode.Extensions;

public sealed class YuriConfig : SimpleModConfig
{
    [ConfigSection("Audio")]
    public static bool EnableOverlimitMusic { get; set; } = true;
}