using System;
using HeroDefense.Battle.Combat;
using UnityEngine;

namespace HeroDefense.Battle.Waves
{
    public enum WaveState { NotStarted,Preparing,Spawning,Fighting,Completed,StageCleared,Failed }

    [Serializable]
    public sealed class WaveSpawnGroup
    {
        [SerializeField] private UnitData enemyData;[SerializeField,Min(1)] private int count=1;[SerializeField,Min(0)] private float initialDelay;[SerializeField,Min(0)] private float spawnInterval=1f;[SerializeField,Min(0)] private int spawnPointIndex;
        public UnitData EnemyData=>enemyData;public int Count=>count;public float InitialDelay=>initialDelay;public float SpawnInterval=>spawnInterval;public int SpawnPointIndex=>spawnPointIndex;
        public WaveSpawnGroup(UnitData enemy,int amount,float delay,float interval,int point=0){enemyData=enemy;count=amount;initialDelay=delay;spawnInterval=interval;spawnPointIndex=point;}
        public bool Validate(int spawnPointCount,out string reason){if(enemyData==null){reason="Enemy data is required.";return false;}if(count<1||initialDelay<0||spawnInterval<0){reason="Count must be positive and timing cannot be negative.";return false;}if(spawnPointIndex<0||spawnPointIndex>=spawnPointCount){reason="Spawn point index is outside the configured range.";return false;}reason=string.Empty;return true;}
    }

    [CreateAssetMenu(fileName="WaveData",menuName="Hero Defense/Waves/Wave Data")]
    public sealed class WaveData:ScriptableObject
    {
        [SerializeField] private string waveId;[SerializeField] private string displayName;[SerializeField,Min(0)] private float preparationDuration=5f;[SerializeField,Min(0)] private float completionDelay=2f;[SerializeField,Min(0)] private int clearRewardGold;[SerializeField] private bool eliteWave;[SerializeField] private bool bossWave;[SerializeField] private string announcement;[SerializeField] private WaveSpawnGroup[] spawnGroups;
        public string WaveId=>waveId;public string DisplayName=>displayName;public float PreparationDuration=>preparationDuration;public float CompletionDelay=>completionDelay;public int ClearRewardGold=>clearRewardGold;public bool IsEliteWave=>eliteWave;public bool IsBossWave=>bossWave;public string Announcement=>announcement;public WaveSpawnGroup[] SpawnGroups=>spawnGroups;
        public int TotalEnemyCount{get{int total=0;if(spawnGroups!=null)for(int i=0;i<spawnGroups.Length;i++)total+=spawnGroups[i]?.Count??0;return total;}}
        public bool Validate(int spawnPointCount,out string reason){reason=string.Empty;if(string.IsNullOrWhiteSpace(waveId)||preparationDuration<0||completionDelay<0||clearRewardGold<0||spawnGroups==null||spawnGroups.Length==0){reason="Wave metadata, timing, reward, and spawn groups must be valid.";return false;}for(int i=0;i<spawnGroups.Length;i++)if(spawnGroups[i]==null||!spawnGroups[i].Validate(spawnPointCount,out reason))return false;if(bossWave){bool found=false;for(int i=0;i<spawnGroups.Length;i++)if(spawnGroups[i].EnemyData.UnitId.StartsWith("boss_")){found=true;break;}if(!found){reason="Boss wave must contain a boss unit.";return false;}}return true;}
        public void Configure(string id,string name,float preparation,float completion,int reward,bool elite,bool boss,string message,WaveSpawnGroup[] groups){waveId=id;displayName=name;preparationDuration=preparation;completionDelay=completion;clearRewardGold=reward;eliteWave=elite;bossWave=boss;announcement=message;spawnGroups=groups;}
    }
}
