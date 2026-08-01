using HeroDefense.Core;
using HeroDefense.Input;
using HeroDefense.UI;
using HeroDefense.UI.Buildings;
using HeroDefense.Battle.Waves;
using HeroDefense.Heroes;
using HeroDefense.Progression;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using HeroDefense.Battle.Stages;
using HeroDefense.Save;
using HeroDefense.Meta;
using HeroDefense.Battle.Economy;
using HeroDefense.Battle.Effects;
using HeroDefense.Audio;
using HeroDefense.Build;

namespace HeroDefense.Battle
{
    /// <summary>Composes the Phase 1 battlefield and coordinates its focused controllers.</summary>
    public sealed class BattleSceneController : MonoBehaviour
    {
        private BattleSessionState state;
        private PauseController pause;
        private BattleHudController hud;
        private BuildingSelectionController buildings;
        private BuildingSystemController buildingSystem;
        private WaveManager waveManager;
        private HeroSpawnManager heroManager;
        private BattleProgressionController progression;
        private BackInputRouter backRouter;
        private BattleCombatController combat;
        private BuildingSelectionModel buildingModel;
        private GameObject pausePanel;
        private InputAction debugGold, debugWave, debugDamage;
        private ScreenShakeController screenShake;
        private GoldMineController goldMine;

        private void Start()
        {
            if (SceneLoader.Instance == null) new GameObject("SceneLoader", typeof(SceneLoader));
            if (SaveGameManager.Instance == null) new GameObject("SaveGameManager", typeof(SaveGameManager));
            StageData launchStage=BattleLaunchConfig.SelectedStage;DifficultyModifiers difficulty=DifficultyModifiers.For(BattleLaunchConfig.Difficulty);var meta=new MetaUpgradeService(SaveGameManager.Instance);int startGold=Mathf.RoundToInt(launchStage.StartingGold*difficulty.StartingGold)+Mathf.RoundToInt(meta.Effect("meta_starting_gold"));if(BattleLaunchConfig.Mode==GameMode.Endless)startGold+=Mathf.RoundToInt(meta.Effect("meta_endless_start_gold"));int baseHp=Mathf.RoundToInt(launchStage.BaseMaxHealth*difficulty.BaseHealth*(1+meta.Effect("meta_base_health")));state = new BattleSessionState(startGold,baseHp); pause = new PauseController();
            var canvas = UiFactory.CreateCanvas(); BuildField(canvas,launchStage);
            var safe = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaController)).GetComponent<RectTransform>(); safe.SetParent(canvas, false); UiFactory.Stretch(safe, Vector2.zero, Vector2.one);

            var combatWorld = new GameObject("CombatWorld", typeof(RectTransform)).GetComponent<RectTransform>(); combatWorld.SetParent(safe, false);
            UiFactory.Stretch(combatWorld, new Vector2(.08f,.31f), new Vector2(.92f,.82f));
            GameArtwork.AddBattleWorldDecorations(combatWorld);

            var top = UiFactory.Panel(safe, "TopHUD", new Color(.035f,.055f,.09f,.84f), new Vector2(.02f, .86f), new Vector2(.98f, .98f));
            var hudRow = UiFactory.Horizontal(top, "Values", 20); UiFactory.Stretch(hudRow, Vector2.zero, Vector2.one); hud = new BattleHudController(hudRow, state);

