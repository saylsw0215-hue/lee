using System.Collections;
using System.IO;
using HeroDefense.Audio;
using HeroDefense.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HeroDefense.Tests.PlayMode
{
    /// <summary>Lifecycle smoke tests for save and audio services used on macOS and iOS.</summary>
    public sealed class Phase9SystemsPlayModeTests
    {
        private string directory;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (AudioManager.Instance != null) Object.Destroy(AudioManager.Instance.gameObject);
            if (SaveGameManager.Instance != null) Object.Destroy(SaveGameManager.Instance.gameObject);
            yield return null;
            directory = Path.Combine(Application.temporaryCachePath, "HeroDefensePhase9PlayMode", System.Guid.NewGuid().ToString("N"));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (AudioManager.Instance != null) Object.Destroy(AudioManager.Instance.gameObject);
            if (SaveGameManager.Instance != null) Object.Destroy(SaveGameManager.Instance.gameObject);
            yield return null;
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [UnityTest]
        public IEnumerator SaveManagerPersistsChangedCurrency()
        {
            var manager = new GameObject("Phase9SaveManager").AddComponent<SaveGameManager>();
            var repository = new JsonFileSaveRepository(directory);
            manager.InitializeForTests(repository);
            manager.Data.currencies.coin = 321;
            Assert.That(manager.SaveNow(SaveReason.Manual), Is.True);
            yield return null;
            Assert.That(repository.Load().Data.currencies.coin, Is.EqualTo(321));
        }

        [UnityTest]
        public IEnumerator AudioManagerSafelyIgnoresEmptyCue()
        {
            new GameObject("Phase9AudioManager").AddComponent<AudioManager>();
            yield return null;
            Assert.That(AudioManager.Instance, Is.Not.Null);
            Assert.That(AudioManager.Instance.Play(null), Is.False);
        }

        [UnityTest]
        public IEnumerator SettingsApplyWithoutFrameDelay()
        {
            var manager = new GameObject("Phase9SettingsManager").AddComponent<SaveGameManager>();
            manager.InitializeForTests(new JsonFileSaveRepository(directory));
            manager.Data.settings.masterVolume = .35f;
            manager.Data.settings.targetFrameRate = 30;
            manager.ApplySettings();
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(.35f).Within(.001f));
            Assert.That(Application.targetFrameRate, Is.EqualTo(30));
        }
    }
}
