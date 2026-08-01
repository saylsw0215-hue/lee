using HeroDefense.Battle.Waves;
using UnityEngine;

namespace HeroDefense.Battle.Stages
{
    [CreateAssetMenu(fileName="StageData",menuName="Hero Defense/Stages/Stage Data")]
    public sealed class StageData:ScriptableObject
    {
        [SerializeField] private string stageId;[SerializeField] private string displayName;[SerializeField] private string description;[SerializeField] private int startingGold=500;[SerializeField] private int baseMaxHealth=100;[SerializeField,Min(0)] private int victoryReward;[SerializeField] private string difficulty;[SerializeField] private Color backgroundColor=new(.2f,.38f,.22f);[SerializeField] private WaveData[] waves;[SerializeField,Min(.1f)]private float enemyHealthMultiplier=1;[SerializeField,Min(.1f)]private float enemyDamageMultiplier=1;
        public string StageId=>stageId;public string DisplayName=>displayName;public string Description=>description;public int StartingGold=>startingGold;public int BaseMaxHealth=>baseMaxHealth;public int VictoryReward=>victoryReward;public string Difficulty=>difficulty;public Color BackgroundColor=>backgroundColor;public WaveData[] Waves=>waves;public int WaveCount=>waves?.Length??0;public float EnemyHealthMultiplier=>Mathf.Max(.1f,enemyHealthMultiplier);public float EnemyDamageMultiplier=>Mathf.Max(.1f,enemyDamageMultiplier);
        public bool Validate(out string reason){reason=string.Empty;if(string.IsNullOrWhiteSpace(stageId)||startingGold<0||baseMaxHealth<=0||victoryReward<0||waves==null||waves.Length==0){reason="Stage metadata, economy, base health, and waves are required.";return false;}for(int i=0;i<waves.Length;i++){if(waves[i]==null||!waves[i].Validate(2,out reason))return false;}if(!waves[waves.Length-1].IsBossWave){reason="The final wave must be a boss wave.";return false;}return true;}
        public void Configure(string id,string name,string details,int gold,int health,int reward,string recommended,Color background,WaveData[] waveList){stageId=id;displayName=name;description=details;startingGold=gold;baseMaxHealth=health;victoryReward=reward;difficulty=recommended;backgroundColor=background;waves=waveList;}
        public void ConfigureBalance(float enemyHealth,float enemyDamage){enemyHealthMultiplier=Mathf.Max(.1f,enemyHealth);enemyDamageMultiplier=Mathf.Max(.1f,enemyDamage);}
    }
}
