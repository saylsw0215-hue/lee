using System.Collections.Generic;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Stages;
using HeroDefense.Core;
using HeroDefense.Heroes.Selection;
using HeroDefense.Save;
using NUnit.Framework;

namespace HeroDefense.Tests.EditMode
{
    /// <summary>Protects the player-authored runtime catalog as the single source used by every target.</summary>
    public sealed class RuntimeCatalogParityTests
    {
        [Test]
        public void RuntimeCatalogsContainExpectedContentWithoutNullsOrDuplicateIds()
        {
            AssertCatalog(RuntimeHeroCatalog.GetHeroes(),6,value=>value.HeroId);
            AssertCatalog(RuntimeUnitCatalog.GetAll(),25,value=>value.UnitId);
            AssertCatalog(RuntimeBuildingCatalog.GetAll(),6,value=>value.BuildingId);
            AssertCatalog(RuntimeStageCatalog.GetAll(),4,value=>value.StageId);
            AssertCatalog(RuntimeStatusCatalog.GetAll(),10,value=>value.EffectId);
        }

        [Test]
        public void RepeatedCatalogRequestsReturnTheSameObjects()
        {
            Assert.That(RuntimeUnitCatalog.Get("EnemySlime"),Is.SameAs(RuntimeUnitCatalog.Get("EnemySlime")));
            Assert.That(RuntimeBuildingCatalog.Get("Barracks"),Is.SameAs(RuntimeBuildingCatalog.Get("Barracks")));
            Assert.That(RuntimeStageCatalog.GetStageOne(),Is.SameAs(RuntimeStageCatalog.GetStageOne()));
            Assert.That(RuntimeStatusCatalog.Get("Burn"),Is.SameAs(RuntimeStatusCatalog.Get("Burn")));
            Assert.That(RuntimeHeroCatalog.GetHeroes(),Is.SameAs(RuntimeHeroCatalog.GetHeroes()));
        }

        [Test]
        public void StageAndBuildingReferencesComeFromSharedUnitCatalog()
        {
            var units=new Dictionary<string,UnitData>();
            foreach(UnitData unit in RuntimeUnitCatalog.GetAll())units.Add(unit.UnitId,unit);
            foreach(BuildingData building in RuntimeBuildingCatalog.GetAll())
                Assert.That(building.ProducedUnit,Is.SameAs(units[building.ProducedUnit.UnitId]),building.BuildingId);
            foreach(StageData stage in RuntimeStageCatalog.GetAll())
                foreach(var wave in stage.Waves)
                    foreach(var group in wave.SpawnGroups)
                        Assert.That(group.EnemyData,Is.SameAs(units[group.EnemyData.UnitId]),$"{stage.StageId}/{wave.WaveId}");
        }

        [Test]
        public void HeroesAndFinalWavesAreValid()
        {
            foreach(var hero in RuntimeHeroCatalog.GetHeroes())
            {
                Assert.That(hero.ActiveSkill,Is.Not.Null,hero.HeroId);
                Assert.That(hero.UltimateSkill,Is.Not.Null,hero.HeroId);
                Assert.That(hero.Passive,Is.Not.Null,hero.HeroId);
                Assert.That(hero.ActiveSkill.Validate(out string activeReason),Is.True,$"{hero.HeroId}: {activeReason}");
                Assert.That(hero.UltimateSkill.Validate(out string ultimateReason),Is.True,$"{hero.HeroId}: {ultimateReason}");
                Assert.That(hero.Passive.Validate(out string passiveReason),Is.True,$"{hero.HeroId}: {passiveReason}");
            }
            foreach(StageData stage in RuntimeStageCatalog.GetAll())
                Assert.That(stage.Waves[stage.Waves.Length-1].IsBossWave,Is.True,stage.StageId);
        }

        [Test]
        public void ExistingSaveIdentifiersRemainResolvable()
        {
            GameSaveData save=DefaultSaveFactory.Create();
            foreach(HeroProgressRecord hero in save.heroes)Assert.That(GameContentDatabase.Hero(hero.heroId),Is.Not.Null,hero.heroId);
            foreach(StageCompletionRecord stage in save.stages)Assert.That(GameContentDatabase.Stage(stage.stageId),Is.Not.Null,stage.stageId);
            Assert.That(GameContentDatabase.Building("building_barracks"),Is.Not.Null);
            Assert.That(GameContentDatabase.Unit("player_swordsman"),Is.Not.Null);
        }

        private static void AssertCatalog<T>(T[] values,int expected,System.Func<T,string> id) where T:class
        {
            Assert.That(values,Has.Length.EqualTo(expected));
            var ids=new HashSet<string>();
            for(int i=0;i<values.Length;i++)
            {
                Assert.That(values[i],Is.Not.Null,$"catalog[{i}]");
                string value=id(values[i]);
                Assert.That(value,Is.Not.Null.And.Not.Empty,$"catalog[{i}]");
                Assert.That(ids.Add(value),Is.True,$"duplicate ID: {value}");
            }
        }
    }
}
