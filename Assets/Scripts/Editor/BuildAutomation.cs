using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HeroDefense.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using HeroDefense.Build;

namespace HeroDefense.Editor
{
    /// <summary>Validated Editor-menu and command-line builds for desktop and Apple targets.</summary>
    public static class BuildAutomation
    {
        private const string BundleIdentifier = "com.independent.herodefense";
        private static readonly string[] RequiredScenes = { SceneNames.Boot, SceneNames.MainMenu, SceneNames.HeroSelect, SceneNames.StageSelect, SceneNames.Battle };
        private static readonly string[] TransientTestRunnerAssets = { "Assets/Resources/PerformanceTestRunInfo.json", "Assets/Resources/PerformanceTestRunSettings.json" };

        [MenuItem("Tools/Hero Defense/Build/macOS App")]
        public static void BuildMacOS()
        {
            RunBuild(BuildTarget.StandaloneOSX, "Builds/macOS/HeroDefense.app", ConfigureMacOS,
                output => Directory.Exists(output), "macOS .app bundle");
        }

        [MenuItem("Tools/Hero Defense/Build/WebGL for GitHub Pages")]
        public static void BuildWebGL()
        {
            RunBuild(BuildTarget.WebGL,"Builds/WebGL",ConfigureWebGL,
                output=>File.Exists(Path.Combine(output,"index.html")),"WebGL site");
        }

        [MenuItem("Tools/Hero Defense/Build/iOS Xcode Project")]
        public static void BuildIOS()
        {
            RunIOSBuild("Builds/iOS/HeroDefenseXcode", iOSSdkVersion.DeviceSDK);
        }

        [MenuItem("Tools/Hero Defense/Build/iOS Development")]
        public static void BuildIOSDevelopment()=>RunEnvironment(HeroDefenseBuildType.Development,()=>RunIOSBuild("Builds/iOS/Development/HeroDefenseXcode",iOSSdkVersion.DeviceSDK));
        [MenuItem("Tools/Hero Defense/Build/iOS Release Candidate")]
        public static void BuildIOSReleaseCandidate()=>RunEnvironment(HeroDefenseBuildType.ReleaseCandidate,()=>RunIOSBuild($"Builds/iOS/ReleaseCandidate/{GameVersionService.ReleaseArtifactName("iOS")}",iOSSdkVersion.DeviceSDK),true);
        [MenuItem("Tools/Hero Defense/Build/iOS Store Release")]
        public static void BuildIOSStoreRelease()=>RunEnvironment(HeroDefenseBuildType.StoreRelease,()=>RunIOSBuild($"Builds/iOS/StoreRelease/{GameVersionService.ReleaseArtifactName("iOS")}",iOSSdkVersion.DeviceSDK),true);

        [MenuItem("Tools/Hero Defense/Build/iOS and Open Xcode")]
        public static void BuildIOSAndOpenXcode()
        {
            BuildIOS();
            if (Application.isBatchMode) return;
            string project = Path.GetFullPath("Builds/iOS/HeroDefenseXcode/Unity-iPhone.xcodeproj");
            if (!Directory.Exists(project)) return;
            try { Process.Start(new ProcessStartInfo("open", $"\"{project}\"") { UseShellExecute = false }); }
            catch (Exception exception) { Debug.LogWarning($"Xcode project was generated, but could not be opened automatically: {exception.Message}"); }
        }

        [MenuItem("Tools/Hero Defense/Build/iOS Simulator Project")]
        public static void BuildIOSSimulator()
        {
            RunIOSBuild("Builds/iOSSimulator/HeroDefenseXcode", iOSSdkVersion.SimulatorSDK);
        }

        // Retained as optional compatibility builds; Phase 1 release targets are macOS and iOS.
        [MenuItem("Tools/Hero Defense/Build/Windows (Optional)")]
        public static void BuildWindows() => RunBuild(BuildTarget.StandaloneWindows64, "Builds/Windows/HeroDefense.exe", null, File.Exists, "Windows executable");

