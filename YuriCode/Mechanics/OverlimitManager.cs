using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Powers;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Mechanics;

public static class OverlimitManager
{
    public class OverlimitData
    {
        public Action<int>? OnOverlimitChanged;
    }

    private const int MaxOverlimit = 100;

    private static readonly Dictionary<Player, OverlimitData> _data = new();

    private static ArteRelicBase? GetRelic(Player player)
    {
        return player.Relics
            .OfType<ArteRelicBase>()
            .FirstOrDefault();
    }
    
    private static OverlimitData GetData(Player player)
    {
        if (!_data.TryGetValue(player, out var data))
        {
            data = new OverlimitData();
            _data[player] = data;
        }

        return data;
    }

    public static int GetOverlimit(Player player)
    {
        return GetRelic(player)?.StoredOverlimit ?? 0;
    }

    public static int GetMaxOverlimit()
    {
        return MaxOverlimit;
    }

    public static void SetOverlimit(Player player, int value)
    {
        var relic = GetRelic(player);

        if (relic == null)
            return;

        value = Math.Clamp(
            value,
            0,
            MaxOverlimit);

        if (relic.StoredOverlimit == value)
            return;

        relic.StoredOverlimit = value;

        GetData(player)
            .OnOverlimitChanged?
            .Invoke(value);
    }

    public static void GainOverlimit(Player player, int amount)
    {
        if (amount <= 0)
            return;

        SetOverlimit(
            player,
            GetOverlimit(player) + amount);
    }
    
    public static async Task CheckOverlimitReady(
        Creature creature,
        PlayerChoiceContext? context,
        Creature source,
        CardModel? card)
    {
        var overlimitTurns = 2;
        
        if (creature.Player.GetRelic<LimitOctet>() != null)
            overlimitTurns = 3;
        
        var player = creature.Player;

        if (player == null)
            return;

        if (IsFull(player) &&
            !creature.HasPower<OverlimitPower>())
        {
            if (context != null)
            {
                await PowerCmd.Apply<OverlimitPower>(
                    context,
                    creature,
                    overlimitTurns,
                    source,
                    card
                );
            }
            else
            {
                await PowerCmd.Apply<OverlimitPower>(
                    new ThrowingPlayerChoiceContext(),
                    creature,
                    overlimitTurns,
                    source,
                    card
                );
            }
        }
    }

    public static void SpendOverlimit(Player player, int amount)
    {
        if (amount <= 0)
            return;

        SetOverlimit(
            player,
            GetOverlimit(player) - amount);
    }

    public static bool HasOverlimit(Player player, int amount)
    {
        return GetOverlimit(player) >= amount;
    }

    public static bool IsFull(Player player)
    {
        return GetOverlimit(player) >= MaxOverlimit;
    }

    public static OverlimitData GetDataForUI(Player player)
    {
        return GetData(player);
    }

    public static void Reset(Player player)
    {
        SetOverlimit(player, 0);
    }

    public static void RemoveData(Player player)
    {
        _data.Remove(player);
    }
}