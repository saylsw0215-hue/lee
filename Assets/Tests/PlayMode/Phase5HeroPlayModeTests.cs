using System.Collections;
using System.Reflection;
using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Core;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HeroDefense.Tests
{
    public sealed class Phase5HeroPlayModeTests
    {
        [SetUp] public void Setup() => Time.timeScale = 1f;
        [TearDown] public void Teardown() => Time.timeScale = 1f;

        [UnityTest] public IEnumerator HeroSelect_ShowsThreeCardsAndCarriesSelectionThroughStageSelect()
        {
            yield return Load(SceneNames.HeroSelect);
            Assert.NotNull(GameObject.Find("hero_arden_knight"));
            Assert.NotNull(GameObject.Find("hero_rian_ranger"));
            Assert.NotNull(GameObject.Find("hero_sera_fire_mage"));
            GameObject.Find("hero_rian_ranger").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("StartBattle").GetComponent<Button>().onClick.Invoke();
            yield return WaitForScene(SceneNames.StageSelect);
            GameObject.Find("stage_01_grassland").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Start").GetComponent<Button>().onClick.Invoke();
            yield return WaitForScene(SceneNames.Battle);
            Assert.AreEqual("hero_rian_ranger", Object.FindAnyObjectByType<HeroSpawnManager>().Hero.Data.HeroId);
        }

        [UnityTest] public IEnumerator Battle_SpawnsDefaultHeroAndHeroDamagesEnemy()
        {
            Select("ArdenKnight"); yield return Load(SceneNames.Battle);
            HeroController hero = Object.FindAnyObjectByType<HeroSpawnManager>().Hero;
            CombatUnit enemy = SpawnEnemyNear(hero, "EnemySlime");
            float before = enemy.Health.CurrentHealth;
            hero.Runtime.Tick(3f); hero.Simulate(1.1f);
            Assert.Less(enemy.Health.CurrentHealth, before);
        }

        [UnityTest] public IEnumerator ActiveSkill_DamagesEnemyAndStartsCooldown()
        {
            Select("SeraMage"); yield return Load(SceneNames.Battle);
            HeroController hero = Object.FindAnyObjectByType<HeroSpawnManager>().Hero;
            CombatUnit enemy = SpawnEnemyNear(hero, "EnemySlime");
            float before = enemy.Health.CurrentHealth;
            Assert.IsTrue(hero.UseActiveSkill());
            yield return new WaitForSeconds(.8f);
            Assert.Less(enemy.Health.CurrentHealth, before);
            Assert.Greater(hero.Runtime.SkillCooldownRemaining, 0f);
            Assert.IsFalse(hero.UseActiveSkill());
        }

        [UnityTest] public IEnumerator Ultimate_ConsumesFullEnergyAndDamagesEnemy()
        {
            Select("ArdenKnight"); yield return Load(SceneNames.Battle);
            HeroController hero = Object.FindAnyObjectByType<HeroSpawnManager>().Hero;
            CombatUnit enemy = SpawnEnemyNear(hero, "EnemyGoblin");
            hero.Runtime.AddEnergy(100f); float before = enemy.Health.CurrentHealth;
            Assert.IsTrue(hero.UseUltimate());
            Assert.AreEqual(0f, hero.Runtime.UltimateEnergy);
            Assert.Less(enemy.Health.CurrentHealth, before);
        }

        [UnityTest] public IEnumerator HeroDeath_RespawnsAtFullHealthWithInvincibility()
        {
            Select("RianRanger"); yield return Load(SceneNames.Battle);
            HeroController hero = Object.FindAnyObjectByType<HeroSpawnManager>().Hero;
            hero.Runtime.Tick(3f);
            hero.TakeDamage(new DamageInfo(9999f, Team.Enemy));
            Assert.AreEqual(HeroState.Respawning, hero.Runtime.State);
            hero.Simulate(hero.Data.RespawnDuration + .1f);
            Assert.AreEqual(HeroState.Alive, hero.Runtime.State);
            Assert.AreEqual(hero.Data.MaxHealth, hero.Health.CurrentHealth);
            float full = hero.Health.CurrentHealth;
            hero.TakeDamage(new DamageInfo(50f, Team.Enemy));
            Assert.AreEqual(full, hero.Health.CurrentHealth);
        }

        [UnityTest] public IEnumerator Pause_FreezesHeroCooldownAndRespawnTimer()
        {
            Select("ArdenKnight"); yield return Load(SceneNames.Battle);
            HeroController hero = Object.FindAnyObjectByType<HeroSpawnManager>().Hero;
            SpawnEnemyNear(hero, "EnemySlime"); Assert.IsTrue(hero.UseActiveSkill());
            float cooldown = hero.Runtime.SkillCooldownRemaining;
            GameObject.Find("Pause").GetComponent<Button>().onClick.Invoke();
            hero.Simulate(2f);
            Assert.AreEqual(cooldown, hero.Runtime.SkillCooldownRemaining);
        }

        private static void Select(string resource)
        {
            if (HeroSelectionService.Instance == null) new GameObject("HeroSelectionService", typeof(HeroSelectionService));
            string id=resource=="ArdenKnight"?"hero_arden_knight":resource=="RianRanger"?"hero_rian_ranger":"hero_sera_fire_mage";
            HeroSelectionService.Instance.Select(GameContentDatabase.Hero(id));
        }

        private static CombatUnit SpawnEnemyNear(HeroController hero, string resource)
        {
            BattleSceneController scene = Object.FindAnyObjectByType<BattleSceneController>();
            FieldInfo field = typeof(BattleSceneController).GetField("combat", BindingFlags.Instance | BindingFlags.NonPublic);
            BattleCombatController combat = (BattleCombatController)field.GetValue(scene);
            combat.Spawn(RuntimeUnitCatalog.Get(resource));
            CombatUnit[] units = Object.FindObjectsByType<CombatUnit>(FindObjectsInactive.Exclude);
            CombatUnit enemy = null;
            for (int i = 0; i < units.Length; i++) if (units[i].Team == Team.Enemy) enemy = units[i];
            Assert.NotNull(enemy);
            enemy.transform.localPosition = hero.transform.localPosition + new Vector3(80f, 0f);
            return enemy;
        }

        private static IEnumerator Load(string scene)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
            while (!operation.isDone) yield return null;
            yield return null;
        }

        private static IEnumerator WaitForScene(string scene)
        {
            float timeout = 5f;
            while (SceneManager.GetActiveScene().name != scene && timeout > 0f) { timeout -= Time.unscaledDeltaTime; yield return null; }
            Assert.AreEqual(scene, SceneManager.GetActiveScene().name);
            yield return null;
        }
    }
}
