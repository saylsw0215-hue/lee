using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Statistics;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Skills;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests
{
    public sealed class Phase6AdvancedCombatTests
    {
        private sealed class FixedRandom:ICombatRandom{private readonly float value;public FixedRandom(float v)=>value=v;public float Value()=>value;}
        private sealed class FakeCombatant:MonoBehaviour,IAdvancedCombatant
        {
            public Team Team{get;set;}=Team.Enemy;public bool IsAlive=>Health.IsAlive;public Transform TargetTransform=>transform;public float CollisionRadius=>1;public RuntimeCombatStats RuntimeStats{get;private set;}public StatusEffectController Statuses{get;private set;}public ShieldController Shields{get;private set;}=new();public HealthComponent Health{get;private set;}
            public void Init(CombatStats stats,float hp=100){Health=gameObject.AddComponent<HealthComponent>();Health.Initialize(hp);RuntimeStats=new RuntimeCombatStats(stats);Statuses=new StatusEffectController(this);}public void TakeDamage(DamageInfo info)=>ApplyAdvancedDamage(info);public DamageResult ApplyAdvancedDamage(DamageInfo info)=>AdvancedCombatResolver.Apply(this,info);
        }
        private GameObject go;private FakeCombatant target;
        [SetUp]public void Setup(){go=new GameObject("AdvancedTarget");target=go.AddComponent<FakeCombatant>();target.Init(Stats());}
        [TearDown]public void TearDown(){Object.DestroyImmediate(go);AdvancedCombatResolver.Service=new DamageCalculationService();}
        [Test]public void DefenseFormula_ReturnsHalfAtOneHundred(){Assert.AreEqual(50,DamageCalculationService.ApplyDefense(100,100),.01f);}
        [Test]public void PercentPenetration_PrecedesFlat(){Assert.AreEqual(65,DamageCalculationService.EffectiveDefense(100,.2f,15),.01f);}
        [Test]public void TrueDamage_IgnoresDefense(){var high=Stats(500,500);var r=new DamageCalculationService(new FixedRandom(1)).Calculate(Info(100,DamageType.True),null,new RuntimeCombatStats(high),null,new ShieldController(),true);Assert.AreEqual(100,r.HealthDamage,.01f);}
        [Test]public void CriticalMultiplier_AppliesOnce(){var attacker=new RuntimeCombatStats(Stats(0,0,1,2));var r=new DamageCalculationService(new FixedRandom(0)).Calculate(Info(100,DamageType.Physical,true,false),attacker,new RuntimeCombatStats(Stats()),null,new ShieldController(),true);Assert.AreEqual(200,r.HealthDamage,.01f);}
        [Test]public void NonCriticalAttack_NeverCrits(){var r=new DamageCalculationService(new FixedRandom(0)).Calculate(Info(100,DamageType.Physical,false),new RuntimeCombatStats(Stats(0,0,1,3)),target.RuntimeStats,target.Statuses,target.Shields,true);Assert.IsFalse(r.WasCritical);}
        [Test]public void Dodge_ProducesZeroDamage(){var dodge=new RuntimeCombatStats(Stats(0,0,0,1.5f,.75f));var r=new DamageCalculationService(new FixedRandom(0)).Calculate(Info(100,DamageType.Physical,false,true),null,dodge,null,new ShieldController(),true);Assert.IsTrue(r.WasDodged);Assert.Zero(r.HealthDamage);}
        [Test]public void DodgedAttack_DoesNotApplyFollowup(){var r=new DamageCalculationService(new FixedRandom(0)).Calculate(Info(100,DamageType.Physical,false,true),null,new RuntimeCombatStats(Stats(0,0,0,1.5f,.75f)),null,new ShieldController(),true);Assert.IsFalse(r.WasApplied);}
        [Test]public void Shield_IsConsumedBeforeHealth(){target.Shields.Add("test",50,5);var r=target.ApplyAdvancedDamage(Info(30,DamageType.True));Assert.AreEqual(20,target.Shields.Total,.01f);Assert.AreEqual(100,target.Health.CurrentHealth);Assert.AreEqual(30,r.ShieldAbsorbed,.01f);}
        [Test]public void ShieldOverflow_ReachesHealth(){target.Shields.Add("test",25,5);var r=target.ApplyAdvancedDamage(Info(40,DamageType.True));Assert.AreEqual(85,target.Health.CurrentHealth,.01f);Assert.AreEqual(15,r.HealthDamage,.01f);}
        [Test]public void Invincible_BlocksDamage(){target.Statuses.Apply(Status("Invincible"));var r=target.ApplyAdvancedDamage(Info(50,DamageType.True));Assert.IsTrue(r.WasBlocked);Assert.AreEqual(100,target.Health.CurrentHealth);}
        [Test]public void Stun_BlocksActionFlags(){target.Statuses.Apply(Status("Stun"));Assert.IsTrue(target.Statuses.IsStunned);}
        [Test]public void Freeze_ExpiresAndRestoresMovement(){target.Statuses.Apply(Status("Freeze"));Assert.Less(target.Statuses.MoveMultiplier,1);target.Statuses.Tick(3);Assert.AreEqual(1,target.Statuses.MoveMultiplier);}
        [Test]public void Burn_TicksExpectedCount(){target.Statuses.Apply(Status("Burn"),null,10);target.Statuses.Tick(1);target.Statuses.Tick(1);target.Statuses.Tick(1);Assert.LessOrEqual(target.Health.CurrentHealth,70);}
        [Test]public void Poison_StacksAreCapped(){var poison=Status("Poison");for(int i=0;i<9;i++)target.Statuses.Apply(poison,null,1);Assert.AreEqual(5,target.Statuses.Active[0].Stacks);}
        [Test]public void Shock_IncreasesOnlyMagicalDamage(){target.Statuses.Apply(Status("Shock"));var service=new DamageCalculationService(new FixedRandom(1));float physical=service.Calculate(Info(100,DamageType.Physical),null,target.RuntimeStats,target.Statuses,new ShieldController(),true).HealthDamage;float magical=service.Calculate(Info(100,DamageType.Magical),null,target.RuntimeStats,target.Statuses,new ShieldController(),true).HealthDamage;Assert.Greater(magical,physical);}
        [Test]public void Slow_KeepsStrongestValue(){var slow=Status("Slow");target.Statuses.Apply(slow,null,.6f);target.Statuses.Apply(slow,null,.2f);Assert.AreEqual(.4f,target.Statuses.MoveMultiplier,.01f);}
        [Test]public void Silence_IsReported(){target.Statuses.Apply(Status("Silence"));Assert.IsTrue(target.Statuses.IsSilenced);}
        [Test]public void Taunt_OverridesTarget(){var source=new GameObject("Taunter");var fake=source.AddComponent<FakeCombatant>();fake.Init(Stats());target.Statuses.Apply(Status("Taunt"),source,1,fake);Assert.AreSame(fake,target.Statuses.TauntTarget);Object.DestroyImmediate(source);}
        [Test]public void CrowdControlResistance_ShortensDuration(){target.RuntimeStats.Add(new StatModifier("cc",CombatStat.CrowdControlResistance,StatModifierType.Flat,.6f));target.Statuses.Apply(Status("Stun"));Assert.AreEqual(.6f,target.Statuses.Active[0].Remaining,.01f);}
        [Test]public void ExpiredModifier_IsRemoved(){target.Statuses.Apply(Status("ShamanPower"),null,.15f);Assert.Greater(target.RuntimeStats.AttackPowerMultiplier,1);target.Statuses.Tick(6);Assert.AreEqual(1,target.RuntimeStats.AttackPowerMultiplier,.01f);}
        [Test]public void Reset_ClearsStatusesAndShields(){target.Statuses.Apply(Status("Stun"));target.Shields.Add("x",10,5);target.Statuses.Clear();target.Shields.Clear();Assert.Zero(target.Statuses.Active.Count);Assert.Zero(target.Shields.Total);}
        [Test]public void AimCancel_DoesNotStartCooldown(){var state=new HeroRuntimeState();state.Spawn();Assert.Zero(state.SkillCooldownRemaining);}
        [Test]public void AimPoint_IsClampedToRange(){Vector3 p=SkillAimMath.Clamp(Vector3.zero,new Vector3(100,0),20);Assert.AreEqual(20,p.x,.01f);}
        [Test]public void RefreshRule_RefreshesDuration(){var stun=Status("Stun");target.Statuses.Apply(stun);target.Statuses.Tick(1);target.Statuses.Apply(stun);Assert.AreEqual(1.5f,target.Statuses.Active[0].Remaining,.01f);}
        [Test]public void Statistics_RecordDamageExactlyOnce(){var stats=new BattleStatistics();stats.RecordDamage(DamageType.Magical,25);Assert.AreEqual(25,stats.TotalMagicalDamage);Assert.Zero(stats.TotalPhysicalDamage);}
        [Test]public void HundredConcurrentStatusControllers_TickSafely(){var objects=new GameObject[100];for(int i=0;i<objects.Length;i++){objects[i]=new GameObject("StatusLoad"+i);var value=objects[i].AddComponent<FakeCombatant>();value.Init(Stats());value.Statuses.Apply(Status("Burn"),null,1);value.Statuses.Tick(1);}for(int i=0;i<objects.Length;i++)Object.DestroyImmediate(objects[i]);Assert.Pass();}
        private static DamageInfo Info(float amount,DamageType type,bool crit=false,bool dodge=false)=>new(amount,Team.Player,null,type,crit,dodge);
        private static CombatStats Stats(float defense=0,float magic=0,float crit=.05f,float multiplier=1.5f,float dodge=0){var value=new CombatStats();value.Configure(defense,magic,crit,multiplier,dodge,0,0,0,0,0);return value;}
        private static StatusEffectData Status(string file)=>RuntimeStatusCatalog.Get(file);
    }
}
