using System.Collections;
using System.Reflection;
using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Effects;
using HeroDefense.Battle.Projectiles;
using HeroDefense.Battle.Waves;
using HeroDefense.Core;
using HeroDefense.Heroes.Skills;
using HeroDefense.Save;
using HeroDefense.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HeroDefense.Tests
{
    /// <summary>Accelerated lifecycle checks for repeated Stage 1 resets and thirty endless waves.</summary>
    public sealed class LongSessionLifecyclePlayModeTests
    {
        private sealed class FailingSaveRepository:ISaveRepository{public bool HasSaveData=>true;public SaveLoadResult Load()=>new(true,DefaultSaveFactory.Create());public SaveWriteResult Save(GameSaveData data)=>new(false,"expected WebGL storage failure");public SaveWriteResult Delete()=>new(true);}
        [SetUp]public void Setup(){Time.timeScale=1;BattleLaunchConfig.Reset();}
        [TearDown]public void Teardown(){Time.timeScale=1;BattleLaunchConfig.Reset();}

        [UnityTest]
        public IEnumerator FiveBattleResetsReusePoolsAndClearEveryTransient()
        {
            yield return LoadBattle();BattleCombatController combat=Combat();var projectiles=Object.FindAnyObjectByType<ProjectilePool>();var labels=Object.FindAnyObjectByType<FloatingDamageTextPool>();var heroEffects=Object.FindAnyObjectByType<HeroEffectPool>();int stableCreated=-1;bool oldNumbers=SaveGameManager.Instance.Data.settings.damageNumbers;SaveGameManager.Instance.Data.settings.damageNumbers=true;
            for(int cycle=0;cycle<5;cycle++)
            {
                for(int i=0;i<15;i++)combat.Spawn(RuntimeUnitCatalog.Get("PlayerArcher"));for(int i=0;i<20;i++)combat.Spawn(RuntimeUnitCatalog.Get(i%2==0?"EnemySlime":"EnemyGoblin"));
                CombatUnit active=Object.FindAnyObjectByType<CombatUnit>();active.ApplyStatus(RuntimeStatusCatalog.Get("Burn"));active.Shields.Add("long_session",20,20);for(int i=0;i<12;i++)projectiles.Show(UnitVisualShape.Archer,combat.World,Vector3.zero,Vector3.right*100);for(int i=0;i<12;i++)labels.ShowText(combat.World,Vector3.zero,"TEST",Color.white);heroEffects.Show(Vector3.zero,40,Color.white,10);
                Assert.That(combat.ActivePlayerCount,Is.EqualTo(15));Assert.That(combat.ActiveEnemyCount,Is.EqualTo(20));Assert.That(projectiles.ActiveCount,Is.EqualTo(12));Assert.That(labels.ActiveCount,Is.GreaterThanOrEqualTo(12));Assert.That(heroEffects.ActiveCount,Is.EqualTo(1));
                combat.ResetBattle();Assert.That(combat.ActivePlayerCount,Is.Zero);Assert.That(combat.ActiveEnemyCount,Is.Zero);Assert.That(combat.ActiveProjectileCount,Is.Zero);Assert.That(combat.ActiveDamageTextCount,Is.Zero);Assert.That(heroEffects.ActiveCount,Is.Zero);
                foreach(var unit in Object.FindObjectsByType<CombatUnit>(FindObjectsInactive.Include,FindObjectsSortMode.None)){Assert.That(unit.Statuses.Active.Count,Is.Zero);Assert.That(unit.Shields.Total,Is.Zero);}
                if(stableCreated<0)stableCreated=combat.CreatedUnitCount;else Assert.That(combat.CreatedUnitCount,Is.EqualTo(stableCreated),$"Unit pool expanded after reset cycle {cycle+1}.");yield return null;
            }SaveGameManager.Instance.Data.settings.damageNumbers=oldNumbers;
        }

        [Test]
        public void EndlessThirtyWaveStateMachineAlwaysTerminatesAndSpawnsBosses()
        {
            int bossWaves=0,maxActive=0;for(int wave=1;wave<=30;wave++){bool boss=EndlessWaveGenerator.IsBoss(wave);if(boss)bossWaves++;int count=boss?1:Mathf.Min(6+wave/2,28);var data=ScriptableObject.CreateInstance<WaveData>();data.Configure("stress_"+wave,"Stress",0,0,0,EndlessWaveGenerator.IsElite(wave),boss,"",new[]{new WaveSpawnGroup(boss?EndlessWaveGenerator.BossForWave(wave):RuntimeUnitCatalog.Get("EnemySlime"),count,0,0,0)});var runtime=new WaveRuntimeState();runtime.Begin(wave-1,data);Assert.That(runtime.TickPreparation(1),Is.True);for(int i=0;i<count;i++)runtime.RegisterSpawn();maxActive=Mathf.Max(maxActive,runtime.AliveTracked);Assert.That(runtime.CanComplete,Is.False);for(int i=0;i<count;i++)runtime.RegisterDeath();Assert.That(runtime.CanComplete,Is.True);Assert.That(runtime.Complete(false),Is.True);Object.DestroyImmediate(data);}Assert.That(bossWaves,Is.EqualTo(3));Assert.That(maxActive,Is.LessThanOrEqualTo(28));
        }

        [UnityTest]
        public IEnumerator SaveFailureIsNonFatalAndDisplaysSystemMessage()
        {
            SaveGameManager save=SaveGameManager.Instance;if(save==null)save=new GameObject("SaveGameManager",typeof(SaveGameManager)).GetComponent<SaveGameManager>();ISaveRepository original=save.Repository;SystemMessageController message=Object.FindAnyObjectByType<SystemMessageController>();bool created=message==null;if(created)message=new GameObject("SystemMessages",typeof(SystemMessageController)).GetComponent<SystemMessageController>();yield return null;
            save.InitializeForTests(new FailingSaveRepository());LogAssert.Expect(LogType.Error,"Game data save failed: expected WebGL storage failure");Assert.That(save.SaveNow(SaveReason.Manual),Is.False);Assert.That(GameObject.Find("SystemMessage"),Is.Not.Null);Assert.That(GameObject.Find("SystemMessage").activeSelf,Is.True);
            if(original!=null)save.InitializeForTests(original);if(created)Object.Destroy(message.gameObject);yield return null;
        }

        private static BattleCombatController Combat(){var scene=Object.FindAnyObjectByType<BattleSceneController>();return (BattleCombatController)typeof(BattleSceneController).GetField("combat",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(scene);}
        private static IEnumerator LoadBattle(){AsyncOperation operation=SceneManager.LoadSceneAsync(SceneNames.Battle);while(!operation.isDone)yield return null;yield return null;}
    }
}
