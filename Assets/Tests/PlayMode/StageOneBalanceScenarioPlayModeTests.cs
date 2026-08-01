using System.Collections;
using System.Reflection;
using HeroDefense.Battle;
using HeroDefense.Battle.Waves;
using HeroDefense.Core;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;
using HeroDefense.QA;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HeroDefense.Tests.PlayMode
{
    /// <summary>Opt-in, accelerated full Battle-scene balance measurements; excluded from normal regression runs.</summary>
    [Category("Balance")]
    public sealed class StageOneBalanceScenarioPlayModeTests
    {
        [UnityTest,Explicit("Run with the balance workflow or select this test explicitly.")] public IEnumerator ArdenStageOne()=>Run("hero_arden_knight");
        [UnityTest,Explicit("Run with the balance workflow or select this test explicitly.")] public IEnumerator RianStageOne()=>Run("hero_rian_ranger");
        [UnityTest,Explicit("Run with the balance workflow or select this test explicitly.")] public IEnumerator SeraStageOne()=>Run("hero_sera_fire_mage");

        private static IEnumerator Run(string heroId)
        {
            Time.timeScale=1;BattleLaunchConfig.Configure("stage_01_grassland",GameDifficulty.Normal,GameMode.Stage);if(HeroSelectionService.Instance==null)new GameObject("HeroSelectionService",typeof(HeroSelectionService));HeroSelectionService.Instance.Select(GameContentDatabase.Hero(heroId));
            yield return SceneManager.LoadSceneAsync(SceneNames.Battle);yield return null;yield return null;
            BattleSceneController scene=Object.FindAnyObjectByType<BattleSceneController>();Assert.That(scene,Is.Not.Null);BattleCombatController combat=Field<BattleCombatController>(scene,"combat");WaveManager waves=Field<WaveManager>(scene,"waveManager");BattleSessionState session=Field<BattleSessionState>(scene,"state");HeroController hero=Field<HeroSpawnManager>(scene,"heroManager").Hero;
            using var telemetry=new StageBalanceTelemetrySession(heroId,waves,combat,session);Place("building_barracks","BuildSlot_1",telemetry,100);Place("building_archery_range","BuildSlot_2",telemetry,140);Place("building_magic_tower","BuildSlot_3",telemetry,180);
            bool upgraded=false,ultimateUsed=false;float deadline=Time.realtimeSinceStartup+180f;Time.timeScale=12f;
            while(!combat.IsStageEnded&&Time.realtimeSinceStartup<deadline)
            {
                telemetry.SampleActiveObjects();
                if(!upgraded&&session.CurrentGold>=100&&waves.Runtime.WaveIndex>=0){int before=session.CurrentGold;Click("BuildSlot_1");Click("Upgrade");if(session.CurrentGold<before){telemetry.RecordGoldSpent(before-session.CurrentGold);upgraded=true;}}
                if(combat.ActiveEnemyCount>0){hero.UseActiveSkill();if(!ultimateUsed){hero.Runtime.AddEnergy(100);ultimateUsed=hero.UseUltimate();}}
                yield return null;
            }
            Time.timeScale=1f;if(!combat.IsStageEnded)telemetry.RecordTimeout("Stage did not reach victory or defeat within 180 real seconds at 12x simulation speed.");string path=StageBalanceTelemetryWriter.PathFor(heroId);telemetry.Write(path);Debug.Log("Balance telemetry: "+System.IO.Path.GetFullPath(path));
            Assert.That(combat.IsStageEnded,Is.True,"Stage did not terminate.");Assert.That(telemetry.Report.HasInvalidNumbers(),Is.False);Assert.That(telemetry.Report.bossSpawned,Is.True,"Boss did not spawn.");Assert.That(waves.Statistics.InstalledBuildings,Is.GreaterThanOrEqualTo(3));Assert.That(waves.Statistics.HeroSkillUseCount,Is.GreaterThan(0));Assert.That(waves.Statistics.HeroUltimateUseCount,Is.GreaterThan(0));Assert.That(upgraded,Is.True,"Baseline strategy never afforded an upgrade.");
        }

        private static void Place(string button,string slot,StageBalanceTelemetrySession telemetry,int cost){int before=SessionGold();Click(button);Click(slot);int spent=before-SessionGold();Assert.That(spent,Is.EqualTo(cost),button);telemetry.RecordGoldSpent(spent);}
        private static int SessionGold(){BattleSceneController scene=Object.FindAnyObjectByType<BattleSceneController>();return Field<BattleSessionState>(scene,"state").CurrentGold;}
        private static void Click(string name){GameObject value=GameObject.Find(name);Assert.That(value,Is.Not.Null,name);Button button=value.GetComponent<Button>();Assert.That(button,Is.Not.Null,name);Assert.That(button.interactable,Is.True,name);button.onClick.Invoke();}
        private static T Field<T>(object owner,string name) where T:class{FieldInfo field=owner.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);Assert.That(field,Is.Not.Null,name);return field.GetValue(owner) as T;}
    }
}
