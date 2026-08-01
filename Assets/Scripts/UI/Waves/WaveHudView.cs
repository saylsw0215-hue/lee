using HeroDefense.Battle.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.UI.Waves
{
    /// <summary>Read-only wave HUD, announcement, boss bar, and debug start control.</summary>
    public sealed class WaveHudView
    {
        public Button SkipButton{get;}public GameObject ResultPanel{get;}public Text ResultText{get;}
        private readonly Text status,announcement,bossName;private readonly Image bossFill;private readonly GameObject announcementPanel,bossPanel;
        public WaveHudView(RectTransform safe,System.Action skip,System.Action replay,System.Action menu)
        {
            var hud=UiFactory.Panel(safe,"WaveHUD",new Color(.02f,.04f,.08f,.9f),new Vector2(.21f,.79f),new Vector2(.79f,.855f));
            status=UiFactory.Label(hud,"WaveStatus","",27,TextAnchor.MiddleCenter,Color.white);status.rectTransform.anchorMax=new Vector2(.78f,1);
            SkipButton=UiFactory.Button(hud,"SkipPreparation","웨이브 즉시 시작",new Color(.48f,.26f,.1f));var sr=SkipButton.GetComponent<RectTransform>();sr.anchorMin=new Vector2(.79f,.08f);sr.anchorMax=new Vector2(.99f,.92f);sr.offsetMin=sr.offsetMax=Vector2.zero;SkipButton.onClick.AddListener(()=>skip());
            announcementPanel=UiFactory.Panel(safe,"WaveAnnouncement",new Color(0,0,0,.68f),new Vector2(.28f,.43f),new Vector2(.72f,.65f)).gameObject;
            announcement=UiFactory.Label(announcementPanel.transform,"Text","",58,TextAnchor.MiddleCenter,Color.white);announcementPanel.SetActive(false);
            bossPanel=UiFactory.Panel(safe,"BossHealth",new Color(.08f,.02f,.02f,.94f),new Vector2(.24f,.72f),new Vector2(.76f,.785f)).gameObject;
            bossName=UiFactory.Label(bossPanel.transform,"Name","고블린 대장",24,TextAnchor.MiddleLeft,Color.white);bossName.rectTransform.anchorMax=new Vector2(.3f,1);
            var bar=UiFactory.Panel(bossPanel.transform,"Bar",new Color(.16f,.05f,.05f),new Vector2(.31f,.2f),new Vector2(.98f,.8f));var fill=UiFactory.Panel(bar,"Fill",new Color(.86f,.12f,.08f),Vector2.zero,Vector2.one);bossFill=fill.GetComponent<Image>();bossFill.type=Image.Type.Filled;bossFill.fillMethod=Image.FillMethod.Horizontal;bossPanel.SetActive(false);
            ResultPanel=UiFactory.Panel(safe,"StageResult",new Color(0,0,0,.9f),new Vector2(.25f,.12f),new Vector2(.75f,.88f)).gameObject;
            var col=UiFactory.Vertical(ResultPanel.transform,"Column",14);UiFactory.Stretch(col,new Vector2(.08f,.05f),new Vector2(.92f,.95f));ResultText=UiFactory.Label(col,"Result","",36,TextAnchor.MiddleCenter,Color.white);ResultText.gameObject.AddComponent<LayoutElement>().preferredHeight=500;
            UiFactory.Button(col,"Replay","다시 플레이",new Color(.16f,.48f,.3f)).onClick.AddListener(()=>replay());UiFactory.Button(col,"Menu","메인 메뉴",new Color(.24f,.31f,.5f)).onClick.AddListener(()=>menu());ResultPanel.SetActive(false);
        }
        public void Refresh(int wave,int total,WaveRuntimeState runtime,bool boss)
        {
            string detail=runtime.State==WaveState.Preparing?$"다음 웨이브까지 {runtime.PreparationRemaining:0.0}초":$"남은 적 {runtime.AliveTracked+runtime.RemainingToSpawn}";
            status.text=$"Wave {wave} / {total} | {detail} | {runtime.State}"+(boss?" | BOSS WAVE":"");SkipButton.interactable=runtime.State==WaveState.Preparing;
        }
        public void ShowAnnouncement(string value,Color color){announcement.text=value;announcement.color=color;announcementPanel.SetActive(true);}
        public void HideAnnouncement()=>announcementPanel.SetActive(false);
        public void ShowBoss(string name,float current,float max){bossPanel.SetActive(true);bossName.text=name;bossFill.fillAmount=max<=0?0:Mathf.Clamp01(current/max);}
        public void HideBoss()=>bossPanel.SetActive(false);
        public void HideTransient(){announcementPanel.SetActive(false);bossPanel.SetActive(false);ResultPanel.SetActive(false);}
    }
}
