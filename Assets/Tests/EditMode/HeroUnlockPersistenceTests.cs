using System;
using System.IO;
using HeroDefense.Meta;
using HeroDefense.Save;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests.EditMode
{
    public sealed class HeroUnlockPersistenceTests
    {
        private string directory;
        private GameObject owner;
        private SaveGameManager manager;

        [SetUp]
        public void SetUp()
        {
            directory=Path.Combine(Application.temporaryCachePath,"HeroUnlockPersistenceTests",Guid.NewGuid().ToString("N"));
            owner=new GameObject("HeroUnlockSave",typeof(SaveGameManager));
            manager=owner.GetComponent<SaveGameManager>();
            manager.InitializeForTests(new JsonFileSaveRepository(directory));
        }

        [TearDown]
        public void TearDown()
        {
            if(owner!=null)UnityEngine.Object.DestroyImmediate(owner);
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }

        [Test]
        public void EligibleHeroUnlocksImmediatelyAndPersistsAfterReload()
        {
            SaveRecords.Stage(manager.Data,"stage_01_grassland").normalCleared=true;
            manager.Data.currencies.coin=500;
            var unlocks=new HeroUnlockService(manager);

            Assert.That(unlocks.TryUnlock("hero_kai_engineer"),Is.True);
            Assert.That(unlocks.IsUnlocked("hero_kai_engineer"),Is.True);
            Assert.That(manager.Data.currencies.coin,Is.Zero);
            Assert.That(manager.SaveNow(SaveReason.Manual),Is.True);

            manager.LoadOrCreate();
            Assert.That(new HeroUnlockService(manager).IsUnlocked("hero_kai_engineer"),Is.True);
        }

        [Test]
        public void InsufficientCurrencyKeepsHeroLockedAndDoesNotSpend()
        {
            SaveRecords.Stage(manager.Data,"stage_01_grassland").normalCleared=true;
            manager.Data.currencies.coin=499;
            var unlocks=new HeroUnlockService(manager);

            Assert.That(unlocks.TryUnlock("hero_kai_engineer"),Is.False);
            Assert.That(unlocks.IsUnlocked("hero_kai_engineer"),Is.False);
            Assert.That(manager.Data.currencies.coin,Is.EqualTo(499));
        }
    }
}
