using System;
using System.Collections.Generic;
using HeroDefense.Battle.Combat;
using UnityEngine;

namespace HeroDefense.Progression
{
    public enum UpgradeCategory{Hero,HeroSkill,HeroUltimate,Unit,Building,Economy,Base,Global,Special}
    public enum UpgradeRarity{Common,Rare,Epic,Legendary}
    public enum UpgradeEffectKind{AttackPercent,DefenseFlat,MagicDefenseFlat,AttackSpeedPercent,CriticalFlat,ArmorPenetrationFlat,HeroHealthPercent,HeroAttackPercent,ProductionSpeed,KillGold,WaveGold,BuildingCost,BaseHealthPercent,BaseHeal,SkillPower,Special}

    [Serializable]public sealed class UpgradeEffectData{[SerializeField]private UpgradeEffectKind kind;[SerializeField]private float value;public UpgradeEffectKind Kind=>kind;public float Value=>value;public UpgradeEffectData(UpgradeEffectKind type,float amount){kind=type;value=amount;}}
    [Serializable]public sealed class UpgradeRequirementData{[SerializeField]private int battleLevel;[SerializeField]private string requiredUpgradeId;[SerializeField]private int requiredUpgradeLevel;public int BattleLevel=>battleLevel;public string RequiredUpgradeId=>requiredUpgradeId;public int RequiredUpgradeLevel=>requiredUpgradeLevel;public UpgradeRequirementData(int level=1,string id=null,int upgradeLevel=0){battleLevel=level;requiredUpgradeId=id;requiredUpgradeLevel=upgradeLevel;}}

    [CreateAssetMenu(fileName="UpgradeData",menuName="Hero Defense/Progression/Upgrade Data")]
    public sealed class UpgradeData:ScriptableObject
    {
        [SerializeField]private string upgradeId,displayName,description,heroId;[SerializeField]private UpgradeCategory category;[SerializeField]private UpgradeRarity rarity;[SerializeField]private int maxLevel=1;[SerializeField]private float selectionWeight=1;[SerializeField]private UpgradeEffectData[] effects;[SerializeField]private UpgradeRequirementData requirement;
        public string UpgradeId=>upgradeId;public string DisplayName=>displayName;public string Description=>description;public string HeroId=>heroId;public UpgradeCategory Category=>category;public UpgradeRarity Rarity=>rarity;public int MaxLevel=>maxLevel;public float SelectionWeight=>selectionWeight;public UpgradeEffectData[] Effects=>effects;public UpgradeRequirementData Requirement=>requirement;
        public void Configure(string id,string title,string details,UpgradeCategory group,UpgradeRarity quality,int levels,float weight,UpgradeEffectData[] values,string requiredHero=null,UpgradeRequirementData condition=null){upgradeId=id;displayName=title;description=details;category=group;rarity=quality;maxLevel=Mathf.Max(1,levels);selectionWeight=Mathf.Max(0,weight);effects=values;heroId=requiredHero;requirement=condition;}
        public bool Validate(out string reason){if(string.IsNullOrWhiteSpace(upgradeId)||string.IsNullOrWhiteSpace(displayName)||maxLevel<1||selectionWeight<0||effects==null||effects.Length==0){reason="Upgrade metadata and effects are required.";return false;}reason=string.Empty;return true;}
    }

    public sealed class UpgradeRuntimeState
    {
        public UpgradeData Data{get;}public int Level{get;private set;}public int FirstBattleLevel{get;private set;}public int LastBattleLevel{get;private set;}
        public UpgradeRuntimeState(UpgradeData data){Data=data;}public bool CanLevel=>Level<Data.MaxLevel;
        public bool Add(int battleLevel){if(!CanLevel)return false;if(Level==0)FirstBattleLevel=battleLevel;Level++;LastBattleLevel=battleLevel;return true;}
    }

    public sealed class BattleUpgradeInventory
    {
        private readonly Dictionary<string,UpgradeRuntimeState> states=new();public IEnumerable<UpgradeRuntimeState> Selected=>states.Values;public int SelectionCount{get;private set;}
        public int LevelOf(string id)=>states.TryGetValue(id,out var value)?value.Level:0;
        public bool CanSelect(UpgradeData data)=>data!=null&&LevelOf(data.UpgradeId)<data.MaxLevel;
        public bool Select(UpgradeData data,int battleLevel){if(!CanSelect(data))return false;if(!states.TryGetValue(data.UpgradeId,out var state)){state=new UpgradeRuntimeState(data);states.Add(data.UpgradeId,state);}if(!state.Add(battleLevel))return false;SelectionCount++;return true;}
        public void Clear(){states.Clear();SelectionCount=0;}
    }

    public interface IRandomProvider{float Value();int Range(int min,int max);}
    public sealed class SeededRandomProvider:IRandomProvider{private readonly System.Random random;public SeededRandomProvider(int seed)=>random=new System.Random(seed);public float Value()=>(float)random.NextDouble();public int Range(int min,int max)=>random.Next(min,max);}

