using System.Reflection;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Stages;
using HeroDefense.Battle.Waves;
using HeroDefense.Core;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests.EditMode
{
    public sealed class ContentDatabaseValidationTests
    {
        [Test]
        public void RuntimeCatalogsPassDetailedValidation()
        {
            Assert.That(GameContentDatabase.Validate(out string reason),Is.True,reason);
        }

        [Test]
        public void DuplicateContentIdReportsTypeIdAndIndex()
        {
            UnitData first=Unit("duplicate_unit");
            UnitData second=Unit("duplicate_unit");
            Assert.That(Validate(RuntimeHeroCatalog.GetHeroes(),new[]{first,second},RuntimeBuildingCatalog.GetAll(),RuntimeStageCatalog.GetAll(),RuntimeStatusCatalog.GetAll(),out string reason),Is.False);
            StringAssert.Contains("duplicate_unit",reason);
            StringAssert.Contains("Unit catalog[1]",reason);
        }

        [Test]
        public void MissingHeroSkillReportsHeroIdAndField()
        {
            HeroData hero=ScriptableObject.CreateInstance<HeroData>();
            hero.Configure("hero_invalid_skill","Invalid","Test","Test",HeroArchetype.Knight,100,1,10,1,1,5,1,null,null,null,Color.white,HeroVisualShape.Knight);
            Assert.That(Validate(new[]{hero},RuntimeUnitCatalog.GetAll(),RuntimeBuildingCatalog.GetAll(),RuntimeStageCatalog.GetAll(),RuntimeStatusCatalog.GetAll(),out string reason),Is.False);
            StringAssert.Contains("hero_invalid_skill",reason);
            StringAssert.Contains("ActiveSkill",reason);
        }

        [Test]
        public void MissingSpawnEnemyReportsWaveAndGroupField()
        {
            WaveData wave=ScriptableObject.CreateInstance<WaveData>();
            wave.Configure("wave_invalid_enemy","Invalid",0,0,0,false,true,"",new[]{new WaveSpawnGroup(null,1,0,0,0)});
            StageData stage=ScriptableObject.CreateInstance<StageData>();
            stage.Configure("stage_invalid_enemy","Invalid","",0,100,0,"",Color.white,new[]{wave});
            Assert.That(Validate(RuntimeHeroCatalog.GetHeroes(),RuntimeUnitCatalog.GetAll(),RuntimeBuildingCatalog.GetAll(),new[]{stage},RuntimeStatusCatalog.GetAll(),out string reason),Is.False);
            StringAssert.Contains("wave_invalid_enemy",reason);
            StringAssert.Contains("SpawnGroups[0].EnemyData",reason);
        }

        [Test]
        public void MissingBuildingProducedUnitReportsBuildingIdAndField()
        {
            BuildingData building=ScriptableObject.CreateInstance<BuildingData>();
            building.Configure("building_invalid_unit","Invalid","",10,.5f,null,new[]{new BuildingLevelData(1,0)},Color.white,Vector2.one,BuildingVisualShape.Barracks);
            Assert.That(Validate(RuntimeHeroCatalog.GetHeroes(),RuntimeUnitCatalog.GetAll(),new[]{building},RuntimeStageCatalog.GetAll(),RuntimeStatusCatalog.GetAll(),out string reason),Is.False);
            StringAssert.Contains("building_invalid_unit",reason);
            StringAssert.Contains("ProducedUnit",reason);
        }

        [Test]
        public void InvalidUnitNumberReportsUnitIdAndField()
        {
            UnitData unit=Unit("unit_invalid_speed",-1);
            Assert.That(Validate(RuntimeHeroCatalog.GetHeroes(),new[]{unit},RuntimeBuildingCatalog.GetAll(),RuntimeStageCatalog.GetAll(),RuntimeStatusCatalog.GetAll(),out string reason),Is.False);
            StringAssert.Contains("unit_invalid_speed",reason);
            StringAssert.Contains("MoveSpeed",reason);
        }

        [Test]
        public void RuntimeValidationLivesInPlayerRuntimeAssembly()
        {
            Assert.That(typeof(GameContentDatabase).Assembly.GetName().Name,Is.EqualTo("HeroDefense.Runtime"));
            Assert.That(typeof(GameContentDatabase).GetMethod(nameof(GameContentDatabase.Validate),BindingFlags.Public|BindingFlags.Static),Is.Not.Null);
            Assert.That(GameContentDatabase.Validate(out string reason),Is.True,reason);
        }

        private static UnitData Unit(string id,float speed=1)
        {
            UnitData data=ScriptableObject.CreateInstance<UnitData>();
            data.Configure(id,"Test Unit",Team.Player,100,speed,10,1,1,5,.5f,0,Color.white,UnitVisualShape.Swordsman);
            return data;
        }

        private static bool Validate(HeroData[] heroes,UnitData[] units,BuildingData[] buildings,StageData[] stages,StatusEffectData[] statuses,out string reason)
        {
            MethodInfo method=typeof(GameContentDatabase).GetMethod("ValidateCatalogs",BindingFlags.NonPublic|BindingFlags.Static);
            Assert.That(method,Is.Not.Null);
            object[] arguments={heroes,units,buildings,stages,statuses,null};
            bool result=(bool)method.Invoke(null,arguments);
            reason=(string)arguments[5];
            return result;
        }
    }
}
