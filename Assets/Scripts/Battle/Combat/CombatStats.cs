using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    public enum CombatStat { AttackPower, Defense, MagicDefense, AttackSpeed, MoveSpeed, AttackRange, CriticalChance, CriticalDamage, Dodge, Accuracy, ArmorPenetrationFlat, ArmorPenetrationPercent, CrowdControlResistance, StatusResistance, DamageDealt, DamageTaken, Healing }
    public enum StatModifierType { Flat, AdditivePercent, MultiplicativePercent }

    [Serializable]
    public sealed class CombatStats
    {
        [SerializeField,Min(0)] private float defense;
        [SerializeField,Min(0)] private float magicDefense;
        [SerializeField,Range(0,1)] private float criticalChance=.05f;
        [SerializeField,Min(1)] private float criticalDamageMultiplier=1.5f;
        [SerializeField,Range(0,.75f)] private float dodgeChance;
        [SerializeField,Range(0,.75f)] private float accuracy;
        [SerializeField,Min(0)] private float armorPenetrationFlat;
        [SerializeField,Range(0,1)] private float armorPenetrationPercent;
        [SerializeField,Range(0,.8f)] private float crowdControlResistance;
        [SerializeField,Range(0,.8f)] private float statusEffectResistance;
        [SerializeField,Min(0)] private float maxShield;
        public float Defense=>defense; public float MagicDefense=>magicDefense; public float CriticalChance=>criticalChance;
        public float CriticalDamageMultiplier=>Mathf.Max(1,criticalDamageMultiplier); public float DodgeChance=>dodgeChance; public float Accuracy=>accuracy;
        public float ArmorPenetrationFlat=>armorPenetrationFlat; public float ArmorPenetrationPercent=>armorPenetrationPercent;
        public float CrowdControlResistance=>crowdControlResistance; public float StatusEffectResistance=>statusEffectResistance; public float MaxShield=>maxShield;
        public void Configure(float armor,float magicArmor,float critical,float criticalMultiplier,float dodge,float hit,float flatPen,float percentPen,float ccResist,float statusResist,float shield=0)
        {defense=Mathf.Max(0,armor);magicDefense=Mathf.Max(0,magicArmor);criticalChance=Mathf.Clamp01(critical);criticalDamageMultiplier=Mathf.Max(1,criticalMultiplier);dodgeChance=Mathf.Clamp(dodge,0,.75f);accuracy=Mathf.Clamp(hit,0,.75f);armorPenetrationFlat=Mathf.Max(0,flatPen);armorPenetrationPercent=Mathf.Clamp01(percentPen);crowdControlResistance=Mathf.Clamp(ccResist,0,.8f);statusEffectResistance=Mathf.Clamp(statusResist,0,.8f);maxShield=Mathf.Max(0,shield);}
        public bool Validate(out string reason){if(defense<0||magicDefense<0||criticalChance<0||criticalChance>1||criticalDamageMultiplier<1||dodgeChance<0||dodgeChance>.75f||crowdControlResistance<0||crowdControlResistance>.8f){reason="Advanced combat stats are outside their valid range.";return false;}reason=string.Empty;return true;}
    }

    public readonly struct StatModifier
    {
        public readonly string SourceId; public readonly CombatStat Stat; public readonly StatModifierType Type; public readonly float Value;
        public StatModifier(string sourceId,CombatStat stat,StatModifierType type,float value){SourceId=sourceId;Stat=stat;Type=type;Value=value;}
    }

    /// <summary>Runtime-only combat stat snapshot. ScriptableObject source data is never mutated.</summary>
    public sealed class RuntimeCombatStats
    {
        private readonly CombatStats source; private readonly List<StatModifier> modifiers=new(12);
        public RuntimeCombatStats(CombatStats data){source=data??new CombatStats();}
        public float Defense=>Get(CombatStat.Defense,source.Defense); public float MagicDefense=>Get(CombatStat.MagicDefense,source.MagicDefense);
        public float CriticalChance=>Mathf.Clamp01(Get(CombatStat.CriticalChance,source.CriticalChance)); public float CriticalDamage=>Mathf.Max(1,Get(CombatStat.CriticalDamage,source.CriticalDamageMultiplier));
        public float Dodge=>Mathf.Clamp(Get(CombatStat.Dodge,source.DodgeChance),0,.75f); public float Accuracy=>Mathf.Clamp01(Get(CombatStat.Accuracy,source.Accuracy));
        public float ArmorPenetrationFlat=>Mathf.Max(0,Get(CombatStat.ArmorPenetrationFlat,source.ArmorPenetrationFlat)); public float ArmorPenetrationPercent=>Mathf.Clamp01(Get(CombatStat.ArmorPenetrationPercent,source.ArmorPenetrationPercent));
        public float CrowdControlResistance=>Mathf.Clamp(Get(CombatStat.CrowdControlResistance,source.CrowdControlResistance),0,.8f); public float StatusResistance=>Mathf.Clamp(Get(CombatStat.StatusResistance,source.StatusEffectResistance),0,.8f);
        public float AttackPowerMultiplier=>Mathf.Max(0,Get(CombatStat.AttackPower,1)); public float AttackSpeedMultiplier=>Mathf.Max(.05f,Get(CombatStat.AttackSpeed,1)); public float MoveSpeedMultiplier=>Mathf.Max(.05f,Get(CombatStat.MoveSpeed,1)); public float AttackRangeMultiplier=>Mathf.Max(.05f,Get(CombatStat.AttackRange,1)); public float DamageDealtMultiplier=>Mathf.Max(0,Get(CombatStat.DamageDealt,1)); public float DamageTakenMultiplier=>Mathf.Max(0,Get(CombatStat.DamageTaken,1)); public float HealingMultiplier=>Mathf.Max(0,Get(CombatStat.Healing,1));
        public void Add(StatModifier value){Remove(value.SourceId,value.Stat);modifiers.Add(value);} public void RemoveSource(string sourceId){for(int i=modifiers.Count-1;i>=0;i--)if(modifiers[i].SourceId==sourceId)modifiers.RemoveAt(i);} public void Clear()=>modifiers.Clear();
        private void Remove(string sourceId,CombatStat stat){for(int i=modifiers.Count-1;i>=0;i--)if(modifiers[i].SourceId==sourceId&&modifiers[i].Stat==stat)modifiers.RemoveAt(i);}
        private float Get(CombatStat stat,float basis){float flat=0,add=0,multiply=1;for(int i=0;i<modifiers.Count;i++){var m=modifiers[i];if(m.Stat!=stat)continue;if(m.Type==StatModifierType.Flat)flat+=m.Value;else if(m.Type==StatModifierType.AdditivePercent)add+=m.Value;else multiply*=1+m.Value;}return (basis+flat)*(1+add)*multiply;}
    }
}
