using System.Collections;using System.IO;using HeroDefense.Accessibility;using HeroDefense.Build;using HeroDefense.Performance;using HeroDefense.Save;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;using UnityEngine.UI;
namespace HeroDefense.Tests.PlayMode
{
    public sealed class Phase10QualityPlayModeTests
    {
        private string directory;
        [UnitySetUp]public IEnumerator SetUp(){if(SaveGameManager.Instance!=null)Object.Destroy(SaveGameManager.Instance.gameObject);yield return null;directory=Path.Combine(Application.temporaryCachePath,"HDPhase10",System.Guid.NewGuid().ToString("N"));var save=new GameObject("Phase10Save").AddComponent<SaveGameManager>();save.InitializeForTests(new JsonFileSaveRepository(directory));}
        [UnityTearDown]public IEnumerator TearDown(){if(SaveGameManager.Instance!=null)Object.Destroy(SaveGameManager.Instance.gameObject);var monitors=Object.FindObjectsByType<PerformanceMonitor>(FindObjectsInactive.Include);for(int i=0;i<monitors.Length;i++)Object.Destroy(monitors[i].gameObject);yield return null;if(Directory.Exists(directory))Directory.Delete(directory,true);AudioListener.volume=1;Application.targetFrameRate=60;}
        [UnityTest]public IEnumerator DevelopmentPerformanceHudCanBeCreated(){var monitor=new GameObject("PerformanceMonitorTest").AddComponent<PerformanceMonitor>();yield return null;Assert.IsTrue(monitor.enabled);Assert.IsNotNull(GameObject.Find("PerformanceHUD"));}
        [UnityTest]public IEnumerator LargeUiIncreasesTextAndButtonMinimum(){SaveGameManager.Instance.Data.settings.largeUi=true;var go=new GameObject("LargeUi",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));var text=new GameObject("Text",typeof(RectTransform),typeof(Text)).GetComponent<Text>();text.transform.SetParent(go.transform);text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=20;LargeUiController.ApplyToLoadedScene();yield return null;Assert.Greater(text.fontSize,20);Assert.GreaterOrEqual(go.GetComponent<LayoutElement>().minHeight,64);Object.Destroy(go);}
        [UnityTest]public IEnumerator ThirtyFpsSettingAppliesImmediately(){SaveGameManager.Instance.Data.settings.targetFrameRate=30;SaveGameManager.Instance.ApplySettings();yield return null;Assert.AreEqual(30,Application.targetFrameRate);}
        [UnityTest]public IEnumerator AccessibilityTogglesPersistInMemory(){var s=SaveGameManager.Instance.Data.settings;s.screenShake=false;s.vibration=false;s.damageNumbers=false;s.colorAccessibility=true;SaveGameManager.Instance.NotifyChanged(SaveReason.SettingsChanged);yield return null;Assert.IsFalse(s.screenShake);Assert.IsFalse(s.vibration);Assert.IsFalse(s.damageNumbers);Assert.IsTrue(s.colorAccessibility);}
        [UnityTest]public IEnumerator VersionServiceReturnsReleaseFormat(){yield return null;Assert.That(GameVersionService.DisplayVersion,Does.StartWith("v1.0.0 (Build "));}
    }
}
