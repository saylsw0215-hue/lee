using System.IO;
using HeroDefense.Battle;
using HeroDefense.Core;
using HeroDefense.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace HeroDefense.Editor
{
    /// <summary>Idempotently creates and validates all Phase 1 project assets and settings.</summary>
    [InitializeOnLoad]
    public static class Phase1Setup
    {
        private static readonly string[] Folders = { "Assets/Art/Materials", "Assets/Art/Sprites", "Assets/Art/UI", "Assets/Audio", "Assets/Prefabs/Environment", "Assets/Prefabs/UI", "Assets/Scenes", "Assets/Scripts/Core", "Assets/Scripts/Input", "Assets/Scripts/UI", "Assets/Scripts/Battle", "Assets/Scripts/Editor", "Assets/ScriptableObjects", "Assets/Settings", "Assets/Tests/EditMode", "Assets/Tests/PlayMode" };
        private static bool autoQueued;

        static Phase1Setup()
        {
            if (!autoQueued) { autoQueued = true; EditorApplication.delayCall += AutoSetupIfRequired; }
        }

        private static void AutoSetupIfRequired()
        {
            if (!File.Exists(ScenePath(SceneNames.Boot)) || !File.Exists(ScenePath(SceneNames.MainMenu)) || !File.Exists(ScenePath(SceneNames.Battle))) Setup();
        }

        [MenuItem("Tools/Hero Defense/Setup Phase 1")]
        public static void Setup()
        {
            foreach (string folder in Folders) Directory.CreateDirectory(folder);
            CreateSceneIfMissing(SceneNames.Boot, typeof(GameBootstrap));
            CreateSceneIfMissing(SceneNames.MainMenu, typeof(MainMenuController));
            CreateSceneIfMissing(SceneNames.HeroSelect, typeof(HeroDefense.UI.Heroes.HeroSelectController));
            CreateSceneIfMissing(SceneNames.StageSelect, typeof(HeroDefense.UI.Stages.StageSelectController));
            CreateSceneIfMissing(SceneNames.Battle, typeof(BattleSceneController));
            ApplyBuildScenes(); ApplyPlayerSettings(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("Hero Defense Phase 1 setup complete. Existing scenes were preserved.");
        }

        private static void CreateSceneIfMissing(string sceneName, System.Type rootType)
        {
            string path = ScenePath(sceneName); if (File.Exists(path)) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(sceneName + "Root"); root.AddComponent(rootType);
            EditorSceneManager.SaveScene(scene, path);
        }

        public static void ApplyBuildScenes()
        {
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(ScenePath(SceneNames.Boot), true),
                new EditorBuildSettingsScene(ScenePath(SceneNames.MainMenu), true),
                new EditorBuildSettingsScene(ScenePath(SceneNames.HeroSelect), true),
                new EditorBuildSettingsScene(ScenePath(SceneNames.StageSelect), true),
                new EditorBuildSettingsScene(ScenePath(SceneNames.Battle), true)
            };
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Independent"; PlayerSettings.productName = "Hero Defense";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.resizableWindow = true; PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, 2);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.independent.herodefense");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "13.0";
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            Object[] settingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settingsAssets.Length > 0)
            {
                var serialized = new SerializedObject(settingsAssets[0]);
                SerializedProperty inputHandler = serialized.FindProperty("activeInputHandler");
                if (inputHandler != null) { inputHandler.intValue = 1; serialized.ApplyModifiedPropertiesWithoutUndo(); }
            }
        }

        public static string ScenePath(string name) => $"Assets/Scenes/{name}.unity";
    }
}
