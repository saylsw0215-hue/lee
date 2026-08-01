using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    public enum StatusEffectType { Buff,Debuff,CrowdControl,DamageOverTime,Shield }
    public enum StatusRefreshRule { IgnoreNew,RefreshDuration,AddDuration,ReplaceIfStronger,Stack }
    public enum StatusApplyResult { Applied,Refreshed,Stacked,RejectedImmune,RejectedResisted,RejectedInvalidTarget }
    public static class StatusId { public const string Stun="status_stun",Freeze="status_freeze",Burn="status_burn",Poison="status_poison",Shock="status_shock",Slow="status_slow",Silence="status_silence",Taunt="status_taunt",Invincible="status_invincible",ShamanPower="status_shaman_power"; }

    public sealed class StatusEffectInstance
    {public StatusEffectData Data;public float Remaining,TickRemaining,Potency;public int Stacks;public GameObject Source;public IDamageable TauntSource;}

    /// <summary>One allocation-conscious timer owner per combatant; no status creates its own Update.</summary>
    public sealed class StatusEffectController
    {
        private readonly List<StatusEffectInstance> active=new(10);private readonly IAdvancedCombatant owner;public event Action Changed;public event Action<StatusEffectData,StatusApplyResult> Applied;public event Action<StatusEffectData> Ended;
        public StatusEffectController(IAdvancedCombatant value){owner=value;}public IReadOnlyList<StatusEffectInstance> Active=>active;public bool IsStunned=>Has(StatusId.Stun);public bool IsFrozen=>Has(StatusId.Freeze);public bool IsSilenced=>Has(StatusId.Silence);public bool IsInvincible=>Has(StatusId.Invincible);public IDamageable TauntTarget{get{var x=Find(StatusId.Taunt);return x?.TauntSource!=null&&x.TauntSource.IsAlive?x.TauntSource:null;}}
        public float MoveMultiplier{get{if(IsFrozen)return .05f;var slow=Find(StatusId.Slow);return slow==null?1:Mathf.Clamp(1-slow.Potency,.05f,1);}}public float AttackSpeedMultiplier=>Has(StatusId.Shock) ? .85f : 1f;
        public bool Has(string id)=>Find(id)!=null;
        public StatusApplyResult Apply(StatusEffectData data,GameObject source=null,float potency=-1,IDamageable tauntSource=null)
        {
            if(data==null||owner==null||!owner.IsAlive)return StatusApplyResult.RejectedInvalidTarget;if(IsInvincible&&data.EffectType!=StatusEffectType.Buff)return StatusApplyResult.RejectedImmune;
            bool cc=data.EffectType==StatusEffectType.CrowdControl;float resistance=data.ResistanceApplies?(cc?owner.RuntimeStats.CrowdControlResistance:owner.RuntimeStats.StatusResistance):0;float duration=data.Duration*Mathf.Max(.2f,1-Mathf.Clamp(resistance,0,.8f));StatusEffectInstance current=Find(data.EffectId);float strength=potency>=0?potency:data.Potency;
            if(current!=null){StatusApplyResult result=StatusApplyResult.Refreshed;switch(data.RefreshRule){case StatusRefreshRule.IgnoreNew:return StatusApplyResult.RejectedResisted;case StatusRefreshRule.AddDuration:current.Remaining+=duration;break;case StatusRefreshRule.ReplaceIfStronger:if(strength<current.Potency)return StatusApplyResult.RejectedResisted;current.Potency=strength;current.Remaining=duration;break;case StatusRefreshRule.Stack:current.Stacks=Mathf.Min(data.MaxStacks,current.Stacks+1);current.Remaining=duration;result=StatusApplyResult.Stacked;break;default:current.Remaining=duration;break;}current.Source=source;current.TauntSource=tauntSource;Applied?.Invoke(data,result);Changed?.Invoke();return result;}
            current=new StatusEffectInstance{Data=data,Remaining=duration,TickRemaining=data.TickInterval,Potency=strength,Stacks=1,Source=source,TauntSource=tauntSource};active.Add(current);if(data.EffectId==StatusId.ShamanPower)owner.RuntimeStats.Add(new StatModifier(StatusId.ShamanPower,CombatStat.AttackPower,StatModifierType.AdditivePercent,strength));Applied?.Invoke(data,StatusApplyResult.Applied);Changed?.Invoke();return StatusApplyResult.Applied;
        }
        public void Tick(float dt)
        {if(dt<=0||owner==null||!owner.IsAlive)return;for(int i=active.Count-1;i>=0;i--){var effect=active[i];effect.Remaining-=dt;if(effect.Data.EffectType==StatusEffectType.DamageOverTime){effect.TickRemaining-=dt;while(effect.TickRemaining<=0&&effect.Remaining>=-.001f){effect.TickRemaining+=effect.Data.TickInterval;float amount=effect.Potency*effect.Stacks;DamageType type=effect.Data.EffectId==StatusId.Poison?DamageType.True:DamageType.Magical;owner.ApplyAdvancedDamage(new DamageInfo(amount,owner.Team==Team.Player?Team.Enemy:Team.Player,effect.Source,type,false,false,false,false,true,effect.Data.EffectId));if(!owner.IsAlive)break;}}if(effect.Remaining<=0){active.RemoveAt(i);owner.RuntimeStats.RemoveSource(effect.Data.EffectId);Ended?.Invoke(effect.Data);Changed?.Invoke();}}}
        public void Remove(string id){for(int i=active.Count-1;i>=0;i--)if(active[i].Data.EffectId==id){var data=active[i].Data;active.RemoveAt(i);owner.RuntimeStats.RemoveSource(id);Ended?.Invoke(data);Changed?.Invoke();}}public void Clear(){if(active.Count==0)return;for(int i=0;i<active.Count;i++)owner.RuntimeStats.RemoveSource(active[i].Data.EffectId);active.Clear();Changed?.Invoke();}
        private StatusEffectInstance Find(string id){for(int i=0;i<active.Count;i++)if(active[i].Data.EffectId==id)return active[i];return null;}
    }
}
