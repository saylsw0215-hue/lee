using System.Collections;
using System.Collections.Generic;
using HeroDefense.Battle.Combat;
using UnityEngine;

namespace HeroDefense.Heroes.Skills
{
    public interface IHeroSkillExecutor{bool ExecuteActive(HeroController hero);bool ExecuteUltimate(HeroController hero);bool ExecuteActiveAt(HeroController hero,Vector3 point);bool ExecuteUltimateAt(HeroController hero,Vector3 point);}
    public static class HeroSkillExecutorFactory{public static IHeroSkillExecutor Create(HeroArchetype type)=>type switch{HeroArchetype.Knight=>new KnightSkillExecutor(),HeroArchetype.Ranger=>new RangerSkillExecutor(),_=>new MageSkillExecutor()};}

    public sealed class KnightSkillExecutor:IHeroSkillExecutor
    {
        private readonly List<IDamageable> targets=new(8);
        public bool ExecuteActive(HeroController hero){Vector3 point=hero.CurrentTarget?.TargetTransform.localPosition??hero.transform.localPosition+Vector3.right*160;return ExecuteActiveAt(hero,point);}
        public bool ExecuteActiveAt(HeroController hero,Vector3 point){if(!hero.CollectCone(point,hero.Data.ActiveSkill.Range,70,hero.Data.ActiveSkill.MaxTargets,targets))return false;StatusEffectData stun=Load("Stun"),taunt=Load("Taunt");for(int i=0;i<targets.Count;i++){hero.DealSkillDamage(targets[i],hero.Data.AttackDamage*1.8f);hero.ApplyStatusTo(targets[i],stun,1,hero);hero.ApplyStatusTo(targets[i],taunt,1,hero);}hero.ShowEffect(hero.transform.localPosition,hero.Data.ActiveSkill.Radius*CombatUnit.PixelsPerUnit,new Color(.35f,.7f,1f,.7f));return true;}
        public bool ExecuteUltimate(HeroController hero)=>ExecuteUltimateAt(hero,hero.transform.localPosition);
        public bool ExecuteUltimateAt(HeroController hero,Vector3 point){hero.ApplySelfDamageReduction(.5f,8f);hero.ApplyAllyDamageReduction(.2f,8f);hero.Shields.Add("guardian_oath",hero.Data.MaxHealth*.2f,8f);hero.ApplyAllyShields("guardian_oath",hero.Data.MaxHealth*.08f,8f);hero.CollectNearHero(hero.Data.UltimateSkill.Radius,20,targets);for(int i=0;i<targets.Count;i++)hero.DealSkillDamage(targets[i],hero.Data.AttackDamage*1.2f);hero.ShowEffect(hero.transform.localPosition,260,new Color(.2f,.65f,1f,.75f),1f);return true;}
        private static StatusEffectData Load(string value)=>RuntimeStatusCatalog.Get(value);
    }
    public sealed class RangerSkillExecutor:IHeroSkillExecutor
    {
        public bool ExecuteActive(HeroController hero){Vector3 center;if(!hero.TryFindCrowdedCenter(out center))return false;return ExecuteActiveAt(hero,center);}
        public bool ExecuteActiveAt(HeroController hero,Vector3 center){hero.StartSkillRoutine(ArrowRain(hero,center));return true;}
        private static IEnumerator ArrowRain(HeroController hero,Vector3 center){StatusEffectData slow=RuntimeStatusCatalog.Get("Slow");for(int tick=0;tick<5;tick++){hero.DamageArea(center,hero.Data.ActiveSkill.Radius,hero.Data.AttackDamage*.6f,int.MaxValue);hero.ApplyStatusArea(center,hero.Data.ActiveSkill.Radius,slow,.2f,int.MaxValue);hero.ShowEffect(center,hero.Data.ActiveSkill.Radius*CombatUnit.PixelsPerUnit,new Color(.45f,.85f,.3f,.45f));yield return new WaitForSeconds(.6f);}}
        public bool ExecuteUltimate(HeroController hero)=>ExecuteUltimateAt(hero,hero.transform.localPosition);
        public bool ExecuteUltimateAt(HeroController hero,Vector3 point){hero.ApplyAttackBuff(.4f,.2f,8f,true);hero.RuntimeStats.Add(new StatModifier("ranger_hawkeye",CombatStat.CriticalChance,StatModifierType.Flat,.2f));hero.StartSkillRoutine(Remove(hero,8));hero.ShowEffect(hero.transform.localPosition,220,new Color(.35f,1f,.45f,.7f),1f);return true;}
        private static IEnumerator Remove(HeroController hero,float time){yield return new WaitForSeconds(time);hero.RuntimeStats.RemoveSource("ranger_hawkeye");}
    }
    public sealed class MageSkillExecutor:IHeroSkillExecutor
    {
        public bool ExecuteActive(HeroController hero){Vector3 center;if(!hero.TryFindCrowdedCenter(out center,true))return false;return ExecuteActiveAt(hero,center);}
        public bool ExecuteActiveAt(HeroController hero,Vector3 center){hero.DamageArea(center,hero.Data.ActiveSkill.Radius,hero.Data.AttackDamage*2.2f,10);hero.ApplyStatusArea(center,hero.Data.ActiveSkill.Radius,RuntimeStatusCatalog.Get("Burn"),hero.Data.AttackDamage*.2f,10);hero.ShowEffect(center,hero.Data.ActiveSkill.Radius*CombatUnit.PixelsPerUnit,new Color(1f,.25f,.05f,.75f));return true;}
        public bool ExecuteUltimate(HeroController hero){Vector3 center;if(!hero.TryFindCrowdedCenter(out center,true))return false;return ExecuteUltimateAt(hero,center);}
        public bool ExecuteUltimateAt(HeroController hero,Vector3 center){hero.DamageMeteor(center,hero.Data.UltimateSkill.Radius,hero.Data.AttackDamage*5f);hero.ShowEffect(center,hero.Data.UltimateSkill.Radius*CombatUnit.PixelsPerUnit,new Color(1f,.12f,.02f,.9f),1.2f);return true;}
    }
}
