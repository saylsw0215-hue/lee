using HeroDefense.Battle.Combat;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests
{
    public sealed class Phase2Tests
    {
        private GameObject healthObject;
        private HealthComponent health;

        [SetUp] public void SetUp() { healthObject = new GameObject("HealthTest", typeof(HealthComponent)); health = healthObject.GetComponent<HealthComponent>(); health.Initialize(100); }
        [TearDown] public void TearDown() { Object.DestroyImmediate(healthObject); }

        [Test] public void Health_InitializesAtMaximum() { Assert.AreEqual(100, health.CurrentHealth); Assert.AreEqual(100, health.MaxHealth); }
        [Test] public void Damage_ReducesHealth() { health.TakeDamage(new DamageInfo(25, Team.Enemy)); Assert.AreEqual(75, health.CurrentHealth); }
        [Test] public void Health_NeverDropsBelowZero() { health.TakeDamage(new DamageInfo(200, Team.Enemy)); Assert.AreEqual(0, health.CurrentHealth); }
        [Test] public void Healing_DoesNotExceedMaximum() { health.TakeDamage(new DamageInfo(10, Team.Enemy)); health.Heal(50); Assert.AreEqual(100, health.CurrentHealth); }
        [Test] public void Death_IsRaisedOnce() { int count=0; health.Died += _ => count++; health.TakeDamage(new DamageInfo(100,Team.Enemy)); health.TakeDamage(new DamageInfo(10,Team.Enemy)); Assert.AreEqual(1,count); }
        [Test] public void DamageAfterDeath_IsIgnored() { health.TakeDamage(new DamageInfo(100,Team.Enemy)); health.TakeDamage(new DamageInfo(10,Team.Enemy)); Assert.AreEqual(0,health.CurrentHealth); }
        [Test] public void SameTeam_CannotAttack() { var a=new FakeDamageable(Team.Player); var b=new FakeDamageable(Team.Player); Assert.IsFalse(CombatRegistry.CanAttack(a,b)); }
        [Test] public void MonsterRewards_AreConfigured() { Assert.AreEqual(10, RuntimeUnitCatalog.Get("EnemySlime").RewardGold); Assert.AreEqual(15, RuntimeUnitCatalog.Get("EnemyGoblin").RewardGold); }
        [Test] public void AttackCooldown_UsesInterval() { var timer=new AttackCooldown(); Assert.IsTrue(timer.TryConsume(1)); timer.Tick(.5f); Assert.IsFalse(timer.TryConsume(1)); timer.Tick(.5f); Assert.IsTrue(timer.TryConsume(1)); }
        [Test] public void UnitData_ValidationRejectsMissingId() { var data=ScriptableObject.CreateInstance<UnitData>(); data.Configure("","Bad",Team.Player,10,1,1,1,1,2,.3f,0,Color.white,UnitVisualShape.Swordsman); Assert.IsFalse(data.Validate(out _)); Object.DestroyImmediate(data); }

        private sealed class FakeDamageable : IDamageable
        {
            public FakeDamageable(Team team) { Team = team; }
            public Team Team { get; }
            public bool IsAlive => true;
            public Transform TargetTransform => null;
            public float CollisionRadius => 1;
            public void TakeDamage(DamageInfo damageInfo) { }
        }
    }
}
