using System;
using System.Collections;
using System.IO;
using HeroDefense.Core;
using HeroDefense.Heroes.Selection;
using HeroDefense.Meta;
using HeroDefense.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HeroDefense.Tests
{
    public sealed class HeroSelectUnlockPlayModeTests
    {
        private string currentDirectory;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if(SaveGameManager.Instance!=null)UnityEngine.Object.Destroy(SaveGameManager.Instance.gameObject);
            if(HeroSelectionService.Instance!=null)UnityEngine.Object.Destroy(HeroSelectionService.Instance.gameObject);
            yield return null;
            if(!string.IsNullOrEmpty(currentDirectory)&&Directory.Exists(currentDirectory))Directory.Delete(currentDirectory,true);
            currentDirectory=null;
        }

        [UnityTest]
        public IEnumerator UnlockRefreshesOverlayAndSelectionImmediately()
        {
            currentDirectory=Path.Combine(Application.temporaryCachePath,"HeroSelectUnlockPlayModeTests",Guid.NewGuid().ToString("N"));
            yield return ReplaceSaveManager(currentDirectory);
            SaveGameManager save=SaveGameManager.Instance;
            SaveRecords.Stage(save.Data,"stage_01_grassland").normalCleared=true;
            save.Data.currencies.coin=500;

            yield return Load(SceneNames.HeroSelect);
            GameObject card=GameObject.Find("hero_kai_engineer");
            Assert.That(card,Is.Not.Null);
            Transform overlay=card.transform.Find("Locked");
            Assert.That(overlay,Is.Not.Null);
            Assert.That(overlay.gameObject.activeSelf,Is.True);

            card.GetComponent<Button>().onClick.Invoke();
            yield return null;

            Assert.That(overlay.gameObject.activeSelf,Is.False);
            Assert.That(GameObject.Find("Selection").GetComponent<Text>().text,Does.Contain("카이"));
            Assert.That(HeroSelectionService.Instance.SelectedHero.HeroId,Is.EqualTo("hero_kai_engineer"));
            Assert.That(new HeroUnlockService(save).IsUnlocked("hero_kai_engineer"),Is.True);
        }

        [UnityTest]
        public IEnumerator LockedHeroCannotBecomeStageSelectionTarget()
        {
            currentDirectory=Path.Combine(Application.temporaryCachePath,"HeroSelectLockedPlayModeTests",Guid.NewGuid().ToString("N"));
            yield return ReplaceSaveManager(currentDirectory);
            SaveRecords.Stage(SaveGameManager.Instance.Data,"stage_01_grassland").normalCleared=true;
            SaveGameManager.Instance.Data.currencies.coin=499;

            yield return Load(SceneNames.HeroSelect);
            GameObject.Find("hero_kai_engineer").GetComponent<Button>().onClick.Invoke();
            GameObject.Find("StartBattle").GetComponent<Button>().onClick.Invoke();
            yield return new WaitForSecondsRealtime(.2f);

            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo(SceneNames.StageSelect));
            Assert.That(HeroSelectionService.Instance.SelectedHero.HeroId,Is.Not.EqualTo("hero_kai_engineer"));
            Assert.That(new HeroUnlockService(SaveGameManager.Instance).IsUnlocked("hero_kai_engineer"),Is.False);
        }

        private static IEnumerator ReplaceSaveManager(string directory)
        {
            if(SaveGameManager.Instance!=null)UnityEngine.Object.Destroy(SaveGameManager.Instance.gameObject);
            if(HeroSelectionService.Instance!=null)UnityEngine.Object.Destroy(HeroSelectionService.Instance.gameObject);
            yield return null;
            var owner=new GameObject("SaveGameManager",typeof(SaveGameManager));
            owner.GetComponent<SaveGameManager>().InitializeForTests(new JsonFileSaveRepository(directory));
            new GameObject("HeroSelectionService",typeof(HeroSelectionService));
            yield return null;
        }

        private static IEnumerator Load(string scene)
        {
            AsyncOperation operation=SceneManager.LoadSceneAsync(scene);
            while(!operation.isDone)yield return null;
            yield return null;
        }
    }
}
