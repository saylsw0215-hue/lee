using System.Collections.Generic;
using HeroDefense.Battle;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Economy;
using HeroDefense.Battle.Production;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HeroDefense.Collection;

namespace HeroDefense.UI.Buildings
{
    /// <summary>Binds building UI to placement, economy, production, upgrade, sale, and reset services.</summary>
    public sealed class BuildingSystemController
    {
        public event System.Action BuildingInstalled;public event System.Action BuildingSold;public event System.Action BuildingUpgraded;
        private readonly BattleSessionState session; private readonly PauseController pause; private readonly BattleCombatController combat;
        private readonly BuildingEconomyService economy; private readonly PlayerUnitLimitService limit=new(30);
        private readonly List<BuildSlotView> slots=new(8); private readonly List<ProductionBuilding> buildings=new(8);
        private readonly Dictionary<BuildingData,Button> buildButtons=new();
        private readonly RectTransform safe,world; private readonly Text status,infoText,sellPrompt; private readonly GameObject infoPanel,sellPanel;
        private readonly Button upgradeButton,sellButton; private BuildingData selectedData; private ProductionBuilding selectedBuilding; private bool processing;
        private RectTransform dragPreview;private Image dragPreviewImage;private BuildSlotView hoveredSlot;private bool dragging;

        public BuildingSystemController(RectTransform safe,RectTransform battleWorld,Transform buttonRow,Transform statusParent,BattleSessionState state,PauseController pauseController,BattleCombatController battleCombat)
        {
            session=state;pause=pauseController;combat=battleCombat;this.safe=safe;world=battleWorld;economy=new BuildingEconomyService(state);
            status=global::HeroDefense.UI.UiFactory.Label(statusParent,"BuildingStatus","건물 카드를 슬롯으로 드래그하세요",25,TextAnchor.MiddleCenter,Color.white);
            string[] buildingNames={"Barracks","ArcheryRange","MagicTower","GuardBarracks","SiegeWorkshop","Sanctuary"};for(int i=0;i<buildingNames.Length;i++)AddBuildButton(buttonRow,Load(buildingNames[i]));
            global::HeroDefense.UI.UiFactory.Button(buttonRow,"CancelBuild","선택 취소",new Color(.3f,.31f,.35f)).onClick.AddListener(ClearBuildSelection);
            CreateSlots();
            (infoPanel,infoText,upgradeButton,sellButton)=CreateInfoPanel(safe); sellPanel=CreateSellPanel(safe,out sellPrompt);
            infoPanel.SetActive(false);sellPanel.SetActive(false);session.Changed+=RefreshAffordability;combat.BattleReset+=ResetAll;combat.DefeatStateChanged+=OnDefeat;
            RefreshAffordability();
        }
        private static BuildingData Load(string name)=>RuntimeBuildingCatalog.Get(name);
        private void AddBuildButton(Transform row,BuildingData data)
        {
            if(data==null)return;var button=global::HeroDefense.UI.UiFactory.Button(row,data.BuildingId,$"{data.DisplayName}\n{data.BuildCost} Gold",Color.Lerp(data.BuildingColor,new Color(.06f,.08f,.12f),.48f));
            Sprite artwork=global::HeroDefense.UI.GameArtwork.GetBuildingSprite(data.BuildingId);if(artwork!=null){var artObject=new GameObject("Artwork",typeof(RectTransform),typeof(Image));artObject.transform.SetParent(button.transform,false);var art=artObject.GetComponent<Image>();art.sprite=artwork;art.preserveAspect=true;art.raycastTarget=false;global::HeroDefense.UI.UiFactory.Stretch(art.rectTransform,new Vector2(.03f,.1f),new Vector2(.34f,.9f));var text=button.GetComponentInChildren<Text>();text.rectTransform.anchorMin=new Vector2(.31f,0);text.rectTransform.anchorMax=Vector2.one;text.fontSize=22;}
            button.onClick.AddListener(()=>ToggleBuildSelection(data));var drag=button.gameObject.AddComponent<BuildingDragHandle>();drag.Initialize(data,this);buildButtons[data]=button;
        }
        private void CreateSlots()
        {
            for(int i=0;i<8;i++)
            {
                var go=new GameObject($"BuildSlot_{i+1}",typeof(RectTransform),typeof(Image),typeof(Button),typeof(BuildSlotView));go.transform.SetParent(world,false);
                var view=go.GetComponent<BuildSlotView>();view.Initialize((i+1).ToString(),OnSlotClicked);
                var rect=view.RectTransform;rect.sizeDelta=new Vector2(125,120);int column=i%4,row=i/4;rect.anchoredPosition=new Vector2(-490+column*145,row==0?170:-175);slots.Add(view);
            }
        }
        private void ToggleBuildSelection(BuildingData data)
        {
            if(IsBlocked()||data==null)return;bool cancel=selectedData==data;SelectInstalled(null);selectedData=cancel?null:data;RefreshSlots();
            status.text=selectedData==null?"건물 카드를 슬롯으로 드래그하세요":$"{data.DisplayName} 선택 · 빈 슬롯을 누르거나 드래그하세요";
        }
        private void ClearBuildSelection(){CancelDrag();selectedData=null;RefreshSlots();status.text="건물 카드를 슬롯으로 드래그하세요";}