        [MenuItem("Tools/Hero Defense/Build/Android APK")]
        [MenuItem("Tools/Hero Defense/Build/Android Development APK")]
        public static void BuildAndroid(){bool previous=EditorUserBuildSettings.buildAppBundle;try{EditorUserBuildSettings.buildAppBundle=false;RunEnvironment(HeroDefenseBuildType.Development,()=>RunBuild(BuildTarget.Android,"Builds/Android/APK/HeroDefense-Development.apk",ConfigureAndroid,File.Exists,"Android development APK"));}finally{EditorUserBuildSettings.buildAppBundle=previous;}}
        [MenuItem("Tools/Hero Defense/Build/Android Release AAB")]
        public static void BuildAndroidReleaseAAB(){if(!PlayerSettings.Android.useCustomKeystore||string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName)||string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName)){string message="Android Release AAB requires a user-owned keystore and alias configured locally in Player Settings. Passwords are never stored by this project.";Debug.LogError(message);if(Application.isBatchMode)EditorApplication.Exit(1);return;}bool previous=EditorUserBuildSettings.buildAppBundle;try{EditorUserBuildSettings.buildAppBundle=true;RunEnvironment(HeroDefenseBuildType.StoreRelease,()=>RunBuild(BuildTarget.Android,$"Builds/Android/AAB/{GameVersionService.ReleaseArtifactName("Android")}.aab",ConfigureAndroid,File.Exists,"Android release AAB"),true);}finally{EditorUserBuildSettings.buildAppBundle=previous;}}
        [MenuItem("Tools/Hero Defense/Build/Release Candidate macOS")]
        public static void BuildReleaseCandidateMacOS()=>RunEnvironment(HeroDefenseBuildType.ReleaseCandidate,()=>RunBuild(BuildTarget.StandaloneOSX,"Builds/macOS/ReleaseCandidate/HeroDefense.app",ConfigureMacOS,Directory.Exists,"macOS Release Candidate"),true);
        [MenuItem("Tools/Hero Defense/Build/Release Candidate iOS")]
        public static void BuildReleaseCandidateIOS()=>BuildIOSReleaseCandidate();
        [MenuItem("Tools/Hero Defense/Build/Release Candidate Android")]
        public static void BuildReleaseCandidateAndroid()=>BuildAndroidReleaseAAB();

        private static void RunIOSBuild(string relativeOutput, iOSSdkVersion sdk)
        {
            iOSSdkVersion previousSdk = PlayerSettings.iOS.sdkVersion;
            try
            {
                PlayerSettings.iOS.sdkVersion = sdk;
                RunBuild(BuildTarget.iOS, relativeOutput, ConfigureIOS, IOSProjectExists, sdk == iOSSdkVersion.DeviceSDK ? "iOS Xcode project" : "iOS Simulator Xcode project");
            }
            finally { PlayerSettings.iOS.sdkVersion = previousSdk; }
        }

        private static void RunBuild(BuildTarget target, string relativeOutput, Action configure, Func<string, bool> artifactExists, string artifactName)
        {
            try
            {
                Phase1Setup.Setup();
                Phase5Setup.Setup();
                Phase6Setup.Setup();
                Phase8Setup.Setup();
                Phase9Setup.Setup();
                RemoveTransientTestRunnerAssets();
                configure?.Invoke();
                Validate(target);
                string output = Path.GetFullPath(relativeOutput);
                string directory = target == BuildTarget.iOS ? output : Path.GetDirectoryName(output);
                if (string.IsNullOrWhiteSpace(directory)) throw new BuildFailedException("The build output path is invalid.");
                Directory.CreateDirectory(directory);
                var options = new BuildPlayerOptions { scenes = RequiredScenes.Select(Phase1Setup.ScenePath).ToArray(), locationPathName = output, target = target, options = BuildOptions.None };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException($"{target} build failed with {report.summary.totalErrors} error(s). Result: {report.summary.result}. See the Editor or batch log.");
                if (!artifactExists(output))
                    throw new BuildFailedException($"Unity reported success, but the expected {artifactName} was not found at: {output}");
                Debug.Log($"{artifactName} build succeeded: {output}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Hero Defense {artifactName} build failed: {exception.Message}\n{exception}");
                if (Application.isBatchMode) EditorApplication.Exit(1); else throw;
            }
        }

        private static void RemoveTransientTestRunnerAssets()
        {
            // Performance Test Framework creates these exact temporary Resources assets after a test run.
            // If left imported, its build hook includes test/NUnit assemblies in a normal Player and TypeDB reports duplicate types.
            for (int i = 0; i < TransientTestRunnerAssets.Length; i++)
            {
                string assetPath = TransientTestRunnerAssets[i];
                if (File.Exists(assetPath) || File.Exists(assetPath + ".meta"))
                {
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }

            AssetDatabase.Refresh();
        }

        private static void ConfigureMacOS()
        {
            PlayerSettings.productName = "Hero Defense";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.resizableWindow = true; PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            // 2 is Unity's macOS Universal (Intel 64-bit + Apple Silicon) architecture value.
            PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, 2);
        }

        private static void ConfigureWebGL()
        {
            PlayerSettings.productName="Hero Defense";
            PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
            // GitHub Pages cannot attach Unity's gzip/brotli response headers, so publish uncompressed files.
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback=false;
        }

        private static void ConfigureIOS()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleIdentifier);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1); // ARM64
            PlayerSettings.iOS.targetOSVersionString = "13.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
        }

        private static void ConfigureAndroid(){PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,BundleIdentifier);PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.Android.forceInternetPermission=false;PlayerSettings.Android.forceSDCardPermission=false;}

        private static void RunEnvironment(HeroDefenseBuildType type,Action build,bool validateRelease=false)
        {
            Phase10Setup.Setup();var environment=AssetDatabase.LoadAssetAtPath<BuildEnvironmentData>(Phase10Setup.EnvironmentPath);if(environment==null)throw new BuildFailedException("BuildEnvironment asset is missing.");HeroDefenseBuildType oldType=environment.BuildType;string identifier=environment.BundleIdentifier,product=environment.ProductName,version=environment.Version;int number=environment.BuildNumber;if(type>=HeroDefenseBuildType.ReleaseCandidate)number++;
            try{environment.Configure(type,identifier,product,version,number);EditorUtility.SetDirty(environment);AssetDatabase.SaveAssets();BuildEnvironmentService.ClearCache();Phase10Setup.ApplyPlayerSettings(environment);if(validateRelease){ReleaseValidationResult validation=ReleaseValidationService.Validate(environment);if(!validation.IsValid)throw new BuildFailedException(validation.ToString());}build();}finally{environment=AssetDatabase.LoadAssetAtPath<BuildEnvironmentData>(Phase10Setup.EnvironmentPath);if(environment!=null){environment.Configure(oldType,identifier,product,version,number);EditorUtility.SetDirty(environment);AssetDatabase.SaveAssets();Phase10Setup.ApplyPlayerSettings(environment);}BuildEnvironmentService.ClearCache();}
        }

        private static void Validate(BuildTarget target)
        {
            if (EditorUtility.scriptCompilationFailed) throw new BuildFailedException("Scripts contain compilation errors. Fix the Console errors before building.");
            string[] expected = RequiredScenes.Select(Phase1Setup.ScenePath).ToArray();
            foreach (string path in expected) if (!File.Exists(path)) throw new BuildFailedException($"Required scene is missing: {path}");
            string[] enabled = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (!enabled.SequenceEqual(expected)) throw new BuildFailedException("Enabled Build Settings scenes must be ordered exactly: Boot, MainMenu, HeroSelect, StageSelect, Battle.");
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(group, target))
                throw new BuildFailedException($"Unity build support for {target} is not installed. Add the matching module in Unity Hub.");
            if (PlayerSettings.productName != "Hero Defense") throw new BuildFailedException("Product Name must be 'Hero Defense'.");
            if (target == BuildTarget.iOS)
            {
                if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS) != BundleIdentifier) throw new BuildFailedException("The iOS Bundle Identifier is invalid.");
                if (PlayerSettings.allowedAutorotateToPortrait || PlayerSettings.allowedAutorotateToPortraitUpsideDown || !PlayerSettings.allowedAutorotateToLandscapeLeft || !PlayerSettings.allowedAutorotateToLandscapeRight)
                    throw new BuildFailedException("iOS orientation must allow Landscape Left/Right only.");
            }
        }

        private static bool IOSProjectExists(string output)
        {
            return Directory.Exists(Path.Combine(output, "Unity-iPhone.xcodeproj")) || Directory.Exists(Path.Combine(output, "Unity-iPhone.xcworkspace"));
        }
    }
}
