using HeroDefense.Battle.Waves;
using HeroDefense.Core;
using NUnit.Framework;

namespace HeroDefense.Tests.EditMode
{
    public sealed class Phase8ContentTests
    {
        [Test]public void RegistryContainsExpandedContent(){Assert.That(GameContentDatabase.Validate(out string reason),Is.True,reason);Assert.That(GameContentDatabase.Heroes.Count,Is.EqualTo(6));Assert.That(GameContentDatabase.Buildings.Count,Is.EqualTo(6));Assert.That(GameContentDatabase.Stages.Count,Is.EqualTo(4));}
        [Test]public void SessionDefaultsAreSafe(){BattleLaunchConfig.Reset();Assert.That(BattleLaunchConfig.SelectedStage,Is.Not.Null);Assert.That(BattleLaunchConfig.Difficulty,Is.EqualTo(GameDifficulty.Normal));}
        [Test]public void DifficultyScalesWithoutMutatingContent(){float original=GameContentDatabase.Unit("enemy_slime").MaxHealth;Assert.That(DifficultyModifiers.For(GameDifficulty.Easy).EnemyHealth,Is.LessThan(1));Assert.That(DifficultyModifiers.For(GameDifficulty.Hard).EnemyHealth,Is.GreaterThan(1));Assert.That(GameContentDatabase.Unit("enemy_slime").MaxHealth,Is.EqualTo(original));}
        [TestCase(5,true,false)][TestCase(10,false,true)][TestCase(11,false,false)]public void EndlessCadence(int wave,bool elite,bool boss){Assert.That(EndlessWaveGenerator.IsElite(wave),Is.EqualTo(elite));Assert.That(EndlessWaveGenerator.IsBoss(wave),Is.EqualTo(boss));}
        [Test]public void EndlessScalingIncreases(){Assert.That(EndlessWaveGenerator.HealthMultiplier(20),Is.GreaterThan(EndlessWaveGenerator.HealthMultiplier(10)));Assert.That(EndlessWaveGenerator.BossForWave(10).UnitId,Is.Not.EqualTo(EndlessWaveGenerator.BossForWave(20).UnitId));}
    }
}
