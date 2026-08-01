using HeroDefense.Battle;
using HeroDefense.Battle.Statistics;
using HeroDefense.Heroes.Selection;
using HeroDefense.Heroes.Skills;
using HeroDefense.UI.Heroes;
using UnityEngine;

namespace HeroDefense.Heroes
{
    /// <summary>Creates exactly one selected hero, coordinates reset/outcome, and owns its HUD/effect pool.</summary>
    public sealed class HeroSpawnManager:MonoBehaviour
    {
        public HeroController Hero{get;private set;}public SkillAimingController Aiming{get;private set;}private BattleCombatController combat;private BattleStatistics statistics;private HeroEffectPool effects;
        public void Initialize(RectTransform safe,BattleSessionState session,PauseController pause,BattleCombatController battleCombat,BattleStatistics stats)
        {
            combat=battleCombat;statistics=stats;if(HeroSelectionService.Instance==null)new GameObject("HeroSelectionService",typeof(HeroSelectionService));HeroData data=HeroSelectionService.Instance.GetSelectedOrDefault();statistics.SelectHero(data.HeroId);
            var effectObject=new GameObject("HeroEffectPool",typeof(HeroEffectPool));effectObject.transform.SetParent(combat.World,false);effects=effectObject.GetComponent<HeroEffectPool>();
            var heroObject=new GameObject("SelectedHero",typeof(RectTransform),typeof(HeroDefense.Battle.Combat.HealthComponent),typeof(HeroController));heroObject.transform.SetParent(combat.World,false);Hero=heroObject.GetComponent<HeroController>();Hero.Initialize(data,combat.Registry,session,pause,combat,stats,effects,new Vector2(-590,-70));
            var aimObject=new GameObject("SkillAimingController",typeof(SkillAimingController));aimObject.transform.SetParent(safe,false);Aiming=aimObject.GetComponent<SkillAimingController>();Aiming.Initialize(safe,combat.World,Hero);
            var hudObject=new GameObject("HeroHudController",typeof(HeroHudController));hudObject.transform.SetParent(safe,false);hudObject.GetComponent<HeroHudController>().Initialize(safe,Hero,Aiming);combat.BattleReset+=OnReset;
        }
        private void OnReset(){Aiming?.Cancel();statistics.SelectHero(Hero.Data.HeroId);Hero.ResetHero();}
        public void OnVictory(){Aiming?.Cancel();Hero.SetOutcome(true);}public void OnDefeat(){Aiming?.Cancel();Hero.SetOutcome(false);}
        private void OnDestroy(){if(combat!=null)combat.BattleReset-=OnReset;}
    }
}
