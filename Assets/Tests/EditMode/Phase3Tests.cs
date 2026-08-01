using HeroDefense.Battle;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Economy;
using HeroDefense.Battle.Production;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests
{
    public sealed class Phase3Tests
    {
        private BuildingData barracks; private BattleSessionState session; private BuildingEconomyService economy;
        [SetUp] public void Setup(){barracks=RuntimeBuildingCatalog.Get("Barracks");session=new BattleSessionState();economy=new BuildingEconomyService(session);}
        [Test] public void BuildingCost_IsLoaded(){Assert.AreEqual(100,barracks.BuildCost);}
        [Test] public void SpendGold_SucceedsWhenAffordable(){Assert.IsTrue(session.TrySpendGold(100));Assert.AreEqual(400,session.CurrentGold);}
        [Test] public void SpendGold_FailsWhenInsufficient(){Assert.IsFalse(session.TrySpendGold(999));}
        [Test] public void FailedSpend_DoesNotChangeGold(){int before=session.CurrentGold;session.TrySpendGold(999);Assert.AreEqual(before,session.CurrentGold);}
        [Test] public void EmptySlot_CanBeOccupied(){var slot=new BuildSlotState("1");Assert.IsTrue(slot.TryOccupy(new BuildingRuntimeState(barracks)));}
        [Test] public void OccupiedSlot_RejectsDuplicate(){var slot=new BuildSlotState("1");slot.TryOccupy(new BuildingRuntimeState(barracks));Assert.IsFalse(slot.TryOccupy(new BuildingRuntimeState(barracks)));}
        [Test] public void Building_CannotExceedMaxLevel(){var state=Ready();Assert.IsTrue(state.TryUpgrade());Assert.IsTrue(state.TryUpgrade());Assert.IsFalse(state.TryUpgrade());Assert.AreEqual(3,state.CurrentLevel);}
        [Test] public void UpgradeCost_IsDeductedExactly(){var state=Ready();Assert.IsTrue(economy.TryUpgrade(state));Assert.AreEqual(400,session.CurrentGold);}
        [Test] public void SellValue_IncludesHalfInvestedGold(){Assert.AreEqual(50,barracks.CalculateSellValue(1));Assert.AreEqual(100,barracks.CalculateSellValue(2));}
        [Test] public void SaleReward_IsGrantedOnlyOnce(){var state=Ready();int first=economy.TrySell(state);int second=economy.TrySell(state);Assert.AreEqual(50,first);Assert.AreEqual(0,second);Assert.AreEqual(550,session.CurrentGold);}
        [Test] public void ProductionInterval_DecreasesByLevel(){Assert.Greater(barracks.GetProductionInterval(1),barracks.GetProductionInterval(2));Assert.Greater(barracks.GetProductionInterval(2),barracks.GetProductionInterval(3));}
        [Test] public void ProductionProgress_IsCalculated(){var timer=new ProductionTimer();timer.Tick(2.5f,5f);Assert.AreEqual(.5f,timer.Progress(5f),.001f);}
        [Test] public void UnitLimit_WaitsAtMaximum(){var limit=new PlayerUnitLimitService(30);Assert.IsFalse(limit.CanProduce(30));Assert.IsTrue(limit.CanProduce(29));}
        [Test] public void ReleasingBuilding_EmptiesSlot(){var slot=new BuildSlotState("1");var state=new BuildingRuntimeState(barracks);slot.TryOccupy(state);slot.Release(state);Assert.IsFalse(slot.IsOccupied);}
        private BuildingRuntimeState Ready(){var state=new BuildingRuntimeState(barracks){IsConstructing=false};return state;}
    }
}
