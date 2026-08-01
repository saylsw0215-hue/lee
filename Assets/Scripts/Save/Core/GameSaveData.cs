using System;
using System.Collections.Generic;

namespace HeroDefense.Save
{
    [Serializable] public sealed class StringIntRecord{public string id;public int value;public StringIntRecord(){}public StringIntRecord(string key,int amount){id=key;value=amount;}}
    [Serializable] public sealed class ContentRecord{public string id;public bool discovered;public bool defeated;public int count;public string firstSeenUtc;}
    [Serializable] public sealed class StageCompletionRecord{public string stageId;public bool unlocked;public bool easyCleared;public bool normalCleared;public bool hardCleared;public int bestBaseHp;public float bestTime;public int plays;}
    [Serializable] public sealed class HeroProgressRecord{public string heroId;public bool unlocked;public int uses;public int wins;public int losses;public long damage;public long kills;public int masteryXp;public int masteryLevel=1;}
    [Serializable] public sealed class AchievementRecord{public string id;public long progress;public bool completed;public bool claimed;public string completedAtUtc;}
    [Serializable] public sealed class PlayerProfileSaveData{public string displayName="Commander";public string firstPlayedAtUtc;public string lastPlayedAtUtc;public double totalPlaySeconds;public int battles;public int wins;public int losses;}
    [Serializable] public sealed class CurrencySaveData{public int coin;public int soulGem;}
    [Serializable] public sealed class EndlessProgressSaveData{public int highestWave;public int highestKills;public float longestPlayTime;public int highestBattleLevel;public int mostBosses;public string heroId;public string achievedAtUtc;}
    [Serializable] public sealed class TutorialSaveData{public bool completed;public bool skipped;public string lastStepId;public List<string> completedSteps=new();}
    public enum GraphicsQualityOption{Low,Medium,High} public enum LanguageOption{Korean,English}
    [Serializable] public sealed class SettingsSaveData{public float masterVolume=1;public float musicVolume=.8f;public float sfxVolume=.9f;public bool vibration=true;public bool screenShake=true;public bool damageNumbers=true;public bool healthBars=true;public bool largeUi;public bool highContrast;public bool colorAccessibility=true;public bool aimAssist=true;public bool autoAim=true;public float textScale=1;public GraphicsQualityOption graphicsQuality=GraphicsQualityOption.Medium;public int targetFrameRate=60;public LanguageOption language=LanguageOption.Korean;}
    [Serializable] public sealed class LifetimeStatisticsSaveData{public long enemyKills;public long eliteKills;public long bossKills;public long unitsProduced;public long buildingsPlaced;public long battleGoldEarned;public long coinEarned;public long soulGemEarned;public double heroDamage;public double healing;public long skillsUsed;public long ultimatesUsed;}
    [Serializable] public sealed class GameSaveData
    {
        public const int CurrentVersion=1;public int saveVersion=CurrentVersion;public string saveId;public string createdAtUtc;public string lastSavedAtUtc;public string checksum;
        public PlayerProfileSaveData profile=new();public CurrencySaveData currencies=new();public List<HeroProgressRecord> heroes=new();public List<StageCompletionRecord> stages=new();public EndlessProgressSaveData endless=new();public List<StringIntRecord> metaUpgrades=new();public List<ContentRecord> collection=new();public List<AchievementRecord> achievements=new();public TutorialSaveData tutorial=new();public SettingsSaveData settings=new();public LifetimeStatisticsSaveData statistics=new();public List<string> claimedBattleResultIds=new();
    }
}
