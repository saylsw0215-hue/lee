using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    public enum DamageType { Physical, Magical, True }
    public interface ICombatRandom { float Value(); }
    public sealed class UnityCombatRandom:ICombatRandom { public float Value()=>Random.value; }

    public readonly struct DamageResult
    {
        public readonly bool WasApplied,WasDodged,WasCritical,WasBlocked; public readonly float RawDamage,MitigatedDamage,ShieldAbsorbed,HealthDamage; public readonly bool TargetDied;
        public DamageResult(bool applied,bool dodged,bool critical,bool blocked,float raw,float mitigated,float shield,float health,bool died){WasApplied=applied;WasDodged=dodged;WasCritical=critical;WasBlocked=blocked;RawDamage=raw;MitigatedDamage=mitigated;ShieldAbsorbed=shield;HealthDamage=health;TargetDied=died;}
    }

    public interface IAdvancedCombatant:IDamageable
    {
        RuntimeCombatStats RuntimeStats{get;} StatusEffectController Statuses{get;} ShieldController Shields{get;} HealthComponent Health{get;}
        DamageResult ApplyAdvancedDamage(DamageInfo info);
    }

    /// <summary>Single deterministic ordering for dodge, critical, penetration, defense and shield resolution.</summary>
    public sealed class DamageCalculationService
    {
        private readonly ICombatRandom random; public DamageCalculationService(ICombatRandom value=null){random=value??new UnityCombatRandom();}
        public static float EffectiveDefense(float defense,float percentPen,float flatPen)=>Mathf.Max(0,Mathf.Max(0,defense)*(1-Mathf.Clamp01(percentPen))-Mathf.Max(0,flatPen));
        public static float ApplyDefense(float damage,float defense){if(damage<=0)return 0;float reduced=damage*(1-(defense/(100+defense)));return Mathf.Max(damage*.05f,reduced);}
        public DamageResult Calculate(DamageInfo info,RuntimeCombatStats source,RuntimeCombatStats target,StatusEffectController statuses,ShieldController shields,bool targetAlive)
        {
            float raw=Mathf.Max(0,info.Amount);if(!targetAlive||raw<=0||statuses!=null&&statuses.IsInvincible)return new DamageResult(false,false,false,true,raw,0,0,0,false);
            float dodge=target?.Dodge??0;float accuracy=source?.Accuracy??0;if(info.CanDodge&&info.DamageType==DamageType.Physical&&random.Value()<Mathf.Clamp(dodge-accuracy,0,.75f))return new DamageResult(false,true,false,false,raw,0,0,0,false);
            float value=raw*(source?.DamageDealtMultiplier??1);bool critical=info.CanCritical&&info.DamageType!=DamageType.True&&!info.IsDamageOverTime&&random.Value()<Mathf.Clamp01((source?.CriticalChance??0)+info.CriticalChanceBonus);if(critical)value*=Mathf.Max(1,(source?.CriticalDamage??1.5f)+info.CriticalDamageBonus);
            float mitigated=value;if(info.DamageType!=DamageType.True){float defense=info.DamageType==DamageType.Physical?(target?.Defense??0):(target?.MagicDefense??0);float effective=EffectiveDefense(defense,(source?.ArmorPenetrationPercent??0)+info.ArmorPenetrationPercent,(source?.ArmorPenetrationFlat??0)+info.ArmorPenetrationFlat);mitigated=ApplyDefense(value,effective);}
            if(info.DamageType==DamageType.Magical&&statuses!=null&&statuses.Has(StatusId.Shock))mitigated*=1.15f;mitigated*=target?.DamageTakenMultiplier??1;
            float absorbed=shields?.Absorb(mitigated)??0;float health=Mathf.Max(0,mitigated-absorbed);return new DamageResult(health>0||absorbed>0,false,critical,health<=0,raw,mitigated,absorbed,health,false);
        }
    }

    public static class AdvancedCombatResolver
    {
        public static DamageCalculationService Service{get;set;}=new();
        public static DamageResult Apply(IAdvancedCombatant target,DamageInfo info)
        {if(target==null)return default;RuntimeCombatStats source=null;if(info.Source!=null){var unit=info.Source.GetComponent<CombatUnit>();if(unit!=null)source=unit.RuntimeStats;else{var hero=info.Source.GetComponent<HeroDefense.Heroes.HeroController>();if(hero!=null)source=hero.RuntimeStats;}}var result=Service.Calculate(info,source,target.RuntimeStats,target.Statuses,target.Shields,target.IsAlive);if(result.HealthDamage>0){target.Health.ApplyDamage(info,result.HealthDamage);result=new DamageResult(result.WasApplied,result.WasDodged,result.WasCritical,result.WasBlocked,result.RawDamage,result.MitigatedDamage,result.ShieldAbsorbed,result.HealthDamage,!target.IsAlive);}return result;}
    }
}
