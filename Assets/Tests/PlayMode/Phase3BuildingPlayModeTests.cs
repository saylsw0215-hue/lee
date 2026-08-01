using System.Collections;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Production;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HeroDefense.Tests
{
    public sealed class Phase3BuildingPlayModeTests
    {
        private GameObject root; private UnitData sword; private readonly System.Collections.Generic.List<Object> temporary=new();
        [SetUp] public void Setup(){root=new GameObject("Phase3TestRoot",typeof(RectTransform));sword=Resources.Load<UnitData>("UnitData/PlayerSwordsman");}
        [TearDown] public void Teardown(){Time.timeScale=1f;Object.DestroyImmediate(root);for(int i=0;i<temporary.Count;i++)Object.DestroyImmediate(temporary[i]);temporary.Clear();}

        [UnityTest] public IEnumerator Building_AutomaticallyProducesAfterConstruction()
        {
            int produced=0;ProductionBuilding building=Create(.1f,sword,(_,__)=>{produced++;return true;});yield return new WaitForSeconds(.95f);Assert.GreaterOrEqual(produced,1);Assert.IsFalse(building.Runtime.IsConstructing);
        }
        [UnityTest] public IEnumerator Pause_StopsProductionTimer()
        {
            int produced=0;Create(.15f,sword,(_,__)=>{produced++;return true;});yield return new WaitForSeconds(.75f);Time.timeScale=0;yield return null;yield return null;Assert.AreEqual(0,produced);
        }
        [UnityTest] public IEnumerator FullCapacity_KeepsProductionReady()
        {
            bool capacity=false;int produced=0;var building=Create(.1f,sword,(_,__)=>{if(!capacity)return false;produced++;return true;});yield return new WaitForSeconds(.9f);Assert.AreEqual(0,produced);Assert.AreEqual(1f,building.ProductionProgress,.01f);capacity=true;yield return null;Assert.AreEqual(1,produced);
        }
        [UnityTest] public IEnumerator MultipleBuildings_ProduceIndependently()
        {
            int first=0,second=0;Create(.1f,sword,(_,__)=>{first++;return true;},"A");Create(.14f,sword,(_,__)=>{second++;return true;},"B");yield return new WaitForSeconds(1.05f);Assert.Greater(first,0);Assert.Greater(second,0);
        }
        [UnityTest] public IEnumerator ArcherAndMage_DataRemainCompatibleWithProduction()
        {
            UnitData archer=Resources.Load<UnitData>("UnitData/PlayerArcher"),mage=Resources.Load<UnitData>("UnitData/PlayerMage");Assert.AreEqual(Team.Player,archer.Team);Assert.AreEqual(Team.Player,mage.Team);Assert.Greater(archer.AttackRange,sword.AttackRange);Assert.Greater(mage.AttackDamage,archer.AttackDamage);yield return null;
        }
        [UnityTest] public IEnumerator EightBuildings_ProduceIndependently()
        {
            int produced=0;for(int i=0;i<8;i++)Create(.1f,sword,(_,__)=>{produced++;return true;},i.ToString());yield return new WaitForSeconds(.95f);Assert.GreaterOrEqual(produced,8);
        }
        private ProductionBuilding Create(float interval,UnitData unit,System.Func<UnitData,Vector2,bool> callback,string suffix="")
        {
            var data=ScriptableObject.CreateInstance<BuildingData>();temporary.Add(data);data.Configure("test"+suffix,"테스트","",1,.5f,unit,new[]{new BuildingLevelData(interval,0)},Color.blue,new Vector2(80,80),BuildingVisualShape.Barracks);
            var slotObject=new GameObject("Slot"+suffix,typeof(RectTransform),typeof(Image),typeof(Button),typeof(BuildSlotView));slotObject.transform.SetParent(root.transform,false);var slot=slotObject.GetComponent<BuildSlotView>();slot.Initialize(suffix,_=>{});
            var go=new GameObject("Building"+suffix,typeof(RectTransform),typeof(ProductionBuilding));go.transform.SetParent(slot.transform,false);var building=go.GetComponent<ProductionBuilding>();building.Initialize(data,slot,callback,()=>false);slot.State.TryOccupy(building.Runtime);return building;
        }
    }
}
