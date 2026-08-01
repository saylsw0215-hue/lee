using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    public enum Team { Player, Enemy }
    public enum CombatUnitState { Inactive, Idle, Moving, Attacking, Dead }

    /// <summary>Immutable description of one damage application.</summary>
    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public Team SourceTeam { get; }
        public GameObject Source { get; }
        public GameObject Target { get; }
        public DamageType DamageType { get; }
        public bool CanCritical { get; }
        public bool CanDodge { get; }
        public bool IsSkill { get; }
        public bool IsUltimate { get; }
        public bool IsDamageOverTime { get; }
        public float CriticalChanceBonus { get; }
        public float CriticalDamageBonus { get; }
        public float ArmorPenetrationFlat { get; }
        public float ArmorPenetrationPercent { get; }
        public string SkillId { get; }
        public int HitSequence { get; }

        public DamageInfo(float amount, Team sourceTeam, GameObject source = null)
        {
            Amount=Mathf.Max(0,amount);SourceTeam=sourceTeam;Source=source;Target=null;DamageType=DamageType.Physical;CanCritical=false;CanDodge=false;IsSkill=false;IsUltimate=false;IsDamageOverTime=false;CriticalChanceBonus=CriticalDamageBonus=ArmorPenetrationFlat=ArmorPenetrationPercent=0;SkillId=null;HitSequence=0;
        }

        public DamageInfo(float amount,Team sourceTeam,GameObject source,DamageType type,bool canCritical=true,bool canDodge=true,bool isSkill=false,bool isUltimate=false,bool isDot=false,string skillId=null,int hitSequence=0,float criticalChanceBonus=0,float criticalDamageBonus=0,float flatPenetration=0,float percentPenetration=0,GameObject target=null)
        {Amount=Mathf.Max(0,amount);SourceTeam=sourceTeam;Source=source;Target=target;DamageType=type;CanCritical=canCritical;CanDodge=canDodge;IsSkill=isSkill;IsUltimate=isUltimate;IsDamageOverTime=isDot;SkillId=skillId;HitSequence=hitSequence;CriticalChanceBonus=criticalChanceBonus;CriticalDamageBonus=criticalDamageBonus;ArmorPenetrationFlat=flatPenetration;ArmorPenetrationPercent=percentPenetration;}
    }

    public interface IDamageable
    {
        Team Team { get; }
        bool IsAlive { get; }
        Transform TargetTransform { get; }
        float CollisionRadius { get; }
        void TakeDamage(DamageInfo damageInfo);
    }

    /// <summary>Allocation-free attack interval timer used by combat units.</summary>
    public sealed class AttackCooldown
    {
        private float remaining;
        public float Remaining => remaining;
        public void Reset() => remaining = 0f;
        public void Tick(float deltaTime) => remaining = Mathf.Max(0f, remaining - Mathf.Max(0f, deltaTime));
        public bool TryConsume(float interval)
        {
            if (remaining > 0f) return false;
            remaining = Mathf.Max(0.01f, interval);
            return true;
        }
    }
}
