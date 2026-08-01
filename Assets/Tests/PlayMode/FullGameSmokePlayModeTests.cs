using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Projectiles;
using HeroDefense.Battle.Statistics;
using HeroDefense.Battle.Waves;
using HeroDefense.Core;
using HeroDefense.Meta;
using HeroDefense.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HeroDefense.Tests
{
    public sealed class FullGameSmokePlayModeTests
    {
        private string temporarySaveDirectory;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale=1f;
            if(!string.IsNullOrEmpty(temporarySaveDirectory)&&SaveGameManager.Instance!=null)UnityEngine.Object.Destroy(SaveGameManager.Instance.gameObject);
            yield return null;
            if(!string.IsNullOrEmpty(temporarySaveDirectory)&&Directory.Exists(temporarySaveDirectory))Directory.Delete(temporarySaveDirectory,true);
            temporarySaveDirectory=null;
            BattleLaunchConfig.Reset();
        }

        [UnityTest]
        public IEnumerator BootToBattleCanonicalFlowLoadsEveryScene()
        {
            yield return Load(SceneNames.Boot);
            yield return WaitForScene(SceneNames.MainMenu);
            GameObject.Find("Start").GetComponent<Button>().onClick.Invoke();
            yield return WaitForScene(SceneNames.HeroSelect);
            GameObject.Find("hero_arden_knight").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("StartBattle").GetComponent<Button>().onClick.Invoke();
            yield return WaitForScene(SceneNames.StageSelect);
            GameObject.Find("stage_01_grassland").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("Start").GetComponent<Button>().onClick.Invoke();
            yield return WaitForScene(SceneNames.Battle);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<BattleSceneController>(),Is.Not.Null);
            Assert.That(UnityEngine.Object.FindAnyObjectByType<WaveManager>().Runtime.State,Is.EqualTo(WaveState.Preparing));
        }

        [UnityTest]
        public IEnumerator StageOneVictoryRewardAndStageTwoUnlockAreIdempotent()
        {
            yield return ReplaceSaveManager();
            BattleLaunchConfig.Configure("stage_01_grassland",GameDifficulty.Normal,GameMode.Stage);
            var statistics=new BattleStatistics();
            statistics.SelectHero("hero_arden_knight");
            for(int i=0;i<10;i++)statistics.ReachWave(i+1);
            var progress=new BattleResultProgressService(SaveGameManager.Instance);
            int beforeCoin=SaveGameManager.Instance.Data.currencies.coin;

            PermanentReward first=progress.Record("smoke-result-001",true,statistics,100);
            int afterFirst=SaveGameManager.Instance.Data.currencies.coin;
            PermanentReward duplicate=progress.Record("smoke-result-001",true,statistics,100);

            Assert.That(first.Coin,Is.GreaterThan(0));
            Assert.That(afterFirst,Is.EqualTo(beforeCoin+first.Coin));
            Assert.That(duplicate.Coin,Is.Zero);
            Assert.That(SaveGameManager.Instance.Data.currencies.coin,Is.EqualTo(afterFirst));
            Assert.That(SaveRecords.Stage(SaveGameManager.Instance.Data,"stage_02_red_canyon").unlocked,Is.True);
            Assert.That(SaveGameManager.Instance.Data.profile.wins,Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DefeatRestartClearsCombatEffectsAndRestoresWaveAndTime()
        {
            yield return ReplaceSaveManager();
            BattleLaunchConfig.Reset();
            yield return Load(SceneNames.Battle);
            BattleSceneController scene=UnityEngine.Object.FindAnyObjectByType<BattleSceneController>();
            BattleCombatController combat=Private<BattleCombatController>(scene,"combat");
            WaveManager waves=UnityEngine.Object.FindAnyObjectByType<WaveManager>();
            combat.Spawn(RuntimeUnitCatalog.Get("PlayerArcher"));
            combat.Spawn(RuntimeUnitCatalog.Get("EnemySlime"));
            yield return null;
            CombatUnit unit=UnityEngine.Object.FindAnyObjectByType<CombatUnit>();
            Assert.That(unit,Is.Not.Null);
            unit.Shields.Add("smoke",25,10);
            unit.ApplyStatus(RuntimeStatusCatalog.Get("Burn"));
            ProjectilePool projectiles=UnityEngine.Object.FindAnyObjectByType<ProjectilePool>();
            projectiles.Show(UnitVisualShape.Archer,combat.World,Vector3.zero,Vector3.right*100);
            Assert.That(projectiles.ActiveCount,Is.EqualTo(1));
            combat.DamageBaseForDebug(99999);
            yield return null;
            Assert.That(GameObject.Find("DefeatOverlay").activeSelf,Is.True);

            GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke();
            yield return null;

            Assert.That(combat.ActivePlayerCount,Is.Zero);
            Assert.That(combat.ActiveEnemyCount,Is.Zero);
            Assert.That(projectiles.ActiveCount,Is.Zero);
            Assert.That(unit.Statuses.Active.Count,Is.Zero);
            Assert.That(unit.Shields.Total,Is.Zero);
            Assert.That(waves.Runtime.WaveIndex,Is.EqualTo(0));
            Assert.That(waves.Runtime.State,Is.EqualTo(WaveState.Preparing));
            Assert.That(GameObject.Find("DefeatOverlay"),Is.Null,"Inactive defeat panel must not remain visible.");
            Assert.That(GameObject.Find("StageResult"),Is.Null,"Inactive result panel must not remain visible.");
            Assert.That(Time.timeScale,Is.EqualTo(1f));
        }

        private IEnumerator ReplaceSaveManager()
        {
            temporarySaveDirectory=Path.Combine(Application.temporaryCachePath,"FullGameSmokePlayModeTests",Guid.NewGuid().ToString("N"));
            if(SaveGameManager.Instance!=null)UnityEngine.Object.Destroy(SaveGameManager.Instance.gameObject);
            yield return null;
            var owner=new GameObject("SaveGameManager",typeof(SaveGameManager));
            owner.GetComponent<SaveGameManager>().InitializeForTests(new JsonFileSaveRepository(temporarySaveDirectory));
            yield return null;
        }

        private static T Private<T>(object owner,string field) where T:class
        {
            return typeof(BattleSceneController).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(owner) as T;
        }

        private static IEnumerator Load(string scene)
        {
            AsyncOperation operation=SceneManager.LoadSceneAsync(scene);
            while(!operation.isDone)yield return null;
            yield return null;
        }

        private static IEnumerator WaitForScene(string scene)
        {
            float timeout=8f;
            while(SceneManager.GetActiveScene().name!=scene&&timeout>0){timeout-=Time.unscaledDeltaTime;yield return null;}
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(scene));
            yield return null;
        }
    }
}
