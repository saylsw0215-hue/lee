using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    public readonly struct HealingInfo{public readonly GameObject Source;public readonly IAdvancedCombatant Target;public readonly float Amount;public readonly string SkillId;public HealingInfo(GameObject source,IAdvancedCombatant target,float amount,string skillId=null){Source=source;Target=target;Amount=Mathf.Max(0,amount);SkillId=skillId;}}
    public readonly struct HealingResult{public readonly bool Applied;public readonly float Requested,Restored;public HealingResult(bool applied,float requested,float restored){Applied=applied;Requested=requested;Restored=restored;}}
    public static class HealingService
    {
        public static HealingResult Apply(HealingInfo info){if(info.Target==null||!info.Target.IsAlive||info.Amount<=0)return default;float before=info.Target.Health.CurrentHealth;float amount=info.Amount*info.Target.RuntimeStats.HealingMultiplier;info.Target.Health.Heal(amount);float restored=info.Target.Health.CurrentHealth-before;return new HealingResult(restored>0,info.Amount,restored);}
    }
}
