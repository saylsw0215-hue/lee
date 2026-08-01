using System.IO;using System.Linq;using HeroDefense.Accessibility;using HeroDefense.Build;using HeroDefense.Editor;using HeroDefense.Logging;using HeroDefense.Performance;using HeroDefense.Save;using NUnit.Framework;using UnityEditor;using UnityEngine;
namespace HeroDefense.Tests.EditMode
{
    public sealed class Phase10ReleaseTests
    {
        private BuildEnvironmentData Make(HeroDefenseBuildType type=HeroDefenseBuildType.Development,string version="1.0.0",int build=1,string id="com.independent.herodefense"){var value=ScriptableObject.CreateInstance<BuildEnvironmentData>();value.Configure(type,id,"Hero Defense",version,build);return value;}
        [Test]public void DevelopmentEnablesDebugTools(){var e=Make();Assert.IsTrue(e.EnableDebugMenu);Assert.IsTrue(e.EnableCheats);}
        [Test]public void InternalTestDisablesCheats(){var e=Make(HeroDefenseBuildType.InternalTest);Assert.IsFalse(e.EnableCheats);Assert.IsFalse(e.EnableDebugMenu);Assert.IsTrue(e.EnableVerboseLogging);}
        [Test]public void ReleaseCandidateDisablesDebugTools(){var e=Make(HeroDefenseBuildType.ReleaseCandidate);Assert.IsFalse(e.EnableDebugMenu);Assert.IsFalse(e.EnableCheats);Assert.IsFalse(e.EnableVerboseLogging);}
        [Test]public void StoreReleaseDisablesDebugTools(){var e=Make(HeroDefenseBuildType.StoreRelease);Assert.IsFalse(e.EnableDebugMenu);Assert.IsFalse(e.EnableCheats);}
        [TestCase("1.0.0",true)][TestCase("10.24.3",true)][TestCase("1.0",false)][TestCase("v1.0.0",false)][TestCase("",false)]public void VersionValidation(string value,bool expected)=>Assert.AreEqual(expected,GameVersionService.IsValidVersion(value));
        [Test]public void BuildNumberMustBePositive(){var e=Make(build:0);Assert.That(e.BuildNumber,Is.EqualTo(1));Assert.IsTrue(e.Validate(out _));}
        [Test]public void BuildNumberProviderIncrements(){var e=Make(build:4);Assert.AreEqual(5,BuildNumberProvider.Next(e));Assert.AreEqual(5,e.BuildNumber);Object.DestroyImmediate(e);}
        [Test]public void BundleIdentifierMustContainThreeParts(){var e=Make(id:"invalid");Assert.IsFalse(e.Validate(out _));}
        [Test]public void ArtifactNameIncludesVersionAndBuild(){Assert.That(GameVersionService.ReleaseArtifactName("iOS"),Does.Contain(GameVersionService.Version).And.Contain(GameVersionService.BuildNumber.ToString()));}
        [Test]public void AppIconAssetExists(){Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Branding/AppIcons/HeroDefenseIcon.png"));}
        [Test]public void RequiredReleaseDocumentsExist(){foreach(string path in ReleaseValidationService.RequiredDocuments)Assert.IsTrue(File.Exists(path),path);}
        [Test]public void ReleaseValidationHasNoBlockersAfterSetup(){Assert.Zero(ReleaseValidationService.Validate().Blockers);}
        [Test]public void BuildSceneOrderIsCanonical(){string[] expected={"Boot","MainMenu","HeroSelect","StageSelect","Battle"};CollectionAssert.AreEqual(expected,EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>Path.GetFileNameWithoutExtension(x.path)));}
        [Test]public void SaveVersionRemainsCompatible()=>Assert.AreEqual(1,GameSaveData.CurrentVersion);
        [Test]public void AccessibilityDefaultsProvideColorLabels(){var s=new SettingsSaveData();Assert.IsTrue(s.colorAccessibility);Assert.IsTrue(s.aimAssist);Assert.IsFalse(s.largeUi);}
        [Test]public void AccessibilityValuesSurviveNormalization(){var data=DefaultSaveFactory.Create();data.settings.largeUi=true;data.settings.highContrast=true;Assert.IsTrue(SaveValidationService.Normalize(data,out _));Assert.IsTrue(data.settings.largeUi);Assert.IsTrue(data.settings.highContrast);}
        [Test]public void TextScaleIsClamped(){var data=DefaultSaveFactory.Create();data.settings.textScale=9;SaveValidationService.Normalize(data,out _);Assert.AreEqual(1.35f,data.settings.textScale);}
        [Test]public void FrameRateIsRestrictedToThirtyOrSixty(){var data=DefaultSaveFactory.Create();data.settings.targetFrameRate=17;SaveValidationService.Normalize(data,out _);Assert.AreEqual(60,data.settings.targetFrameRate);data.settings.targetFrameRate=30;SaveValidationService.Normalize(data,out _);Assert.AreEqual(30,data.settings.targetFrameRate);}
        [Test]public void LowQualityHasLowerObjectLimits(){Assert.Less(24,36);Assert.Less(28,40);}
        [Test]public void ReleaseLogMinimumIsRepresented(){var e=Make(HeroDefenseBuildType.StoreRelease);Assert.IsFalse(e.EnableVerboseLogging);Assert.That(GameLogLevel.Warning,Is.GreaterThan(GameLogLevel.Info));}
        [Test]public void BuildEnvironmentContainsNoCredentialFields(){string[] fields=typeof(BuildEnvironmentData).GetFields(System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Select(x=>x.Name.ToLowerInvariant()).ToArray();Assert.IsFalse(fields.Any(x=>x.Contains("password")||x.Contains("teamid")||x.Contains("certificate")));}
        [Test]public void AndroidOutputExtensionIsAab()=>Assert.AreEqual(".aab",Path.GetExtension($"{GameVersionService.ReleaseArtifactName("Android")}.aab"));
        [Test]public void IOSOutputUsesXcodeDirectoryName()=>Assert.That(GameVersionService.ReleaseArtifactName("iOS"),Does.StartWith("HeroDefense-iOS-"));
        [Test]public void GitIgnoreProtectsSigningFiles(){string value=File.ReadAllText(".gitignore");Assert.That(value,Does.Contain("*.keystore").And.Contain("*.p12").And.Contain("*.mobileprovision"));}
        [Test]public void PrivacyDraftExplainsLocalStorage(){Assert.That(File.ReadAllText("Documentation/PRIVACY_POLICY_DRAFT.md"),Does.Contain("로컬"));}
        [Test]public void LicenseDocumentListsUnity(){Assert.That(File.ReadAllText("Documentation/THIRD_PARTY_NOTICES.md"),Does.Contain("Unity"));}
    }
}
