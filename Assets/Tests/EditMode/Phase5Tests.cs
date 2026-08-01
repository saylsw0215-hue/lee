using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;
using HeroDefense.Heroes.Skills;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests
{
    public sealed class Phase5Tests
    {
        private HeroData knight;
        [SetUp]public void Setup(){knight=RuntimeHeroCatalog.GetDefault();}
        [Test]public void HeroData_RequiredValuesValidate(){Assert.NotNull(knight.ActiveSkill);Assert.NotNull(knight.UltimateSkill);Assert.NotNull(knight.Passive);Assert.AreEqual("hero_arden_knight",knight.HeroId);}
        [Test]public void SelectionService_StoresSelectedHero(){var go=new GameObject("Selection",typeof(HeroSelectionService));var ranger=RuntimeHeroCatalog.GetHeroes()[1];go.GetComponent<HeroSelectionService>().Select(ranger);Assert.AreSame(ranger,go.GetComponent<HeroSelectionService>().SelectedHero);Object.DestroyImmediate(go);}
        [Test]public void NullSelection_ReturnsDefaultKnight(){var go=new GameObject("Selection",typeof(HeroSelectionService));var service=go.GetComponent<HeroSelectionService>();service.Select(null);Assert.AreEqual("hero_arden_knight",service.GetSelectedOrDefault().HeroId);Object.DestroyImmediate(go);}
        [Test]public void SkillCooldown_StartsOnUse(){var state=Alive();Assert.IsTrue(state.BeginSkill(8));Assert.AreEqual(8,state.SkillCooldownRemaining);}
        [Test]public void Cooldown_BlocksReuse(){var state=Alive();state.BeginSkill(8);state.FinishCast();Assert.IsFalse(state.BeginSkill(8));}
        [Test]public void UltimateEnergy_IsClampedToHundred(){var state=Alive();state.AddEnergy(150);Assert.AreEqual(100,state.UltimateEnergy);}
        [Test]public void UltimateUse_ConsumesAllEnergy(){var state=Alive();state.AddEnergy(100);Assert.IsTrue(state.BeginUltimate());Assert.AreEqual(0,state.UltimateEnergy);}
        [Test]public void DeadHero_CannotUseSkill(){var state=Alive();state.Die(10);Assert.IsFalse(state.BeginSkill(8));}
        [Test]public void RespawnTimer_NeverNegative(){var state=Alive();state.Die(2);Assert.IsTrue(state.Tick(5));Assert.AreEqual(0,state.RespawnRemaining);}
        [Test]public void RespawnHealth_CanRestoreMaximum(){var go=new GameObject("Health",typeof(HealthComponent));var health=go.GetComponent<HealthComponent>();health.Initialize(knight.MaxHealth);health.TakeDamage(new DamageInfo(999,Team.Enemy));health.Initialize(knight.MaxHealth);Assert.AreEqual(knight.MaxHealth,health.CurrentHealth);Object.DestroyImmediate(go);}
        [Test]public void PassiveData_UsesExpectedEventKind(){Assert.AreEqual(HeroPassiveKind.SteelWill,knight.Passive.Kind);Assert.AreEqual(HeroPassiveKind.ConsecutiveShot,RuntimeHeroCatalog.GetHeroes()[1].Passive.Kind);}
        [Test]public void AreaRule_BlocksFriendlyFire(){var source=new Fake(Team.Player);var friendly=new Fake(Team.Player);Assert.IsFalse(HeroSkillRules.TryAccept(source,friendly,new System.Collections.Generic.HashSet<IDamageable>()));}
        [Test]public void AreaRule_BlocksDuplicateTarget(){var source=new Fake(Team.Player);var enemy=new Fake(Team.Enemy);var hit=new System.Collections.Generic.HashSet<IDamageable>();Assert.IsTrue(HeroSkillRules.TryAccept(source,enemy,hit));Assert.IsFalse(HeroSkillRules.TryAccept(source,enemy,hit));}
        [Test]public void Outcome_BlocksHeroInput(){var state=Alive();state.SetOutcome(true);Assert.IsFalse(state.CanUseSkill);state.Reset();state.Spawn();state.SetOutcome(false);Assert.IsFalse(state.CanUseSkill);}
        [Test]public void Reset_ClearsHeroRuntime(){var state=Alive();state.AddEnergy(100);state.BeginSkill(8);state.Reset();Assert.AreEqual(HeroState.Inactive,state.State);Assert.AreEqual(0,state.UltimateEnergy);Assert.AreEqual(0,state.SkillCooldownRemaining);}
        private static HeroRuntimeState Alive(){var state=new HeroRuntimeState();state.Spawn();return state;}
        private sealed class Fake:IDamageable{public Fake(Team team){Team=team;}public Team Team{get;}public bool IsAlive=>true;public Transform TargetTransform=>null;public float CollisionRadius=>1;public void TakeDamage(DamageInfo info){}}
    }
}
