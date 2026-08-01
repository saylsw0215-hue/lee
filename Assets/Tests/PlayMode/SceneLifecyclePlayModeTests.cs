using System.Collections;
using HeroDefense.Audio;
using HeroDefense.Core;
using HeroDefense.Heroes.Selection;
using HeroDefense.Performance;
using HeroDefense.Save;
using HeroDefense.Tutorial;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HeroDefense.Tests
{
    public sealed class SceneLifecyclePlayModeTests
    {
        [UnityTest]
        public IEnumerator MainMenuAndBattleTenRoundTripsDoNotDuplicatePersistentServices()
        {
            yield return Load(SceneNames.MainMenu);
            if(SceneLoader.Instance==null)new GameObject("SceneLoader",typeof(SceneLoader));
            if(SaveGameManager.Instance==null)new GameObject("SaveGameManager",typeof(SaveGameManager));
            if(HeroSelectionService.Instance==null)new GameObject("HeroSelectionService",typeof(HeroSelectionService));
            if(AudioManager.Instance==null)new GameObject("AudioManager",typeof(AudioManager));
            if(TutorialManager.Instance==null)new GameObject("TutorialManager",typeof(TutorialManager));
            yield return null;
            PersistentCounts baseline=Capture();
            Assert.That(baseline.SceneLoaders,Is.EqualTo(1));
            Assert.That(baseline.SaveManagers,Is.EqualTo(1));
            Assert.That(baseline.HeroSelections,Is.EqualTo(1));
            Assert.That(baseline.AudioManagers,Is.EqualTo(1));
            Assert.That(baseline.TutorialManagers,Is.LessThanOrEqualTo(1));
            Assert.That(baseline.PerformanceMonitors,Is.LessThanOrEqualTo(1));

            for(int i=0;i<10;i++)
            {
                yield return Load(SceneNames.Battle);
                AssertCounts(baseline,$"Battle iteration {i+1}");
                yield return Load(SceneNames.MainMenu);
                AssertCounts(baseline,$"MainMenu iteration {i+1}");
            }
            Assert.That(Time.timeScale,Is.EqualTo(1f));
        }

        private static void AssertCounts(PersistentCounts expected,string context)
        {
            PersistentCounts actual=Capture();
            Assert.That(actual.SceneLoaders,Is.EqualTo(expected.SceneLoaders),context+" SceneLoader");
            Assert.That(actual.SaveManagers,Is.EqualTo(expected.SaveManagers),context+" SaveGameManager");
            Assert.That(actual.HeroSelections,Is.EqualTo(expected.HeroSelections),context+" HeroSelectionService");
            Assert.That(actual.AudioManagers,Is.EqualTo(expected.AudioManagers),context+" AudioManager");
            Assert.That(actual.TutorialManagers,Is.EqualTo(expected.TutorialManagers),context+" TutorialManager");
            Assert.That(actual.PerformanceMonitors,Is.EqualTo(expected.PerformanceMonitors),context+" PerformanceMonitor");
            Assert.That(actual.PersistentCanvases,Is.EqualTo(expected.PersistentCanvases),context+" persistent Canvas");
        }

        private static PersistentCounts Capture()
        {
            int persistentCanvases=0;
            Canvas[] canvases=Resources.FindObjectsOfTypeAll<Canvas>();
            for(int i=0;i<canvases.Length;i++)if(canvases[i]!=null&&canvases[i].gameObject.scene.name=="DontDestroyOnLoad")persistentCanvases++;
            return new PersistentCounts(
                Count<SceneLoader>(),Count<SaveGameManager>(),Count<HeroSelectionService>(),Count<AudioManager>(),
                Count<TutorialManager>(),Count<PerformanceMonitor>(),persistentCanvases);
        }

        private static int Count<T>() where T:Object
        {
            T[] values=Resources.FindObjectsOfTypeAll<T>();
            int count=0;
            for(int i=0;i<values.Length;i++)if(values[i]!=null)count++;
            return count;
        }

        private readonly struct PersistentCounts
        {
            public readonly int SceneLoaders,SaveManagers,HeroSelections,AudioManagers,TutorialManagers,PerformanceMonitors,PersistentCanvases;
            public PersistentCounts(int sceneLoaders,int saveManagers,int heroSelections,int audioManagers,int tutorialManagers,int performanceMonitors,int persistentCanvases)
            {SceneLoaders=sceneLoaders;SaveManagers=saveManagers;HeroSelections=heroSelections;AudioManagers=audioManagers;TutorialManagers=tutorialManagers;PerformanceMonitors=performanceMonitors;PersistentCanvases=persistentCanvases;}
        }

        private static IEnumerator Load(string scene)
        {
            AsyncOperation operation=SceneManager.LoadSceneAsync(scene);
            while(!operation.isDone)yield return null;
            yield return null;
        }

    }
}
