using System.Collections.Generic;
using HeroDefense.Core;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Save;
using HeroDefense.Meta;
using HeroDefense.Collection;

namespace HeroDefense.UI.Heroes
{
    /// <summary>Builds the safe-area hero card scene and stores one selected HeroData.</summary>
    public sealed class HeroSelectController:MonoBehaviour
    {
        private readonly Dictionary<HeroData,Image> cards=new();private readonly Dictionary<HeroData,GameObject> lockOverlays=new();private HeroData selected;private Text selectionText;
        private void Start()
        {
            if(SceneLoader.Instance==null)new GameObject("SceneLoader",typeof(SceneLoader));if(HeroSelectionService.Instance==null)new GameObject("HeroSelectionService",typeof(HeroSelectionService));if(SaveGameManager.Instance==null)new GameObject("SaveGameManager",typeof(SaveGameManager));
            var canvas=UiFactory.CreateCanvas();GameArtwork.AddMainMenuBackground(canvas);GameArtwork.AddReadabilityOverlay(canvas,.55f);var safe=new GameObject("SafeArea",typeof(RectTransform),typeof(SafeAreaController)).GetComponent<RectTransform>();safe.SetParent(canvas,false);UiFactory.Stretch(safe,Vector2.zero,Vector2.one);
            var title=UiFactory.Label(safe,"Title","영웅 선택",62,TextAnchor.MiddleCenter,UiFactory.Gold);title.rectTransform.anchorMin=new Vector2(.25f,.86f);title.rectTransform.anchorMax=new Vector2(.75f,.98f);title.rectTransform.offsetMin=title.rectTransform.offsetMax=Vector2.zero;
            var row=UiFactory.Horizontal(safe,"HeroCards",28);UiFactory.Stretch(row,new Vector2(.05f,.2f),new Vector2(.95f,.84f));HeroData[] heroes=RuntimeHeroCatalog.GetHeroes();for(int i=0;i<heroes.Length;i++)AddCard(row,heroes[i]);
            selectionText=UiFactory.Label(safe,"Selection","",28,TextAnchor.MiddleCenter,Color.white);selectionText.rectTransform.anchorMin=new Vector2(.3f,.11f);selectionText.rectTransform.anchorMax=new Vector2(.7f,.19f);selectionText.rectTransform.offsetMin=selectionText.rectTransform.offsetMax=Vector2.zero;
            var back=UiFactory.Button(safe,"Back","뒤로 가기",new Color(.3f,.32f,.4f));Place(back,new Vector2(.05f,.05f),new Vector2(.25f,.16f));back.onClick.AddListener(()=>SceneLoader.Instance.Load(SceneNames.MainMenu));
            var start=UiFactory.Button(safe,"StartBattle","다음: 스테이지",new Color(.12f,.48f,.32f));Place(start,new Vector2(.75f,.05f),new Vector2(.95f,.16f));start.onClick.AddListener(BeginStageSelection);
            Select(HeroSelectionService.Instance.GetSelectedOrDefault());
        }
        private void AddCard(Transform row,HeroData data)
        {
            var button=UiFactory.Button(row,data.HeroId,"",data.PlaceholderColor);button.GetComponent<LayoutElement>().preferredWidth=280;Image image=button.GetComponent<Image>();cards[data]=image;var unlocks=new HeroUnlockService(SaveGameManager.Instance);button.onClick.AddListener(()=>SelectOrUnlock(data,unlocks));
            Sprite poster=RuntimeArtworkCatalog.HeroPoster(data.HeroId);if(poster!=null){image.sprite=poster;image.color=Color.white;image.preserveAspect=true;}
            var portrait=UiFactory.Panel(button.transform,"Portrait",Color.Lerp(data.PlaceholderColor,Color.white,.15f),new Vector2(.12f,.52f),new Vector2(.88f,.94f));if(poster!=null)portrait.gameObject.SetActive(false);else if(data.Portrait!=null){Image portraitImage=portrait.GetComponent<Image>();portraitImage.sprite=data.Portrait;portraitImage.color=Color.white;portraitImage.preserveAspect=true;}
            var mastery=SaveGameManager.Instance!=null?SaveRecords.Hero(SaveGameManager.Instance.Data,data.HeroId):null;string masteryText=mastery!=null?$"숙련 Lv.{mastery.masteryLevel} ({mastery.masteryXp} XP)":"숙련 Lv.1";
            string text=$"{data.DisplayName}\n{data.RoleName}\n{masteryText}\nHP {data.MaxHealth:0}  ATK {data.AttackDamage:0}\n스킬: {data.ActiveSkill.DisplayName}\n궁극기: {data.UltimateSkill.DisplayName}";var label=UiFactory.Label(button.transform,"Details",text,20,TextAnchor.MiddleCenter,Color.white);label.raycastTarget=false;label.rectTransform.anchorMin=new Vector2(.05f,.02f);label.rectTransform.anchorMax=new Vector2(.95f,.56f);label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;if(poster!=null)label.gameObject.SetActive(false);
            var locked=UiFactory.Label(button.transform,"Locked","🔒 잠김\n"+unlocks.Requirement(data.HeroId),18,TextAnchor.MiddleCenter,new Color(1,.85f,.55f));locked.raycastTarget=false;locked.rectTransform.anchorMin=new Vector2(.02f,.3f);locked.rectTransform.anchorMax=new Vector2(.98f,.7f);locked.rectTransform.offsetMin=locked.rectTransform.offsetMax=Vector2.zero;locked.gameObject.SetActive(!unlocks.IsUnlocked(data.HeroId));lockOverlays[data]=locked.gameObject;
        }
        private void SelectOrUnlock(HeroData data,HeroUnlockService unlocks){if(!unlocks.IsUnlocked(data.HeroId)&&!unlocks.TryUnlock(data.HeroId)){selectionText.text="잠김: "+unlocks.Requirement(data.HeroId);RefreshCardVisuals();return;}if(lockOverlays.TryGetValue(data,out GameObject overlay))overlay.SetActive(false);Select(data);}
        private void Select(HeroData data){if(data==null||SaveGameManager.Instance!=null&&!new HeroUnlockService(SaveGameManager.Instance).IsUnlocked(data.HeroId))return;selected=data;CollectionService.Record(data.HeroId,CollectionEvent.Used);RefreshCardVisuals();if(selectionText!=null)selectionText.text=$"선택: {data.DisplayName}";HeroSelectionService.Instance.Select(data);}
        private void RefreshCardVisuals(){foreach(var pair in cards){bool unlocked=SaveGameManager.Instance==null||new HeroUnlockService(SaveGameManager.Instance).IsUnlocked(pair.Key.HeroId);if(lockOverlays.TryGetValue(pair.Key,out GameObject overlay))overlay.SetActive(!unlocked);bool hasPoster=pair.Value.sprite!=null;pair.Value.color=hasPoster?(pair.Key==selected?new Color(1f,.84f,.48f):Color.white):(pair.Key==selected?Color.Lerp(pair.Key.PlaceholderColor,Color.white,.38f):pair.Key.PlaceholderColor);}}
        private void BeginStageSelection(){if(selected==null||SaveGameManager.Instance!=null&&!new HeroUnlockService(SaveGameManager.Instance).IsUnlocked(selected.HeroId)){if(selectionText!=null)selectionText.text="해금된 영웅을 선택하세요.";return;}HeroSelectionService.Instance.Select(selected);SceneLoader.Instance.Load(SceneNames.StageSelect);}
        private static void Place(Button button,Vector2 min,Vector2 max){var rect=button.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
