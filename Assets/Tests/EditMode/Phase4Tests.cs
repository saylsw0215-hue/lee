using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Stages;
using HeroDefense.Battle.Statistics;
using HeroDefense.Battle.Waves;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests
{
    public sealed class Phase4Tests
    {
        private StageData stage;private WaveData first;
        [SetUp]public void Setup(){stage=Resources.Load<StageData>("StageData/Stage01Grassland");first=stage.Waves[0];}
        [Test]public void Stage_PreservesTenWaveOrder(){Assert.AreEqual(10,stage.WaveCount);for(int i=0;i<10;i++)Assert.AreEqual($"stage01_wave_{i+1:00}",stage.Waves[i].WaveId);}
        [Test]public void Wave_TotalEnemyCountIsAccurate(){Assert.AreEqual(5,first.TotalEnemyCount);Assert.AreEqual(12,stage.Waves[4].TotalEnemyCount);}
        [Test]public void SpawnGroup_RejectsInvalidCountAndInterval(){var bad=new WaveSpawnGroup(Resources.Load<UnitData>("UnitData/EnemySlime"),0,0,-1,0);Assert.IsFalse(bad.Validate(2,out _));}
        [Test]public void WaveState_TransitionsInOrder(){var runtime=new WaveRuntimeState();runtime.Begin(0,first);Assert.AreEqual(WaveState.Preparing,runtime.State);runtime.SkipPreparation();Assert.AreEqual(WaveState.Spawning,runtime.State);}
        [Test]public void Preparation_TransitionsToSpawning(){var runtime=new WaveRuntimeState();runtime.Begin(0,first);Assert.IsTrue(runtime.TickPreparation(20));Assert.AreEqual(WaveState.Spawning,runtime.State);}
        [Test]public void Wave_DoesNotCompleteBeforeAllSpawns(){var runtime=Start();runtime.RegisterSpawn();Assert.IsFalse(runtime.CanComplete);}
        [Test]public void Wave_DoesNotCompleteWhileEnemyAlive(){var runtime=Start();for(int i=0;i<5;i++)runtime.RegisterSpawn();Assert.IsFalse(runtime.CanComplete);}
        [Test]public void Wave_CompletesAfterAllEnemiesDie(){var runtime=Start();for(int i=0;i<5;i++)runtime.RegisterSpawn();for(int i=0;i<5;i++)runtime.RegisterDeath();Assert.IsTrue(runtime.CanComplete);}
        [Test]public void Reward_CanOnlyBeClaimedOnce(){var runtime=Resolved();runtime.Complete(false);Assert.IsTrue(runtime.ClaimReward());Assert.IsFalse(runtime.ClaimReward());}
        [Test]public void LastWave_BecomesStageCleared(){var runtime=Resolved();runtime.Complete(true);Assert.AreEqual(WaveState.StageCleared,runtime.State);}
        [Test]public void FailedWave_CannotBecomeCleared(){var runtime=Start();runtime.Fail();Assert.IsFalse(runtime.Complete(true));Assert.AreEqual(WaveState.Failed,runtime.State);}
        [Test]public void EliteHealth_IsHigherThanNormal(){Assert.Greater(Resources.Load<UnitData>("UnitData/EnemyEliteSlime").MaxHealth,Resources.Load<UnitData>("UnitData/EnemySlime").MaxHealth);Assert.Greater(Resources.Load<UnitData>("UnitData/EnemyEliteGoblin").MaxHealth,Resources.Load<UnitData>("UnitData/EnemyGoblin").MaxHealth);}
        [Test]public void BossDataAndFinalWave_AreValid(){Assert.IsTrue(stage.Waves[9].IsBossWave);Assert.IsTrue(stage.Validate(out _));Assert.AreEqual(1800,Resources.Load<UnitData>("UnitData/BossGoblinChieftain").MaxHealth);}
        [Test]public void EnemyCapacity_StopsAtThirtyFive(){Assert.IsTrue(EnemyCapacity.CanSpawn(34));Assert.IsFalse(EnemyCapacity.CanSpawn(35));}
        [Test]public void Statistics_ResetAllValues(){var stats=new BattleStatistics();stats.RecordEnemy("boss_test",100);stats.RecordProduced();stats.RecordWaveReward(50);stats.Tick(2);stats.Reset();Assert.AreEqual(0,stats.TotalKills);Assert.AreEqual(0,stats.ProducedAllies);Assert.AreEqual(0,stats.WaveGold);Assert.AreEqual(0,stats.PlayTime);}
        private WaveRuntimeState Start(){var r=new WaveRuntimeState();r.Begin(0,first);r.SkipPreparation();return r;}
        private WaveRuntimeState Resolved(){var r=Start();for(int i=0;i<5;i++)r.RegisterSpawn();for(int i=0;i<5;i++)r.RegisterDeath();return r;}
    }
}