        internal void BeginDrag(BuildingData data,PointerEventData eventData)
        {
            if(IsBlocked()||data==null||!session.CanAfford(data.BuildCost))return;
            SelectInstalled(null);selectedData=data;dragging=true;CreateDragPreview(data);MoveDragPreview(eventData);RefreshSlots();status.text=$"{data.DisplayName} 배치 중 · 금색 슬롯에 놓으세요";
        }
        internal void Drag(PointerEventData eventData){if(!dragging)return;MoveDragPreview(eventData);UpdateHoveredSlot(eventData);}
        internal void EndDrag(PointerEventData eventData)
        {
            if(!dragging)return;UpdateHoveredSlot(eventData);BuildSlotView target=hoveredSlot;DestroyDragPreview();dragging=false;hoveredSlot=null;RefreshSlots();
            if(target!=null&&!target.State.IsOccupied)OnSlotClicked(target);else status.text=$"{selectedData?.DisplayName??"건물"} 배치 취소 · 빈 슬롯 위에 놓으세요";
        }
        private void CreateDragPreview(BuildingData data)
        {
            DestroyDragPreview();var go=new GameObject("BuildingDragPreview",typeof(RectTransform),typeof(Image),typeof(CanvasGroup));go.transform.SetParent(safe,false);go.transform.SetAsLastSibling();dragPreview=go.GetComponent<RectTransform>();dragPreview.sizeDelta=new Vector2(150,140);dragPreviewImage=go.GetComponent<Image>();dragPreviewImage.sprite=global::HeroDefense.UI.GameArtwork.GetBuildingSprite(data.BuildingId)??global::HeroDefense.UI.UiFactory.RoundedSprite;dragPreviewImage.preserveAspect=true;dragPreviewImage.color=new Color(1,1,1,.82f);var group=go.GetComponent<CanvasGroup>();group.blocksRaycasts=false;group.alpha=.88f;
        }
        private void MoveDragPreview(PointerEventData eventData){if(dragPreview==null)return;if(RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,eventData.position,eventData.pressEventCamera,out Vector2 local))dragPreview.anchoredPosition=local;}
        private void UpdateHoveredSlot(PointerEventData eventData)
        {
            hoveredSlot=null;for(int i=0;i<slots.Count;i++){BuildSlotView slot=slots[i];if(!slot.State.IsOccupied&&RectTransformUtility.RectangleContainsScreenPoint(slot.RectTransform,eventData.position,eventData.pressEventCamera)){hoveredSlot=slot;break;}}RefreshSlots();
        }
        private void DestroyDragPreview(){if(dragPreview!=null)Object.Destroy(dragPreview.gameObject);dragPreview=null;dragPreviewImage=null;}
        private void CancelDrag(){dragging=false;hoveredSlot=null;DestroyDragPreview();}
        private void OnSlotClicked(BuildSlotView slot)
        {
            if(processing||IsBlocked())return;
            if(slot.State.IsOccupied){SelectInstalled(Find(slot.State.Occupant));return;}
            if(selectedData==null){status.text="먼저 건물을 선택하세요";return;}
            if(!selectedData.Validate(out _)){status.text="건물 데이터가 올바르지 않습니다";return;}
            processing=true;
            if(!economy.TryPayBuild(selectedData)){status.text="골드가 부족합니다.";processing=false;RefreshAffordability();return;}
            var runtime=new BuildingRuntimeState(selectedData);
            if(!slot.State.TryOccupy(runtime)){session.AddGold(selectedData.BuildCost);processing=false;return;}
            var go=new GameObject(selectedData.BuildingId,typeof(RectTransform),typeof(ProductionBuilding));go.transform.SetParent(slot.transform,false);
            var building=go.GetComponent<ProductionBuilding>();building.Initialize(selectedData,slot,TryProduce,IsBlocked);
            // ProductionBuilding owns the authoritative runtime state; swap the temporary occupancy atomically.
            slot.State.Release(runtime);slot.State.TryOccupy(building.Runtime);buildings.Add(building);building.Changed+=OnBuildingChanged;
            status.text=$"{selectedData.DisplayName} 건설 중";selectedData=null;processing=false;RefreshSlots();RefreshAffordability();
            BuildingInstalled?.Invoke();
            CollectionService.Record(runtime.Data.BuildingId,CollectionEvent.Used);
        }
        private bool TryProduce(UnitData unit,Vector2 position)=>!IsBlocked()&&limit.CanProduce(combat.ActivePlayerCount)&&combat.TrySpawnProduced(unit,position);
        private void SelectInstalled(ProductionBuilding building)
        {
            if(selectedBuilding!=null)selectedBuilding.SetSelected(false);selectedBuilding=building;selectedData=null;RefreshSlots();
            if(building==null){infoPanel.SetActive(false);sellPanel.SetActive(false);return;}
            building.SetSelected(true);infoPanel.SetActive(true);RefreshInfo();
        }
        private ProductionBuilding Find(BuildingRuntimeState runtime){for(int i=0;i<buildings.Count;i++)if(buildings[i]!=null&&buildings[i].Runtime==runtime)return buildings[i];return null;}
        private void UpgradeSelected()
        {
            if(processing||IsBlocked()||selectedBuilding==null)return;processing=true;var building=selectedBuilding;int old=building.Runtime.CurrentLevel;
            if(!economy.TryUpgrade(building.Runtime)){status.text=building.Runtime.IsMaxLevel?"최대 레벨":"업그레이드 골드가 부족합니다";processing=false;RefreshInfo();return;}
            // Economy advanced runtime; preserve production progress and refresh the visual through its upgrade path is no longer possible, so notify directly.
            building.RefreshAfterExternalUpgrade(old);status.text="업그레이드 완료";processing=false;RefreshInfo();RefreshAffordability();
            BuildingUpgraded?.Invoke();
        }
        private void AskSell(){if(IsBlocked()||selectedBuilding==null)return;sellPrompt.text=$"{selectedBuilding.Runtime.Data.DisplayName}을 {selectedBuilding.Runtime.Data.CalculateSellValue(selectedBuilding.Runtime.CurrentLevel)} Gold에 판매하시겠습니까?";sellPanel.SetActive(true);}
        private void ConfirmSell()
        {
            if(processing||IsBlocked()||selectedBuilding==null)return;processing=true;var target=selectedBuilding;int value=economy.TrySell(target.Runtime);
            if(value>0){target.StopImmediately();target.Changed-=OnBuildingChanged;target.Slot.State.Release(target.Runtime);buildings.Remove(target);Object.Destroy(target.gameObject);status.text=$"판매 +{value} Gold";BuildingSold?.Invoke();}
            selectedBuilding=null;infoPanel.SetActive(false);sellPanel.SetActive(false);processing=false;RefreshSlots();RefreshAffordability();
        }
        private (GameObject,Text,Button,Button) CreateInfoPanel(RectTransform safe)
        {
            var panel=global::HeroDefense.UI.UiFactory.Panel(safe,"BuildingInfo",new Color(.04f,.07f,.12f,.96f),new Vector2(.72f,.31f),new Vector2(.985f,.83f)).gameObject;
            var column=global::HeroDefense.UI.UiFactory.Vertical(panel.transform,"Column",12);global::HeroDefense.UI.UiFactory.Stretch(column,new Vector2(.06f,.05f),new Vector2(.94f,.95f));
            var text=global::HeroDefense.UI.UiFactory.Label(column,"Info","",27,TextAnchor.MiddleCenter,Color.white);text.gameObject.AddComponent<LayoutElement>().preferredHeight=260;
            var upgrade=global::HeroDefense.UI.UiFactory.Button(column,"Upgrade","업그레이드",new Color(.18f,.48f,.3f));upgrade.onClick.AddListener(UpgradeSelected);
            var sell=global::HeroDefense.UI.UiFactory.Button(column,"Sell","판매",new Color(.55f,.28f,.12f));sell.onClick.AddListener(AskSell);
            global::HeroDefense.UI.UiFactory.Button(column,"Close","닫기",new Color(.25f,.3f,.4f)).onClick.AddListener(()=>SelectInstalled(null));return(panel,text,upgrade,sell);
        }
        private GameObject CreateSellPanel(RectTransform safe,out Text prompt)
        {
            var panel=global::HeroDefense.UI.UiFactory.Panel(safe,"SellConfirmation",new Color(0,0,0,.88f),new Vector2(.28f,.27f),new Vector2(.72f,.73f)).gameObject;
            var column=global::HeroDefense.UI.UiFactory.Vertical(panel.transform,"Column",16);global::HeroDefense.UI.UiFactory.Stretch(column,new Vector2(.08f,.08f),new Vector2(.92f,.92f));
            prompt=global::HeroDefense.UI.UiFactory.Label(column,"Prompt","",32,TextAnchor.MiddleCenter,Color.white);prompt.gameObject.AddComponent<LayoutElement>().preferredHeight=190;
            global::HeroDefense.UI.UiFactory.Button(column,"Confirm","판매",new Color(.62f,.25f,.12f)).onClick.AddListener(ConfirmSell);
            global::HeroDefense.UI.UiFactory.Button(column,"Cancel","취소",new Color(.25f,.32f,.42f)).onClick.AddListener(()=>panel.SetActive(false));return panel;
        }
        private void OnBuildingChanged(ProductionBuilding value){if(value==selectedBuilding)RefreshInfo();}
        private void RefreshInfo()
        {
            if(selectedBuilding==null)return;var runtime=selectedBuilding.Runtime;var data=runtime.Data;int level=runtime.CurrentLevel;
            string next=runtime.IsMaxLevel?"최대 레벨":$"다음: {data.GetProductionInterval(level+1):0.0}초";
            infoText.text=$"{data.DisplayName} Lv.{level}\n생산: {data.ProducedUnit.DisplayName}\n주기: {data.GetProductionInterval(level):0.0}초\n{next}\n판매: {data.CalculateSellValue(level)}";
            upgradeButton.interactable=!runtime.IsMaxLevel&&!runtime.IsConstructing&&!IsBlocked();upgradeButton.GetComponentInChildren<Text>().text=runtime.IsMaxLevel?"최대 레벨":$"업그레이드 {data.GetUpgradeCost(level)}";sellButton.interactable=!IsBlocked();
        }
        private void RefreshSlots(){for(int i=0;i<slots.Count;i++)slots[i].Refresh(selectedData!=null,slots[i]==hoveredSlot);}
        private void RefreshAffordability(){foreach(var pair in buildButtons)pair.Value.interactable=!combat.IsStageEnded&&!pause.IsPaused&&session.CanAfford(pair.Key.BuildCost);}
        private bool IsBlocked()=>pause.IsPaused||combat.IsStageEnded;
        private void OnDefeat(bool value){RefreshAffordability();if(value)ClearBuildSelection();}
        public void ResetAll()
        {
            CancelDrag();for(int i=buildings.Count-1;i>=0;i--){var b=buildings[i];if(b==null)continue;b.StopImmediately();b.Changed-=OnBuildingChanged;Object.Destroy(b.gameObject);}buildings.Clear();
            for(int i=0;i<slots.Count;i++)slots[i].State.Reset();selectedData=null;selectedBuilding=null;infoPanel.SetActive(false);sellPanel.SetActive(false);processing=false;RefreshSlots();RefreshAffordability();
        }
        public void Dispose(){CancelDrag();session.Changed-=RefreshAffordability;combat.BattleReset-=ResetAll;combat.DefeatStateChanged-=OnDefeat;for(int i=0;i<buildings.Count;i++)if(buildings[i]!=null)buildings[i].Changed-=OnBuildingChanged;buildings.Clear();}
    }

    /// <summary>Routes mouse and touch drag gestures from a building card to the placement controller.</summary>
    public sealed class BuildingDragHandle:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        private BuildingData data;private BuildingSystemController owner;
        public void Initialize(BuildingData value,BuildingSystemController controller){data=value;owner=controller;}
        public void OnBeginDrag(PointerEventData eventData)=>owner?.BeginDrag(data,eventData);
        public void OnDrag(PointerEventData eventData)=>owner?.Drag(eventData);
        public void OnEndDrag(PointerEventData eventData)=>owner?.EndDrag(eventData);
    }
}
