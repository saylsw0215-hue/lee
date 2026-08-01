using System;
using UnityEngine;

namespace HeroDefense.Progression
{
    /// <summary>Immutable battle XP curve. Runtime instances are created by the progression composer.</summary>
    [CreateAssetMenu(fileName="ExperienceTable",menuName="Hero Defense/Progression/Experience Table")]
    public sealed class ExperienceTableData:ScriptableObject
    {
        [SerializeField]private int[] requiredExperience={60,80,105,135,170,210,255,305,360,420,490,565,645,730,820,920,1030,1150,1300};
        public int MaxLevel=>requiredExperience.Length+1;
        public int RequiredForLevel(int level)=>level>=MaxLevel?0:requiredExperience[Mathf.Clamp(level-1,0,requiredExperience.Length-1)];
        public bool Validate(out string reason){for(int i=0;i<requiredExperience.Length;i++)if(requiredExperience[i]<=0){reason="Experience requirements must be positive.";return false;}reason=string.Empty;return requiredExperience.Length>0;}
    }

    /// <summary>Pure, queue-aware battle level state supporting multi-level XP awards.</summary>
    public sealed class BattleExperienceManager
    {
        public event Action Changed;public event Action<int> LevelGained;
        public int Level{get;private set;}=1;public int CurrentExperience{get;private set;}public int TotalExperience{get;private set;}public int PendingSelections{get;private set;}
        public int MaxLevel=>requirements.Length+1;public int RequiredExperience=>Level>=MaxLevel?0:requirements[Level-1];public bool IsMaxLevel=>Level>=MaxLevel;
        private readonly int[] requirements;
        public BattleExperienceManager(int[] curve=null){requirements=curve??new[]{60,80,105,135,170,210,255,305,360,420,490,565,645,730,820,920,1030,1150,1300};}
        public int AddExperience(int amount)
        {
            if(amount<=0||IsMaxLevel)return 0;TotalExperience+=amount;CurrentExperience+=amount;int gained=0;
            while(!IsMaxLevel&&CurrentExperience>=RequiredExperience){CurrentExperience-=RequiredExperience;Level++;PendingSelections++;gained++;LevelGained?.Invoke(Level);}
            if(IsMaxLevel)CurrentExperience=0;Changed?.Invoke();return gained;
        }
        public bool ConsumeSelection(){if(PendingSelections<=0)return false;PendingSelections--;Changed?.Invoke();return true;}
        public void ClearPending(){PendingSelections=0;Changed?.Invoke();}
        public void Reset(){Level=1;CurrentExperience=TotalExperience=PendingSelections=0;Changed?.Invoke();}
    }

    public static class ExperienceRewardService
    {
        public static int ForEnemy(string id)=>id switch{"enemy_slime"=>8,"enemy_goblin"=>10,"enemy_poison_goblin"=>14,"enemy_shaman_goblin"=>18,"enemy_elite_slime"=>35,"enemy_elite_goblin"=>45,"boss_goblin_chieftain"=>200,_=>0};
        public static int ForWave(int oneBased)=>Mathf.Max(0,oneBased)*10;
    }
}
