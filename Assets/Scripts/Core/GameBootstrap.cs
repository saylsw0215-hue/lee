using UnityEngine;
using HeroDefense.Heroes.Selection;
using HeroDefense.Save;
using HeroDefense.Achievements;
using HeroDefense.Tutorial;
using HeroDefense.Audio;
using HeroDefense.UI;
using HeroDefense.Accessibility;
using HeroDefense.Performance;
using HeroDefense.Build;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HeroDefense.Core
{
    /// <summary>Creates persistent core services and routes the boot scene.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static bool initialized;

        private void Awake()
        {
            if (initialized) { Destroy(gameObject); return; }
            initialized = true;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if(SceneManager.GetActiveScene().name==SceneNames.Boot)CreateBootView();
            if (SceneLoader.Instance == null) new GameObject("SceneLoader", typeof(SceneLoader));
            if (HeroSelectionService.Instance == null) new GameObject("HeroSelectionService", typeof(HeroSelectionService));
            if (SaveGameManager.Instance == null) new GameObject("SaveGameManager", typeof(SaveGameManager));
            if (FindAnyObjectByType<AchievementNotificationController>() == null) new GameObject("AchievementNotifications", typeof(AchievementNotificationController));
            if (FindAnyObjectByType<TutorialManager>() == null) new GameObject("TutorialManager", typeof(TutorialManager));
            if (AudioManager.Instance == null) new GameObject("AudioManager", typeof(AudioManager));
            if (FindAnyObjectByType<SystemMessageController>() == null) new GameObject("SystemMessages", typeof(SystemMessageController));
            if(SaveGameManager.Instance!=null&&SaveGameManager.Instance.LastLoadRecovered)SystemMessageController.Show("저장 데이터를 백업에서 복구했습니다.");
            if (FindAnyObjectByType<LargeUiController>() == null) new GameObject("Accessibility", typeof(LargeUiController));
            if (Build.BuildEnvironmentService.DebugUiEnabled&&FindAnyObjectByType<PerformanceMonitor>()==null)new GameObject("PerformanceMonitor",typeof(PerformanceMonitor));
        }

        private static void CreateBootView()
        {
            var canvas=UiFactory.CreateCanvas();canvas.name="BootSplash";UiFactory.Panel(canvas,"Background",new Color(.025f,.04f,.09f),Vector2.zero,Vector2.one);var title=UiFactory.Label(canvas,"Brand",BuildEnvironmentService.Current!=null?BuildEnvironmentService.Current.ProductName.ToUpperInvariant():"HERO DEFENSE",64,TextAnchor.MiddleCenter,UiFactory.Gold);title.rectTransform.anchorMin=new Vector2(.2f,.42f);title.rectTransform.anchorMax=new Vector2(.8f,.62f);title.rectTransform.offsetMin=title.rectTransform.offsetMax=Vector2.zero;var status=UiFactory.Label(canvas,"Status","콘텐츠 준비 중",24,TextAnchor.MiddleCenter,new Color(1,1,1,.72f));status.rectTransform.anchorMin=new Vector2(.3f,.34f);status.rectTransform.anchorMax=new Vector2(.7f,.42f);status.rectTransform.offsetMin=status.rectTransform.offsetMax=Vector2.zero;
        }

        private void Start()
        {
            string[] arguments = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == "-heroDefenseSmokeBattle")
                {
                    SceneLoader.Instance.Load(SceneNames.Battle);
                    return;
                }
                if (arguments[i] == "-heroDefenseSmokeHeroSelect")
                {
                    SceneLoader.Instance.Load(SceneNames.HeroSelect);
                    return;
                }
                if (arguments[i] == "-heroDefenseSmokeSelectedBattle")
                {
                    HeroSelectionService.Instance.Select(RuntimeHeroCatalog.GetHeroes()[2]);
                    SceneLoader.Instance.Load(SceneNames.Battle);
                    return;
                }
            }
            SceneLoader.Instance.Load(SceneNames.MainMenu);
        }
    }
}
