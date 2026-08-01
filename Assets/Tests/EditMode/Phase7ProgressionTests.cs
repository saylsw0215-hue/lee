using System.Collections.Generic;
using HeroDefense.Battle;
using HeroDefense.Progression;
using NUnit.Framework;

namespace HeroDefense.Tests
{
    public sealed class Phase7ProgressionTests
    {
        [TearDown]public void Cleanup(){UnityEngine.Time.timeScale=1;BattleModifierRepository.Current=null;}
        [Test]public void ExperienceAccumulates(){var xp=new BattleExperienceManager();xp.AddExperience(25);Assert.AreEqual(25,xp.CurrentExperience);Assert.AreEqual(25,xp.TotalExperience);}
        [Test]public void RequiredExperienceLevelsUp(){var xp=new BattleExperienceManager();xp.AddExperience(60);Assert.AreEqual(2,xp.Level);Assert.AreEqual(1,xp.PendingSelections);}
        [Test]public void OneAwardCanGainSeveralLevels(){var xp=new BattleExperienceManager(new[]{10,10,10});Assert.AreEqual(3,xp.AddExperience(35));Assert.AreEqual(4,xp.Level);Assert.AreEqual(3,xp.PendingSelections);}
        [Test]public void MaximumLevelCannotBeExceeded(){var xp=new BattleExperienceManager(new[]{1});xp.AddExperience(100);Assert.AreEqual(2,xp.Level);Assert.IsTrue(xp.IsMaxLevel);Assert.AreEqual(0,xp.AddExperience(100));}
        [Test]public void PendingSelectionsAreConsumedOneAtATime(){var xp=new BattleExperienceManager(new[]{1,1});xp.AddExperience(2);Assert.IsTrue(xp.ConsumeSelection());Assert.AreEqual(1,xp.PendingSelections);}
        [Test]public void EnemyExperienceRewardsMatchDesign(){Assert.AreEqual(8,ExperienceRewardService.ForEnemy("enemy_slime"));Assert.AreEqual(45,ExperienceRewardService.ForEnemy("enemy_elite_goblin"));Assert.AreEqual(200,ExperienceRewardService.ForEnemy("boss_goblin_chieftain"));}
        [Test]public void CatalogContainsAtLeastFortyCards(){Assert.GreaterOrEqual(RuntimeUpgradeCatalog.All.Count,40);}
        [Test]public void CandidateRollHasNoDuplicates(){var inventory=new BattleUpgradeInventory();var roll=new UpgradeCandidateService(new SeededRandomProvider(7)).Roll(RuntimeUpgradeCatalog.All,inventory,6,"hero_arden_knight");Assert.AreEqual(3,roll.Count);Assert.AreNotEqual(roll[0].UpgradeId,roll[1].UpgradeId);Assert.AreNotEqual(roll[1].UpgradeId,roll[2].UpgradeId);}
        [Test]public void SameSeedReproducesCards(){var inventory=new BattleUpgradeInventory();var a=new UpgradeCandidateService(new SeededRandomProvider(42)).Roll(RuntimeUpgradeCatalog.All,inventory,8,"hero_rian_ranger");var b=new UpgradeCandidateService(new SeededRandomProvider(42)).Roll(RuntimeUpgradeCatalog.All,inventory,8,"hero_rian_ranger");for(int i=0;i<3;i++)Assert.AreEqual(a[i].UpgradeId,b[i].UpgradeId);}
        [Test]public void HeroExclusiveCardsAreFiltered(){var inventory=new BattleUpgradeInventory();for(int seed=0;seed<20;seed++){var cards=new UpgradeCandidateService(new SeededRandomProvider(seed)).Roll(RuntimeUpgradeCatalog.All,inventory,20,"hero_arden_knight",null,20);for(int i=0;i<cards.Count;i++)Assert.IsTrue(string.IsNullOrEmpty(cards[i].HeroId)||cards[i].HeroId=="hero_arden_knight");}}
        [Test]public void MaxedCardIsExcluded(){var inventory=new BattleUpgradeInventory();UpgradeData card=RuntimeUpgradeCatalog.All[0];for(int i=0;i<card.MaxLevel;i++)inventory.Select(card,1);Assert.IsFalse(inventory.CanSelect(card));}
        [Test]public void ModifierRepositoryAppliesProductionCap(){var inventory=new BattleUpgradeInventory();UpgradeData speed=null;foreach(var item in RuntimeUpgradeCatalog.All)if(item.UpgradeId=="upgrade_building_production_speed")speed=item;for(int i=0;i<speed.MaxLevel;i++)inventory.Select(speed,5);var repository=new BattleModifierRepository();repository.Rebuild(inventory);Assert.GreaterOrEqual(repository.ProductionIntervalMultiplier,.5f);Assert.Less(repository.ProductionIntervalMultiplier,1);}
        [Test]public void PauseReasonsDoNotResumeOtherReason(){var pause=new PauseController();pause.PauseFor(GamePauseReason.UserPause);pause.PauseFor(GamePauseReason.LevelUpSelection);pause.ResumeReason(GamePauseReason.LevelUpSelection);Assert.IsTrue(pause.IsPaused);pause.ResumeReason(GamePauseReason.UserPause);Assert.IsFalse(pause.IsPaused);}
    }
}
