using System.Collections;
using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HeroDefense.Tests
{
    public sealed class Phase2CombatPlayModeTests
    {
        private GameObject root;
        private CombatRegistry registry;
        private CombatPool pool;
        private UnitData sword, slime;

        [SetUp]
        public void Setup()
        {
            root = new GameObject("CombatTestRoot",typeof(RectTransform)); registry = new CombatRegistry();
            var effects = new GameObject("Effects",typeof(FloatingDamageTextPool)); effects.transform.SetParent(root.transform);
            pool = new CombatPool(root.transform,registry,effects.GetComponent<FloatingDamageTextPool>());
            sword=RuntimeUnitCatalog.Get("PlayerSwordsman"); slime=RuntimeUnitCatalog.Get("EnemySlime");
        }
        [TearDown] public void Teardown() { Time.timeScale=1; Object.DestroyImmediate(root); }

        [UnityTest]
        public IEnumerator Units_ApproachStopAndDealDamage()
        {
            CombatUnit player=pool.Spawn(sword,new Vector2(-280,0),600); CombatUnit enemy=pool.Spawn(slime,new Vector2(280,0),-600);
            float initialDistance=Vector3.Distance(player.transform.localPosition,enemy.transform.localPosition),initialHealth=enemy.Health.CurrentHealth;
            for(int i=0;i<30;i++){ player.Simulate(.1f); enemy.Simulate(.1f); }
            Assert.Less(Vector3.Distance(player.transform.localPosition,enemy.transform.localPosition),initialDistance);
            Assert.Less(enemy.GetComponent<HealthComponent>().CurrentHealth,initialHealth);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DeadTarget_IsReplaced()
        {
            CombatUnit player=pool.Spawn(sword,new Vector2(0,0),600); CombatUnit first=pool.Spawn(slime,new Vector2(80,0),-600); CombatUnit second=pool.Spawn(slime,new Vector2(180,0),-600);
            first.TakeDamage(new DamageInfo(999,Team.Player,player.gameObject)); player.Simulate(.3f);
            Assert.AreSame(second,player.CurrentTarget); yield return null;
        }

        [UnityTest]
        public IEnumerator Pause_FreezesAutomaticCombat()
        {
            CombatUnit player=pool.Spawn(sword,new Vector2(0,0),600); CombatUnit enemy=pool.Spawn(slime,new Vector2(80,0),-600);
            Time.timeScale=0; float hp=enemy.GetComponent<HealthComponent>().CurrentHealth; yield return null; yield return null;
            Assert.AreEqual(hp,enemy.GetComponent<HealthComponent>().CurrentHealth); Assert.IsTrue(player.IsAlive);
        }

        [UnityTest]
        public IEnumerator ThirtyFivePooledUnits_RemainActiveAndStable()
        {
            for(int i=0;i<15;i++) pool.Spawn(sword,new Vector2(-500,(i%7)*20),600);
            for(int i=0;i<20;i++) pool.Spawn(slime,new Vector2(500,(i%7)*20),-600);
            for(int step=0;step<10;step++) for(int i=0;i<pool.Active.Count;i++) pool.Active[i].Simulate(.05f);
            Assert.AreEqual(35,pool.Active.Count); Assert.AreEqual(15,registry.PlayerCount); Assert.AreEqual(20,registry.EnemyCount); yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyWithoutPlayerTargetsBaseAndDamagesIt()
        {
            var session=new BattleSessionState(); var baseObject=new GameObject("Base",typeof(RectTransform),typeof(PlayerBase)); baseObject.transform.SetParent(root.transform,false);
            var playerBase=baseObject.GetComponent<PlayerBase>(); playerBase.Build(session); registry.SetPlayerBase(playerBase);
            CombatUnit enemy=pool.Spawn(slime,new Vector2(80,0),-600);
            for(int i=0;i<20;i++) enemy.Simulate(.1f);
            Assert.AreSame(playerBase,enemy.CurrentTarget); Assert.Less(session.CurrentBaseHp,session.MaxBaseHp); yield return null;
        }

        [UnityTest]
        public IEnumerator FiftyCombatants_RegisterAndSimulateWithoutErrors()
        {
            for(int i=0;i<30;i++)pool.Spawn(sword,new Vector2(-550-i,(i%10)*18),600);
            for(int i=0;i<20;i++)pool.Spawn(slime,new Vector2(550+i,(i%10)*18),-600);
            for(int step=0;step<5;step++)for(int i=0;i<pool.Active.Count;i++)pool.Active[i].Simulate(.05f);
            Assert.AreEqual(50,pool.Active.Count);Assert.AreEqual(30,registry.PlayerCount);Assert.AreEqual(20,registry.EnemyCount);yield return null;
        }
    }
}