    public sealed class UpgradeCandidateService
    {
        private readonly IRandomProvider random;public UpgradeCandidateService(IRandomProvider provider)=>random=provider;
        public List<UpgradeData> Roll(IReadOnlyList<UpgradeData> source,BattleUpgradeInventory inventory,int battleLevel,string heroId,ISet<string> excluded=null,int count=3)
        {
            var pool=new List<UpgradeData>(source.Count);for(int i=0;i<source.Count;i++){var d=source[i];if(!inventory.CanSelect(d)||(excluded!=null&&excluded.Contains(d.UpgradeId))||(!string.IsNullOrEmpty(d.HeroId)&&d.HeroId!=heroId))continue;var r=d.Requirement;if(r!=null&&(battleLevel<r.BattleLevel||(!string.IsNullOrEmpty(r.RequiredUpgradeId)&&inventory.LevelOf(r.RequiredUpgradeId)<r.RequiredUpgradeLevel)))continue;pool.Add(d);}
            var result=new List<UpgradeData>(count);while(result.Count<count&&pool.Count>0){float total=0;for(int i=0;i<pool.Count;i++)total+=pool[i].SelectionWeight*RarityWeight(pool[i].Rarity,battleLevel);float roll=random.Value()*total,acc=0;int picked=pool.Count-1;for(int i=0;i<pool.Count;i++){acc+=pool[i].SelectionWeight*RarityWeight(pool[i].Rarity,battleLevel);if(roll<=acc){picked=i;break;}}result.Add(pool[picked]);pool.RemoveAt(picked);}return result;
        }
        private static float RarityWeight(UpgradeRarity rarity,int level){float[] weights=level<=5?new[]{75f,23f,2f,0f}:level<=10?new[]{58f,32f,9f,1f}:level<=15?new[]{45f,35f,17f,3f}:new[]{30f,38f,25f,7f};return weights[(int)rarity];}
    }

    /// <summary>Single-battle modifier projection consumed by existing and newly spawned combatants.</summary>
    public sealed class BattleModifierRepository
    {
        public static BattleModifierRepository Current{get;set;}public float ProductionIntervalMultiplier=>Mathf.Max(.5f,1-ProductionSpeedReduction);public float ProductionSpeedReduction{get;private set;}public float KillGoldMultiplier{get;private set;}=1;public float WaveGoldMultiplier{get;private set;}=1;public float BuildingCostMultiplier{get;private set;}=1;
        private readonly List<UpgradeRuntimeState> selected=new();public IReadOnlyList<UpgradeRuntimeState> Selected=>selected;
        public void Rebuild(BattleUpgradeInventory inventory){selected.Clear();ProductionSpeedReduction=0;KillGoldMultiplier=WaveGoldMultiplier=BuildingCostMultiplier=1;foreach(var state in inventory.Selected){selected.Add(state);for(int l=0;l<state.Level;l++)for(int i=0;i<state.Data.Effects.Length;i++){var e=state.Data.Effects[i];if(e.Kind==UpgradeEffectKind.ProductionSpeed)ProductionSpeedReduction=Mathf.Min(.5f,ProductionSpeedReduction+e.Value);else if(e.Kind==UpgradeEffectKind.KillGold)KillGoldMultiplier+=e.Value;else if(e.Kind==UpgradeEffectKind.WaveGold)WaveGoldMultiplier+=e.Value;else if(e.Kind==UpgradeEffectKind.BuildingCost)BuildingCostMultiplier=Mathf.Max(.6f,BuildingCostMultiplier-e.Value);}}}
        public void Apply(RuntimeCombatStats stats,bool hero,string unitId=null){if(stats==null)return;for(int s=0;s<selected.Count;s++){var state=selected[s];for(int level=1;level<=state.Level;level++)for(int i=0;i<state.Data.Effects.Length;i++){var e=state.Data.Effects[i];string source=$"phase7:{state.Data.UpgradeId}:{level}";if(e.Kind==UpgradeEffectKind.AttackPercent||hero&&e.Kind==UpgradeEffectKind.HeroAttackPercent)stats.Add(new StatModifier(source,CombatStat.AttackPower,StatModifierType.AdditivePercent,e.Value));else if(e.Kind==UpgradeEffectKind.DefenseFlat)stats.Add(new StatModifier(source,CombatStat.Defense,StatModifierType.Flat,e.Value));else if(e.Kind==UpgradeEffectKind.MagicDefenseFlat)stats.Add(new StatModifier(source,CombatStat.MagicDefense,StatModifierType.Flat,e.Value));else if(e.Kind==UpgradeEffectKind.AttackSpeedPercent)stats.Add(new StatModifier(source,CombatStat.AttackSpeed,StatModifierType.AdditivePercent,e.Value));else if(e.Kind==UpgradeEffectKind.CriticalFlat)stats.Add(new StatModifier(source,CombatStat.CriticalChance,StatModifierType.Flat,e.Value));else if(e.Kind==UpgradeEffectKind.ArmorPenetrationFlat)stats.Add(new StatModifier(source,CombatStat.ArmorPenetrationFlat,StatModifierType.Flat,e.Value));}}}
        public void Clear(){selected.Clear();ProductionSpeedReduction=0;KillGoldMultiplier=WaveGoldMultiplier=BuildingCostMultiplier=1;}
    }
}
