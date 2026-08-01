using System.IO;using HeroDefense.Build;using UnityEditor;using UnityEditor.Build;using UnityEditor.Build.Reporting;using UnityEditor.Callbacks;using UnityEngine;
namespace HeroDefense.Editor
{
    /// <summary>Validates mobile output without injecting teams, certificates, capabilities, or permissions.</summary>
    public sealed class MobileBuildValidator:IPreprocessBuildWithReport
    {
        public int callbackOrder=>-100;public void OnPreprocessBuild(BuildReport report){if(report.summary.platform!=BuildTarget.iOS&&report.summary.platform!=BuildTarget.Android)return;var env=BuildEnvironmentService.Current;if(env==null)throw new BuildFailedException("Build environment is missing.");if(!env.Validate(out string error))throw new BuildFailedException(error);if(PlayerSettings.allowedAutorotateToPortrait||PlayerSettings.allowedAutorotateToPortraitUpsideDown)throw new BuildFailedException("Portrait orientation must be disabled.");if(env.BuildType>=HeroDefenseBuildType.ReleaseCandidate){var result=ReleaseValidationService.Validate(env);if(!result.IsValid)throw new BuildFailedException(result.ToString());}}
    }
    public static class IOSBuildPostProcessor
    {
        [PostProcessBuild(100)]public static void Validate(BuildTarget target,string path){if(target!=BuildTarget.iOS)return;string project=Path.Combine(path,"Unity-iPhone.xcodeproj");string plist=Path.Combine(path,"Info.plist");if(!Directory.Exists(project))Debug.LogError("Generated iOS project is missing Unity-iPhone.xcodeproj.");if(!File.Exists(plist))Debug.LogWarning("Generated iOS project Info.plist was not found for permission review.");else{string text=File.ReadAllText(plist);string[] forbidden={"NSCameraUsageDescription","NSMicrophoneUsageDescription","NSLocationWhenInUseUsageDescription","NSUserTrackingUsageDescription"};for(int i=0;i<forbidden.Length;i++)if(text.Contains(forbidden[i]))Debug.LogWarning("Review unused iOS permission: "+forbidden[i]);}Debug.Log("iOS project ready for manual Signing & Capabilities review: "+Path.GetFullPath(path));}
    }
}
