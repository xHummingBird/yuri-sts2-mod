using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Yuri.YuriCode.Mechanics;

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class ArteCardUiUpdate
{
    public static void Postfix(NCard __instance)
    {
        CardDisplayOverlay.Ensure(__instance);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class ArteCardUiReady
{
    public static void Postfix(NCard __instance)
    {
        Callable.From(() =>
        {
            CardDisplayOverlay.Ensure(__instance);
        }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class ArteCardUiModelChanged
{
    public static void Postfix(NCard __instance)
    {
        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
            {
                CardDisplayOverlay.Ensure(__instance);

                Callable.From(() =>
                {
                    CardDisplayOverlay.Ensure(__instance);
                }).CallDeferred();

            }).CallDeferred();
        };
    }
}