using HeroDefense.Battle.Stages;
using HeroDefense.Core;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Save;
using HeroDefense.Meta;
using HeroDefense.Collection;

namespace HeroDefense.UI.Stages
{
    /// <summary>Safe-area stage, difficulty and mode selection using the runtime content registry.</summary>
    public sealed class StageSelectController:MonoBehaviour
    {
        private StageData selected;private GameDifficulty difficulty=GameDifficulty.Normal;private GameMode mode=GameMode.Stage;private Text details;private Button start;
        private void Start()
        {
            if(SceneLoader.Instance==null)new GameObject("SceneLoader",typeof(SceneLoader));if(SaveGameManager.Instance==null)new GameObject("SaveGameManager",typeof(SaveGameManager));var canvas=UiFactory.CreateCanvas();GameArtwork.AddMainMenuBackground(canvas);GameArtwork.AddReadabilityOverlay(canvas,.48f);var safe=new GameObject("SafeArea",typeof(RectTransform),typeof(SafeAreaController)).GetComponent<RectTransform>();safe.SetParent(canvas,false);UiFactory.Stretch(safe,Vector2.zero,Vector2.one);
            var title=UiFactory.Label(safe,"Title","스테이지 선택",52,TextAnchor.MiddleCenter,UiFactory.Gold);Place(title.rectTransform,new(.3f,.89f),new(.7f,.98f));
            var stages=UiFactory.Horizontal(safe,"StageCards",16);UiFactory.Stretch(stages,new(.03f,.5f),new(.97f,.87f));foreach(StageData stage in RuntimeStageCatalog.GetAll()){StageData captured=stage;bool unlocked=SaveRecords.Stage(SaveGameManager.Instance.Data,stage.StageId)?.unlocked==true;var button=UiFactory.Button(stages,stage.StageId,unlocked?$"{stage.DisplayName}\n{stage.WaveCount} Waves\n{BossName(stage)}":"🔒 잠김\n이전 스테이지 보통 클리어",unlocked?stage.BackgroundColor:new Color(.15f,.15f,.18f));button.GetComponent<LayoutElement>().preferredWidth=370;button.interactable=unlocked;button.onClick.AddListener(()=>Select(captured));}
            details=UiFactory.Label(safe,"Details","",26,TextAnchor.MiddleCenter,Color.white);Place(details.rectTransform,new(.12f,.27f),new(.88f,.48f));
            var difficulties=UiFactory.Horizontal(safe,"Difficulties",12);UiFactory.Stretch(difficulties,new(.25f,.17f),new(.75f,.26f));AddDifficulty(difficulties,"쉬움",GameDifficulty.Easy);AddDifficulty(difficulties,"보통",GameDifficulty.Normal);AddDifficulty(difficulties,"어려움",GameDifficulty.Hard);
            var bottom=UiFactory.Horizontal(safe,"Bottom",15);UiFactory.Stretch(bottom,new(.12f,.04f),new(.88f,.15f));UiFactory.Button(bottom,"Back","영웅 선택",new Color(.3f,.32f,.4f)).onClick.AddListener(()=>SceneLoader.Instance.Load(SceneNames.HeroSelect));UiFactory.Button(bottom,"Endless","끝없는 방어",new Color(.42f,.18f,.5f)).onClick.AddListener(()=>{if(SaveRecords.Stage(SaveGameManager.Instance.Data,"stage_01_grassland")?.normalCleared!=true){details.text="끝없는 방어: 초원의 관문 보통 클리어 필요";return;}mode=GameMode.Endless;Refresh();});start=UiFactory.Button(bottom,"Start","전투 시작",new Color(.12f,.48f,.32f));start.onClick.AddListener(Begin);Select(RuntimeStageCatalog.GetStageOne());
        }
        private void AddDifficulty(Transform row,string label,GameDifficulty value)=>UiFactory.Button(row,value.ToString(),label,new Color(.2f,.34f,.5f)).onClick.AddListener(()=>{if(value==GameDifficulty.Hard&&selected!=null&&SaveRecords.Stage(SaveGameManager.Instance.Data,selected.StageId)?.normalCleared!=true){details.text="어려움 난이도: 해당 스테이지 보통 클리어 필요";return;}difficulty=value;Refresh();});
        private void Select(StageData value){selected=value;mode=GameMode.Stage;CollectionService.Record(value.StageId,CollectionEvent.Encountered);Refresh();}
        private void Refresh(){if(selected==null)return;var m=DifficultyModifiers.For(difficulty);details.text=$"{selected.DisplayName}\n{selected.Description}\n웨이브 {selected.WaveCount} | 난이도 {DifficultyName()} | 모드 {(mode==GameMode.Endless?"끝없는 방어":"스테이지")}\n적 HP x{m.EnemyHealth:0.00} / 공격 x{m.EnemyDamage:0.00}";}
        private void Begin(){if(selected==null)return;BattleLaunchConfig.Configure(selected.StageId,difficulty,mode);SceneLoader.Instance.Load(SceneNames.Battle);}
        private string DifficultyName()=>difficulty==GameDifficulty.Easy?"쉬움":difficulty==GameDifficulty.Hard?"어려움":"보통";
        private static string BossName(StageData stage){var waves=stage.Waves;if(waves==null||waves.Length==0)return "";var groups=waves[waves.Length-1].SpawnGroups;return groups!=null&&groups.Length>0&&groups[0].EnemyData!=null?groups[0].EnemyData.DisplayName:"보스";}
        private static void Place(RectTransform rect,Vector2 min,Vector2 max){rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}
