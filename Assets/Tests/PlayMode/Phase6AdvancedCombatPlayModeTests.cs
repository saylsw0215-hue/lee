using System.Collections;
using System.Reflection;
using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Core;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;
using HeroDefense.Heroes.Skills;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HeroDefense.Tests
{
    public sealed class Phase6AdvancedCombatPlayModeTests
    {
        [SetUp]public void Setup()=>Time.timeScale=1;
        [TearDown]public void Teardown()=>Time.timeScale=1;
        [UnityTest]public IEnumerator PhysicalDefense_ReducesRuntimeDamage(){Select("ArdenKnight");yield return Load();var hero=Hero();var enemy=Spawn("EnemyGoblin",hero.transform.localPosition+Vector3.right*80);hero.Runtime.Tick(3);float before=hero.Health.CurrentHealth;hero.TakeDamage(new DamageInfo(100,Team.Enemy,enemy.gameObject,DamageType.Physical,false,false));Assert.Less(before-hero.Health.CurrentHealth,100);}
        [UnityTest]public IEnumerator KnightShieldBash_AppliesStunAndTaunt(){Select("ArdenKnight");yield return Load();var hero=Hero();var enemy=Spawn("EnemySlime",hero.transform.localPosition+Vector3.right*80);Assert.IsTrue(hero.UseActiveSkillAt(enemy.transform.localPosition));Assert.IsTrue(enemy.Statuses.IsStunned);Assert.AreSame(hero,enemy.Statuses.TauntTarget);}
        [UnityTest]public IEnumerator KnightUltimate_CreatesShield(){Select("ArdenKnight");yield return Load();var hero=Hero();Spawn("EnemySlime",hero.transform.localPosition+Vector3.right*80);hero.Runtime.AddEnergy(100);Assert.IsTrue(hero.UseUltimate());Assert.Greater(hero.Shields.Total,0);Assert.Greater(GameObject.Find("HeroHUD").transform.Find("HPBar/ShieldFill").GetComponent<Image>().fillAmount,0);}
        [UnityTest]public IEnumerator RangerAiming_CancelDoesNotStartCooldown(){Select("RianRanger");yield return Load();var hero=Hero();Spawn("EnemySlime",hero.transform.localPosition+Vector3.right*100);var aim=Object.FindAnyObjectByType<SkillAimingController>();Assert.IsTrue(aim.Begin(hero.Data.ActiveSkill,false));aim.Cancel();Assert.Zero(hero.Runtime.SkillCooldownRemaining);Assert.IsFalse(aim.IsAiming);}
        [UnityTest]public IEnumerator RangerArrowRain_ManualPositionDamagesAndSlows(){Select("RianRanger");yield return Load();var hero=Hero();var enemy=Spawn("EnemySlime",hero.transform.localPosition+Vector3.right*100);var aim=Object.FindAnyObjectByType<SkillAimingController>();aim.Begin(hero.Data.ActiveSkill,false);Assert.IsTrue(aim.Confirm(enemy.transform.localPosition));yield return null;Assert.IsTrue(enemy.Statuses.Has(StatusId.Slow));Assert.Less(enemy.Health.CurrentHealth,enemy.Health.MaxHealth);}
        [UnityTest]public IEnumerator MageExplosion_ManualPositionAppliesBurn(){Select("SeraFireMage");yield return Load();var hero=Hero();var enemy=Spawn("EnemyEliteSlime",hero.transform.localPosition+Vector3.right*100);Assert.IsTrue(hero.UseActiveSkillAt(enemy.transform.localPosition));Assert.IsTrue(enemy.Statuses.Has(StatusId.Burn));}
        [UnityTest]public IEnumerator Silence_DisablesSkillButton(){Select("SeraFireMage");yield return Load();var hero=Hero();Spawn("EnemySlime",hero.transform.localPosition+Vector3.right*100);hero.ApplyStatus(RuntimeStatusCatalog.Get("Silence"));yield return null;Assert.IsFalse(GameObject.Find("HeroSkill").GetComponent<Button>().interactable);Assert.IsFalse(hero.UseActiveSkill());}
        [UnityTest]public IEnumerator Pause_CancelsAimingAndFreezesStatus(){Select("RianRanger");yield return Load();var hero=Hero();Spawn("EnemySlime",hero.transform.localPosition+Vector3.right*100);hero.ApplyStatus(RuntimeStatusCatalog.Get("Slow"));float remaining=hero.Statuses.Active[0].Remaining;var aim=Object.FindAnyObjectByType<SkillAimingController>();aim.Begin(hero.Data.ActiveSkill,false);GameObject.Find("Pause").GetComponent<Button>().onClick.Invoke();yield return null;hero.Simulate(2);Assert.IsFalse(aim.IsAiming);Assert.AreEqual(remaining,hero.Statuses.Active[0].Remaining,.01f);}
        [UnityTest]public IEnumerator SpecialistEnemies_SpawnAndShamanBuffsAlly(){Select("ArdenKnight");yield return Load();var hero=Hero();var shaman=Spawn("EnemyShamanGoblin",hero.transform.localPosition+Vector3.right*250);var goblin=Spawn("EnemyGoblin",shaman.transform.localPosition+Vector3.right*20);Assert.Greater(shaman.TriggerSupportBuffForDebug(),0);Assert.IsTrue(goblin.Statuses.Has(StatusId.ShamanPower));var poison=Spawn("EnemyPoisonGoblin",shaman.transform.localPosition+Vector3.right*40);Assert.AreEqual(UnitVisualShape.PoisonGoblin,poison.Data.VisualShape);}
        [UnityTest]public IEnumerator CombatReset_RemovesAdvancedState(){Select("ArdenKnight");yield return Load();var hero=Hero();hero.ApplyStatus(RuntimeStatusCatalog.Get("Slow"));hero.Shields.Add("test",50,5);Combat().ResetBattle();Assert.Zero(hero.Statuses.Active.Count);Assert.Zero(hero.Shields.Total);}
        private static HeroController Hero()=>Object.FindAnyObjectByType<HeroSpawnManager>().Hero;
        private static BattleCombatController Combat(){var scene=Object.FindAnyObjectByType<BattleSceneController>();return (BattleCombatController)typeof(BattleSceneController).GetField("combat",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(scene);}
        private static CombatUnit Spawn(string resource,Vector3 position){var combat=Combat();UnitData data=RuntimeUnitCatalog.Get(resource);combat.Spawn(data);var units=Object.FindObjectsByType<CombatUnit>(FindObjectsInactive.Exclude);for(int i=units.Length-1;i>=0;i--)if(units[i].Data==data){units[i].transform.localPosition=position;return units[i];}Assert.Fail("Spawned unit not found: "+resource);return null;}
        private static void Select(string resource){if(HeroSelectionService.Instance==null)new GameObject("HeroSelectionService",typeof(HeroSelectionService));string id=resource=="ArdenKnight"?"hero_arden_knight":resource=="RianRanger"?"hero_rian_ranger":"hero_sera_fire_mage";HeroSelectionService.Instance.Select(GameContentDatabase.Hero(id));}
        private static IEnumerator Load(){AsyncOperation op=SceneManager.LoadSceneAsync(SceneNames.Battle);while(!op.isDone)yield return null;yield return null;}
    }
}
