using HeroDefense.Core;
using HeroDefense.Save;
using HeroDefense.UI.Meta;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Build;

namespace HeroDefense.UI
{
    /// <summary>Phase 9 main menu with persistent profile, currencies and meta navigation.</summary>
    public sealed class MainMenuController:MonoBehaviour
    {
        private SettingsPanelController settings;
        private void Start()
        {
            if(SceneLoader.Instance==null)new GameObject("SceneLoader",typeof(SceneLoader));if(SaveGameManager.Instance==null)new GameObject("SaveGameManager",typeof(SaveGameManager));
            var canvas=UiFactory.CreateCanvas();GameArtwork.AddMainMenuBackground(canvas);GameArtwork.AddReadabilityOverlay(canvas,.18f);
            var safe=new GameObject("SafeArea",typeof(RectTransform),typeof(SafeAreaController)).GetComponent<RectTransform>();safe.SetParent(canvas,false);UiFactory.Stretch(safe,Vector2.zero,Vector2.one);
            var frame=UiFactory.Panel(safe,"MenuFrame",new Color(.025f,.055f,.075f,.78f),new Vector2(.315f,.015f),new Vector2(.685f,.985f));var frameOutline=frame.gameObject.AddComponent<Outline>();frameOutline.effectColor=new Color(1f,.72f,.25f,.5f);frameOutline.effectDistance=new Vector2(2,-2);var frameShadow=frame.gameObject.AddComponent<Shadow>();frameShadow.effectColor=new Color(0,0,0,.65f);frameShadow.effectDistance=new Vector2(7,-7);
            var column=UiFactory.Vertical(frame,"Menu",8);UiFactory.Stretch(column,new Vector2(.055f,.025f),new Vector2(.945f,.975f));
            var title=UiFactory.Label(column,"Title","✦ HERO DEFENSE ✦",48,TextAnchor.MiddleCenter,UiFactory.Gold);title.fontStyle=FontStyle.Bold;title.gameObject.AddComponent<LayoutElement>().preferredHeight=76;
            var data=SaveGameManager.Instance.Data;var profile=UiFactory.Label(column,"Profile",$"♛ {data.profile.displayName}   ·   COIN {data.currencies.coin}   ·   GEM {data.currencies.soulGem}",20,TextAnchor.MiddleCenter,new Color(.92f,.96f,1f));profile.gameObject.AddComponent<LayoutElement>().preferredHeight=36;
            var research=MetaMenuPanels.Research(safe);var collection=MetaMenuPanels.Collection(safe);var achievements=MetaMenuPanels.Achievements(safe);var reset=MetaMenuPanels.ResetConfirmation(safe);
            MenuButton(column,"Start","▶","게임 시작",new Color(.08f,.5f,.4f)).onClick.AddListener(()=>SceneLoader.Instance.Load(SceneNames.HeroSelect));
            MenuButton(column,"Research","✦","연구",new Color(.13f,.42f,.34f)).onClick.AddListener(()=>research.SetActive(true));
            MenuButton(column,"Collection","◆","도감",new Color(.32f,.27f,.52f)).onClick.AddListener(()=>collection.SetActive(true));
            MenuButton(column,"Achievements","★","업적",new Color(.55f,.35f,.16f)).onClick.AddListener(()=>achievements.SetActive(true));
            var settingsButton=MenuButton(column,"Settings","⚙","설정",new Color(.2f,.38f,.6f));
            MenuButton(column,"ResetData","↻","데이터 초기화",new Color(.52f,.22f,.2f)).onClick.AddListener(()=>reset.SetActive(true));
            MenuButton(column,"Quit","×","게임 종료",new Color(.6f,.2f,.22f)).onClick.AddListener(ApplicationQuitService.Quit);
            var version=UiFactory.Label(column,"Version",GameVersionService.DisplayVersion+"  •  Save v"+GameSaveData.CurrentVersion,17,TextAnchor.MiddleCenter,new Color(1,.9f,.65f,.82f));version.gameObject.AddComponent<LayoutElement>().preferredHeight=30;
            settings=new SettingsPanelController(safe);settingsButton.onClick.AddListener(()=>settings.Root.SetActive(true));
        }

        private static Button MenuButton(Transform parent,string name,string icon,string caption,Color color)
        {
            Button button=UiFactory.Button(parent,name,$"{icon}     {caption}",color);var layout=button.GetComponent<LayoutElement>();layout.preferredHeight=78;layout.minHeight=64;
            var image=button.GetComponent<Image>();var outline=button.gameObject.AddComponent<Outline>();outline.effectColor=Color.Lerp(color,UiFactory.Gold,.65f);outline.effectDistance=new Vector2(2,-2);var shadow=button.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.55f);shadow.effectDistance=new Vector2(4,-4);
            RectTransform accent=UiFactory.Panel(button.transform,"Accent",Color.Lerp(color,Color.white,.45f),new Vector2(.015f,.15f),new Vector2(.035f,.85f));accent.GetComponent<Image>().raycastTarget=false;Text label=button.GetComponentInChildren<Text>();label.fontSize=29;label.fontStyle=FontStyle.Bold;return button;
        }
    }
}
