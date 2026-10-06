using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Yuri.YuriCode.Cards.Ancient;
using Yuri.YuriCode.Cards.Basic;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Extensions;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class YuriTouchOfOrobasPatch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is SecondStar)
        {
            __result = ModelDb.Relic<VesperiaNoTwo>().ToMutable();
        }
    }
}


[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
internal static class YuriArchaicToothTranscendencePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<AzureEdge>().Id] = ModelDb.Card<AzureStorm>();
    }
}

[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.SetupForPlayer))]
public static class DustyTomeSetupPatch
{
    [HarmonyPostfix]
    public static void Postfix(DustyTome __instance, Player player)
    {
        if (player.Character is not Character.Yuri)
            return;

        __instance.AncientCard = ModelDb.Card<OverTheLimit>().Id;
    }
}


[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.AfterObtained))]
public static class DustyTomePatch
{
    [HarmonyPrefix]
    public static void Prefix(DustyTome __instance)
    {
        if (__instance.Owner?.Character is not Character.Yuri)
            return;
        
        __instance.AncientCard = ModelDb.Card<OverTheLimit>().Id;
    }
}