            var bottom = UiFactory.Panel(safe, "BottomBar", new Color(.035f,.055f,.09f,.86f), new Vector2(.02f, .02f), new Vector2(.98f, .17f));
            var buildingRow = UiFactory.Horizontal(bottom, "Buildings", 16); buildingRow.anchorMin = new Vector2(.015f,.1f); buildingRow.anchorMax = new Vector2(.56f,.9f); buildingRow.offsetMin = buildingRow.offsetMax = Vector2.zero;
            var selectionStatus = UiFactory.Panel(safe, "SelectionStatus", new Color(.06f,.08f,.11f,.76f), new Vector2(.31f,.18f), new Vector2(.69f,.235f));
            buildingModel = new BuildingSelectionModel();
            var actions = UiFactory.Horizontal(bottom, "Actions", 14); actions.anchorMin = new Vector2(.68f,.1f); actions.anchorMax = new Vector2(.985f,.9f); actions.offsetMin = actions.offsetMax = Vector2.zero;
            UiFactory.Button(actions, "Pause", "일시정지", new Color(.42f,.31f,.18f)).onClick.AddListener(pause.Pause);
            UiFactory.Button(actions, "Menu", "메뉴", new Color(.39f,.19f,.19f)).onClick.AddListener(() => { pause.Resume(); SceneLoader.Instance.Load(SceneNames.MainMenu); });
            BuildPause(safe);
            combat = new BattleCombatController(safe, combatWorld, state, pause, buildingModel);
            var mineObject=new GameObject("ContestedGoldMine",typeof(RectTransform),typeof(GoldMineController));goldMine=mineObject.GetComponent<GoldMineController>();goldMine.Initialize(combatWorld,state,pause,combat.Registry,()=>combat.IsStageEnded);combat.BattleReset+=goldMine.ResetMine;
            screenShake=gameObject.AddComponent<ScreenShakeController>();screenShake.Initialize(combatWorld);
            buildingSystem = new BuildingSystemController(safe,combatWorld,buildingRow,selectionStatus,state,pause,combat);
            var waveObject=new GameObject("WaveManager",typeof(WaveManager));waveObject.transform.SetParent(transform,false);waveManager=waveObject.GetComponent<WaveManager>();waveManager.Initialize(safe,state,pause,combat);
            waveManager.BossWaveStarted+=OnBossWave;
            var heroObject=new GameObject("HeroSpawnManager",typeof(HeroSpawnManager));heroObject.transform.SetParent(transform,false);heroManager=heroObject.GetComponent<HeroSpawnManager>();heroManager.Initialize(safe,state,pause,combat,waveManager.Statistics);waveManager.StageWon+=heroManager.OnVictory;waveManager.StageFailed+=heroManager.OnDefeat;
            bool enableProgression=!IsAutomatedTestRun();
#if UNITY_INCLUDE_TESTS
            enableProgression=false;
#endif
            if(enableProgression){var progressionObject=new GameObject("BattleProgression",typeof(BattleProgressionController));progressionObject.transform.SetParent(transform,false);progression=progressionObject.GetComponent<BattleProgressionController>();progression.Initialize(safe,combat,waveManager,heroManager.Hero,pause);}
            buildingSystem.BuildingInstalled+=waveManager.Statistics.RecordBuilding;buildingSystem.BuildingSold+=waveManager.Statistics.RecordSale;buildingSystem.BuildingUpgraded+=waveManager.Statistics.RecordUpgrade;
            pause.Changed += OnPauseChanged;
            backRouter = gameObject.AddComponent<BackInputRouter>(); backRouter.BackPressed += OnBack;
            SetupDebugInput();
        }

        private static void BuildField(Transform canvas,StageData stage)
        {
            GameArtwork.AddStageBackground(canvas,stage.StageId,"StageEnvironmentArtwork_"+stage.StageId);
            Color stageTint=stage.BackgroundColor;stageTint.a=.2f;UiFactory.Panel(canvas,"StageEnvironmentTint_"+stage.StageId,stageTint,Vector2.zero,Vector2.one).GetComponent<Image>().raycastTarget=false;
            var baseZone = UiFactory.Panel(canvas, "FriendlyBaseZone", new Color(.12f,.28f,.48f,.2f), new Vector2(0,.17f), new Vector2(.22f,.86f));var baseLabel=UiFactory.Label(baseZone,"Label","✦ 아군 본진 ✦",22,TextAnchor.UpperCenter,Color.white);UiFactory.Stretch(baseLabel.rectTransform,new Vector2(.08f,.82f),new Vector2(.92f,.97f));
            var build = UiFactory.Panel(canvas, "BuildZone", new Color(.2f,.55f,.45f,.12f), new Vector2(.22f,.17f), new Vector2(.40f,.86f));
            var combat = UiFactory.Panel(canvas, "CombatZone", new Color(.45f,.30f,.16f,.1f), new Vector2(.40f,.17f), new Vector2(.78f,.86f));
            var spawn = UiFactory.Panel(canvas, "MonsterSpawnZone", new Color(.48f,.12f,.16f,.2f), new Vector2(.78f,.17f), new Vector2(1,.86f));var spawnLabel=UiFactory.Label(spawn,"Label","⚔ 몬스터 포탈 ⚔",22,TextAnchor.UpperCenter,Color.white);UiFactory.Stretch(spawnLabel.rectTransform,new Vector2(.05f,.82f),new Vector2(.95f,.97f));
        }

        private void BuildPause(Transform safe)
        {
            pausePanel = UiFactory.Panel(safe,"PauseOverlay",new Color(0,0,0,.78f),Vector2.zero,Vector2.one).gameObject;
            var column = UiFactory.Vertical(pausePanel.transform,"PauseMenu",22); UiFactory.Stretch(column,new Vector2(.36f,.2f),new Vector2(.64f,.8f));
            var title = UiFactory.Label(column,"Title","일시정지",58,TextAnchor.MiddleCenter,Color.white); title.gameObject.AddComponent<LayoutElement>().preferredHeight=130;
            UiFactory.Button(column,"Resume","계속하기",new Color(.14f,.42f,.34f)).onClick.AddListener(pause.Resume);
            UiFactory.Button(column,"MainMenu","메인 메뉴",new Color(.23f,.31f,.49f)).onClick.AddListener(() => { pause.Resume(); SceneLoader.Instance.Load(SceneNames.MainMenu); });
            UiFactory.Button(column,"Quit","게임 종료",new Color(.48f,.18f,.18f)).onClick.AddListener(ApplicationQuitService.Quit);
            pausePanel.SetActive(false);
        }

        private void SetupDebugInput()
        {
            if(!BuildEnvironmentService.CheatsEnabled)return;
            debugGold = new InputAction("AddGold",binding:"<Keyboard>/g"); debugGold.performed += _ => state.AddGold(100);
            debugWave = new InputAction("Wave",binding:"<Keyboard>/w"); debugWave.performed += _ => state.AdvanceWave();
            debugDamage = new InputAction("Damage",binding:"<Keyboard>/h"); debugDamage.performed += _ => combat.DamageBaseForDebug(10);
            debugGold.Enable(); debugWave.Enable(); debugDamage.Enable();
        }
        private static bool IsAutomatedTestRun(){string[] args=System.Environment.GetCommandLineArgs();for(int i=0;i<args.Length;i++)if(args[i]=="-runTests")return true;return Application.isBatchMode;}
        private void OnBack() { if (combat!=null&&combat.IsStageEnded)return;if (!pause.IsPaused) pause.Pause(); else pause.Resume(); }
        private void OnApplicationPause(bool paused){if(!paused)return;heroManager?.Aiming?.Cancel();SaveGameManager.Instance?.SaveNow(SaveReason.ApplicationPaused);if(pause!=null&&combat!=null&&!combat.IsStageEnded&&!pause.IsPaused)pause.Pause();}
        private void OnApplicationFocus(bool focused){if(focused)return;heroManager?.Aiming?.Cancel();if(pause!=null&&combat!=null&&!combat.IsStageEnded&&!pause.IsPaused)pause.Pause();}
        private void OnPauseChanged(bool value) => pausePanel.SetActive(value&&!pause.HasReason(GamePauseReason.LevelUpSelection));
        private void OnBossWave(int wave){screenShake?.Play(.45f,14);Haptics.Current.Pulse();}
        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (pause != null) pause.Changed -= OnPauseChanged;
            if (backRouter != null) backRouter.BackPressed -= OnBack;
            if(buildingSystem!=null&&waveManager!=null){buildingSystem.BuildingInstalled-=waveManager.Statistics.RecordBuilding;buildingSystem.BuildingSold-=waveManager.Statistics.RecordSale;buildingSystem.BuildingUpgraded-=waveManager.Statistics.RecordUpgrade;}
            if(heroManager!=null&&waveManager!=null){waveManager.StageWon-=heroManager.OnVictory;waveManager.StageFailed-=heroManager.OnDefeat;}
            if(waveManager!=null)waveManager.BossWaveStarted-=OnBossWave;
            if(combat!=null&&goldMine!=null)combat.BattleReset-=goldMine.ResetMine;
            hud?.Dispose(); buildings?.Dispose(); buildingSystem?.Dispose(); combat?.Dispose();
            debugGold?.Dispose(); debugWave?.Dispose(); debugDamage?.Dispose();
        }
    }
}
