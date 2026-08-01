using System;using System.Collections.Generic;using System.IO;using System.Linq;using HeroDefense.Build;using HeroDefense.Core;using HeroDefense.Save;using UnityEditor;using UnityEditor.Build;using UnityEngine;

namespace HeroDefense.Editor
{
    public enum ReleaseIssueSeverity{Warning,Blocker}
    public readonly struct ReleaseIssue{public readonly ReleaseIssueSeverity Severity;public readonly string Message;public ReleaseIssue(ReleaseIssueSeverity severity,string message){Severity=severity;Message=message;}}
    public sealed class ReleaseValidationResult{public readonly List<ReleaseIssue>Issues=new();public int Blockers=>Issues.Count(x=>x.Severity==ReleaseIssueSeverity.Blocker);public int Warnings=>Issues.Count-Blockers;public bool IsValid=>Blockers==0;public void Add(ReleaseIssueSeverity severity,string message)=>Issues.Add(new ReleaseIssue(severity,message));public override string ToString()=> $"Release validation: {Blockers} blocker(s), {Warnings} warning(s)";}

    public static class ReleaseValidationService
    {
        public static readonly string[] RequiredDocuments={"Documentation/QA_CHECKLIST.md","Documentation/RELEASE_CHECKLIST.md","Documentation/KNOWN_ISSUES.md","Documentation/STORE_LISTING_KO.md","Documentation/STORE_LISTING_EN.md","Documentation/PRIVACY_POLICY_DRAFT.md","Documentation/TERMS_DRAFT.md","Documentation/THIRD_PARTY_NOTICES.md","Documentation/ASSET_LICENSES.md"};
        public static ReleaseValidationResult Validate(BuildEnvironmentData environment=null)
        {
            var result=new ReleaseValidationResult();environment??=BuildEnvironmentService.Current;if(environment==null)result.Add(ReleaseIssueSeverity.Blocker,"BuildEnvironment asset is missing.");else if(!environment.Validate(out string error))result.Add(ReleaseIssueSeverity.Blocker,error);
            string[] expected=new[]{SceneNames.Boot,SceneNames.MainMenu,SceneNames.HeroSelect,SceneNames.StageSelect,SceneNames.Battle}.Select(Phase1Setup.ScenePath).ToArray();string[] enabled=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray();if(!enabled.SequenceEqual(expected))result.Add(ReleaseIssueSeverity.Blocker,"Build scene order is invalid.");
            var ids=new HashSet<string>();foreach(var hero in GameContentDatabase.Heroes)if(!ids.Add(hero.HeroId))result.Add(ReleaseIssueSeverity.Blocker,"Duplicate content ID: "+hero.HeroId);foreach(var stage in GameContentDatabase.Stages)if(!ids.Add(stage.StageId))result.Add(ReleaseIssueSeverity.Blocker,"Duplicate content ID: "+stage.StageId);foreach(var unit in GameContentDatabase.Units)if(!ids.Add(unit.UnitId))result.Add(ReleaseIssueSeverity.Blocker,"Duplicate content ID: "+unit.UnitId);foreach(var building in GameContentDatabase.Buildings)if(!ids.Add(building.BuildingId))result.Add(ReleaseIssueSeverity.Blocker,"Duplicate content ID: "+building.BuildingId);
            foreach(string path in RequiredDocuments)if(!File.Exists(path))result.Add(ReleaseIssueSeverity.Warning,"Required release document is missing: "+path);if(!File.Exists("Assets/Art/Branding/AppIcons/HeroDefenseIcon.png"))result.Add(ReleaseIssueSeverity.Warning,"App icon is missing or still a placeholder.");if(environment!=null&&environment.BuildType>=HeroDefenseBuildType.ReleaseCandidate&&(environment.EnableDebugMenu||environment.EnableCheats))result.Add(ReleaseIssueSeverity.Blocker,"Release build enables development UI or cheats.");
            if(PlayerSettings.allowedAutorotateToPortrait||PlayerSettings.allowedAutorotateToPortraitUpsideDown||!PlayerSettings.allowedAutorotateToLandscapeLeft||!PlayerSettings.allowedAutorotateToLandscapeRight)result.Add(ReleaseIssueSeverity.Blocker,"Landscape orientation settings are invalid.");if(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS).Split('.').Length<3)result.Add(ReleaseIssueSeverity.Blocker,"iOS bundle identifier is invalid.");if(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android).Split('.').Length<3)result.Add(ReleaseIssueSeverity.Blocker,"Android package name is invalid.");
            var sensitive=new List<string>();foreach(string root in new[]{"Assets","ProjectSettings"})if(Directory.Exists(root))sensitive.AddRange(Directory.GetFiles(root,"*",SearchOption.AllDirectories).Where(p=>p.EndsWith(".keystore")||p.EndsWith(".jks")||p.EndsWith(".p12")||p.EndsWith(".mobileprovision")));if(sensitive.Count>0)result.Add(ReleaseIssueSeverity.Blocker,"Sensitive signing files exist inside the project.");return result;
        }
        [MenuItem("Tools/Hero Defense/Release/Validate Release")]public static void ValidateMenu(){var result=Validate();foreach(var issue in result.Issues){if(issue.Severity==ReleaseIssueSeverity.Blocker)Debug.LogError(issue.Message);else Debug.LogWarning(issue.Message);}Debug.Log(result);if(!result.IsValid)throw new BuildFailedException(result.ToString());}
    }

    public static class Phase10Setup
    {
        public const string EnvironmentPath="Assets/Resources/Build/BuildEnvironment.asset";
        [MenuItem("Tools/Hero Defense/Setup Phase 10")]
        public static void Setup()
        {
            Phase9Setup.Setup();string[] folders={"Assets/Art/Branding/AppIcons","Assets/Art/Branding/Logos","Assets/Art/Branding/Splash","Assets/Art/Marketing","Assets/Resources/Build","Assets/Scripts/Build/Environments","Assets/Scripts/Build/Validation","Assets/Scripts/Performance","Assets/Scripts/Accessibility","Assets/Scripts/Logging","Assets/Scripts/QA","Documentation","Builds/TestResults","Builds/Logs"};foreach(string folder in folders)Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var environment=AssetDatabase.LoadAssetAtPath<BuildEnvironmentData>(EnvironmentPath);if(environment==null){environment=ScriptableObject.CreateInstance<BuildEnvironmentData>();environment.Configure(HeroDefenseBuildType.Development,"com.independent.herodefense","Hero Defense","1.0.0",1);AssetDatabase.CreateAsset(environment,EnvironmentPath);}ApplyPlayerSettings(environment);ConfigureBranding();EditorUtility.SetDirty(environment);AssetDatabase.SaveAssets();BuildEnvironmentService.ClearCache();Debug.Log("Hero Defense Phase 10 setup complete. "+ReleaseValidationService.Validate(environment));
        }
        public static void ApplyPlayerSettings(BuildEnvironmentData env){PlayerSettings.companyName="Independent";PlayerSettings.productName=env.ProductName;PlayerSettings.bundleVersion=env.Version;PlayerSettings.iOS.buildNumber=env.BuildNumber.ToString();PlayerSettings.Android.bundleVersionCode=env.BuildNumber;PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,env.BundleIdentifier);PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,env.BundleIdentifier);PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);PlayerSettings.stripEngineCode=true;}
        private static void ConfigureBranding(){const string path="Assets/Art/Branding/AppIcons/HeroDefenseIcon.png";var importer=AssetImporter.GetAtPath(path)as TextureImporter;if(importer!=null){importer.textureType=TextureImporterType.Sprite;importer.mipmapEnabled=false;importer.isReadable=false;importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();}Texture2D icon=AssetDatabase.LoadAssetAtPath<Texture2D>(path);if(icon!=null)PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{icon},IconKind.Any);PlayerSettings.SplashScreen.backgroundColor=new Color(.025f,.04f,.09f);PlayerSettings.SplashScreen.show=false;}
    }

    public static class BuildNumberProvider
    {
        public static int Next(BuildEnvironmentData environment){if(environment==null)throw new ArgumentNullException(nameof(environment));int next=Math.Max(1,environment.BuildNumber+1);environment.Configure(environment.BuildType,environment.BundleIdentifier,environment.ProductName,environment.Version,next);EditorUtility.SetDirty(environment);AssetDatabase.SaveAssets();return next;}
        [MenuItem("Tools/Hero Defense/Release/Increment Build Number")]public static void Increment(){var environment=AssetDatabase.LoadAssetAtPath<BuildEnvironmentData>(Phase10Setup.EnvironmentPath);int value=Next(environment);Phase10Setup.ApplyPlayerSettings(environment);Debug.Log("Build number incremented to "+value);}
    }

    public static class MarketingScreenshotController
    {public static bool Enabled=>EditorPrefs.GetBool("HeroDefense.ScreenshotMode",false);[MenuItem("Tools/Hero Defense/Marketing/Screenshot Mode")]public static void Toggle(){bool next=!Enabled;EditorPrefs.SetBool("HeroDefense.ScreenshotMode",next);string[] hidden={"PerformanceHUDCanvas","Phase2Debug"};for(int i=0;i<hidden.Length;i++){GameObject value=GameObject.Find(hidden[i]);if(value!=null)value.SetActive(!next);}Debug.Log($"Screenshot Mode: {(next?"ON":"OFF")}. Debug overlays {(next?"hidden":"restored")}; select the desired hero/wave with development controls, disable Gizmos, and capture an actual Game View preset.");}}

    public static class BalanceSimulationRunner
    {
        [MenuItem("Tools/Hero Defense/Balance/Run Stage Simulation")]
        public static void Run(){Directory.CreateDirectory("Builds/Balance");Debug.Log("Stage 1 balance now uses the real Battle scene. Run the explicit StageOneBalanceScenarioPlayModeTests fixture, or dispatch GitHub Actions with build_target=balance. JSON output is written to "+Path.GetFullPath("Builds/Balance"));}
    }
}
