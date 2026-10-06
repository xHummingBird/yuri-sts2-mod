using MegaCrit.Sts2.Core.Commands;

namespace Yuri.YuriCode.Extensions;

public static class AudioHelper
{
    private static readonly Random rng = new Random();
    
    private static readonly string[] attackSfx =
    {
        "res://Yuri/sounds/atk-1.wav",
        "res://Yuri/sounds/atk-2.wav",
        "res://Yuri/sounds/atk-3.wav",
    };
    
    private static readonly string[] damagedSfx =
    {
        "res://Yuri/sounds/hit_low.wav"
    };
    
    private static readonly string[] highDamagedSfx =
    {
        "res://Yuri/sounds/hit_high.wav"
    };
    
    private static readonly string[] criticalDamagedSfx =
    {
        "res://Yuri/sounds/hit_critical.wav"
    };
    
    private static readonly string[] defendSfx =
    {
        "res://Yuri/sounds/atk-1.wav",
        "res://Yuri/sounds/atk-2.wav",
    };
    
    private static readonly string[] victorySfx =
    {
        "res://Yuri/sounds/victory (1).wav",
        "res://Yuri/sounds/victory (2).wav",
        "res://Yuri/sounds/victory (3).wav",
        "res://Yuri/sounds/victory (4).wav",
        "res://Yuri/sounds/victory.wav",
        
    };

    private static readonly string[] gameoverSfx =
    {
        "res://Yuri/sounds/gameover_1.wav",
        "res://Yuri/sounds/gameover_2.wav",
    };
    
    private static readonly string[] phraseSfx =
    {
        "res://Yuri/sounds/phrase (1).wav",
        "res://Yuri/sounds/phrase (2).wav",
        "res://Yuri/sounds/phrase (3).wav",
        "res://Yuri/sounds/phrase (4).wav",
        "res://Yuri/sounds/phrase (5).wav",
    };
    
    private static readonly string[] attackHighSfx =
    {
        "res://Yuri/sounds/atk_hard.wav"
        
    };
    
    public static void PlayRandomAttack()
    {
        PlayRandom(attackSfx);
    }
    
    public static void PlayRandomDefend()
    {
        PlayRandom(defendSfx);
    }

    public static void PlayRandomPhrase()
    {
        PlayRandom(phraseSfx);
    }
    
    public static void PlayRandomDamaged()
    {
        PlayRandom(damagedSfx);
    }

    public static void PlayRandomDamagedHigh()
    {
        PlayRandom(highDamagedSfx);
    }

    public static void PlayRandomGameover()
    {
        PlayRandom(gameoverSfx);
    }

    public static void PlayRandomDamagedCritical()
    {
        PlayRandom(criticalDamagedSfx);
    }
    
    public static void PlayRandomVictory()
    {
        PlayRandom(victorySfx);
    }

    public static void PlayRandomAttackHard()
    {
        PlayRandom(attackHighSfx);
    }

    public static void PlayRandom(string[] pool)
    {
        int index = rng.Next(pool.Length);
        SfxCmd.Play(pool[index]);
    }
}