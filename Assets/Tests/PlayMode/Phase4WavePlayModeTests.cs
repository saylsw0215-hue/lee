using System.Collections;
using HeroDefense.Battle.Waves;
using HeroDefense.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HeroDefense.Tests
{
    public sealed class Phase4WavePlayModeTests
    {
        [SetUp]public void Setup()=>Time.timeScale=1f;
        [TearDown]public void Teardown()=>Time.timeScale=1f;

        [UnityTest]public IEnumerator Battle_StartsWaveOnePreparationAndAutoSpawns()
        {
            yield return LoadBattle();WaveManager manager=Object.FindAnyObjectByType<WaveManager>();Assert.NotNull(manager);Assert.AreEqual(0,manager.Runtime.WaveIndex);Assert.AreEqual(WaveState.Preparing,manager.Runtime.State);
            manager.SkipPreparation();manager.Simulate(.05f);Assert.Greater(manager.Runtime.AliveTracked,0);Assert.Less(manager.Runtime.RemainingToSpawn,manager.Stage.Waves[0].TotalEnemyCount);
        }
        [UnityTest]public IEnumerator ForceClear_PaysOnceAndBeginsNextPreparation()
        {
            yield return LoadBattle();WaveManager manager=Object.FindAnyObjectByType<WaveManager>();manager.SkipPreparation();manager.Simulate(.05f);manager.ForceClearCurrentWave();Assert.AreEqual(WaveState.Completed,manager.Runtime.State);Assert.IsTrue(manager.Runtime.RewardClaimed);
            manager.Simulate(3f);Assert.AreEqual(1,manager.Runtime.WaveIndex);Assert.AreEqual(WaveState.Preparing,manager.Runtime.State);
        }
        [UnityTest]public IEnumerator Pause_FreezesPreparationCountdown()
        {
            yield return LoadBattle();WaveManager manager=Object.FindAnyObjectByType<WaveManager>();float remaining=manager.Runtime.PreparationRemaining;Time.timeScale=0;yield return null;yield return null;Assert.AreEqual(remaining,manager.Runtime.PreparationRemaining,.001f);
        }
        [UnityTest]public IEnumerator TenForcedWaves_ShowVictoryAndReplayRestarts()
        {
            yield return LoadBattle();WaveManager manager=Object.FindAnyObjectByType<WaveManager>();
            for(int wave=0;wave<10;wave++){manager.SkipPreparation();manager.Simulate(.05f);manager.ForceClearCurrentWave();if(wave<9)manager.Simulate(3f);}
            Assert.AreEqual(WaveState.StageCleared,manager.Runtime.State);GameObject result=GameObject.Find("StageResult");Assert.NotNull(result);Assert.IsTrue(result.activeInHierarchy);
            Button replay=GameObject.Find("Replay").GetComponent<Button>();replay.onClick.Invoke();Assert.AreEqual(WaveState.Preparing,manager.Runtime.State);Assert.AreEqual(0,manager.Runtime.WaveIndex);Assert.IsFalse(result.activeSelf);
        }
        [UnityTest]public IEnumerator FocusLoss_AutomaticallyPausesBattle()
        {
            yield return LoadBattle();var controller=Object.FindAnyObjectByType<HeroDefense.Battle.BattleSceneController>();controller.SendMessage("OnApplicationFocus",false);Assert.AreEqual(0f,Time.timeScale);
        }
        [UnityTest]public IEnumerator EliteWaveAndBossWave_SpawnConfiguredSpecialEnemies()
        {
            yield return LoadBattle();WaveManager manager=Object.FindAnyObjectByType<WaveManager>();
            for(int i=0;i<4;i++){manager.SkipPreparation();manager.Simulate(.05f);manager.ForceClearCurrentWave();manager.Simulate(3f);}
            manager.SkipPreparation();manager.Simulate(6.1f);Assert.IsTrue(ContainsActive("enemy_elite_goblin"));manager.ForceClearCurrentWave();manager.Simulate(3f);
            for(int i=5;i<9;i++){manager.SkipPreparation();manager.Simulate(.05f);manager.ForceClearCurrentWave();manager.Simulate(3f);}
            manager.SkipPreparation();manager.Simulate(8.1f);Assert.IsTrue(ContainsActive("boss_goblin_chieftain"));Assert.IsTrue(GameObject.Find("BossHealth").activeInHierarchy);
        }
        [UnityTest]public IEnumerator BaseDestruction_FailsWaveAndRestartReturnsToWaveOne()
        {
            yield return LoadBattle();var scene=Object.FindAnyObjectByType<HeroDefense.Battle.BattleSceneController>();var field=typeof(HeroDefense.Battle.BattleSceneController).GetField("combat",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var combat=(HeroDefense.Battle.BattleCombatController)field.GetValue(scene);combat.DamageBaseForDebug(999);WaveManager manager=Object.FindAnyObjectByType<WaveManager>();Assert.AreEqual(WaveState.Failed,manager.Runtime.State);Assert.IsTrue(GameObject.Find("DefeatOverlay").activeInHierarchy);
            GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke();Assert.AreEqual(WaveState.Preparing,manager.Runtime.State);Assert.AreEqual(0,manager.Runtime.WaveIndex);
        }
        private static bool ContainsActive(string id){var units=Object.FindObjectsByType<HeroDefense.Battle.Combat.CombatUnit>(FindObjectsInactive.Exclude);for(int i=0;i<units.Length;i++)if(units[i].Data.UnitId==id)return true;return false;}
        private static IEnumerator LoadBattle(){Time.timeScale=1f;AsyncOperation operation=SceneManager.LoadSceneAsync(SceneNames.Battle);while(!operation.isDone)yield return null;yield return null;}
    }
}
