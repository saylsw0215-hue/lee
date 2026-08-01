using System.IO;
using HeroDefense.Meta;
using HeroDefense.Save;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests.EditMode
{
    public sealed class Phase9SaveTests
    {
        private string directory;private JsonFileSaveRepository repository;
        [SetUp]public void Setup(){directory=Path.Combine(Application.temporaryCachePath,"HeroDefensePhase9Tests",System.Guid.NewGuid().ToString("N"));repository=new JsonFileSaveRepository(directory);}
        [TearDown]public void Cleanup(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [Test]public void DefaultSaveIsValid(){var data=DefaultSaveFactory.Create();Assert.That(data.saveVersion,Is.EqualTo(1));Assert.That(SaveValidationService.Normalize(data,out _),Is.True);Assert.That(data.heroes.Count,Is.EqualTo(6));Assert.That(data.stages.Count,Is.EqualTo(4));}
        [Test]public void RoundTripAndBackupWork(){var data=DefaultSaveFactory.Create();data.currencies.coin=123;Assert.That(repository.Save(data).Success,Is.True);data.currencies.coin=456;Assert.That(repository.Save(data).Success,Is.True);Assert.That(repository.Load().Data.currencies.coin,Is.EqualTo(456));Assert.That(File.Exists(repository.BackupPath),Is.True);}
        [Test]public void CorruptMainRecoversBackup(){var data=DefaultSaveFactory.Create();data.currencies.coin=77;repository.Save(data);repository.Save(data);File.WriteAllText(repository.MainPath,"{broken");var loaded=repository.Load();Assert.That(loaded.Success,Is.True);Assert.That(loaded.Recovered,Is.True);Assert.That(loaded.Data.currencies.coin,Is.EqualTo(77));}
        [Test]public void ValidationClampsUnsafeValues(){var data=DefaultSaveFactory.Create();data.currencies.coin=-2;data.settings.masterVolume=9;data.endless.highestWave=-5;Assert.That(SaveValidationService.Normalize(data,out _),Is.True);Assert.That(data.currencies.coin,Is.Zero);Assert.That(data.settings.masterVolume,Is.EqualTo(1));Assert.That(data.endless.highestWave,Is.Zero);}
        [Test]public void WalletNeverOverspends(){var go=new GameObject("SaveTest");var manager=go.AddComponent<SaveGameManager>();manager.InitializeForTests(repository);manager.Data.currencies.coin=10;var wallet=new CurrencyWallet(manager);Assert.That(wallet.TrySpend(11),Is.False);Assert.That(wallet.Coin,Is.EqualTo(10));Object.DestroyImmediate(go);}
        [Test]public void UpgradeCostAndMaximumAreStable(){var item=MetaUpgradeCatalog.All[0];Assert.That(MetaUpgradeCatalog.Cost(item,1),Is.GreaterThan(MetaUpgradeCatalog.Cost(item,0)));Assert.That(item.MaxLevel,Is.GreaterThan(0));Assert.That(MetaUpgradeCatalog.All.Length,Is.GreaterThanOrEqualTo(18));}
        [Test]public void DuplicateBattleRewardIsRejected(){var go=new GameObject("SaveTest");var manager=go.AddComponent<SaveGameManager>();manager.InitializeForTests(repository);var stats=new HeroDefense.Battle.Statistics.BattleStatistics();stats.SelectHero("hero_arden_knight");var service=new BattleResultProgressService(manager);var first=service.Record("result",true,stats,100);var second=service.Record("result",true,stats,100);Assert.That(first.Coin,Is.GreaterThan(0));Assert.That(second.Coin,Is.Zero);Object.DestroyImmediate(go);}
    }
}
