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
            foreach(var audio in Object.FindObjectsByType<AudioManager>(FindObjectsInactive.Include,FindObjectsSortMode.None))Object.Destroy(audio.gameObject);
            if (SaveGameManager.Instance != null) Object.Destroy(SaveGameManager.Instance.gameObject);
            yield return null;
            directory = Path.Combine(Application.temporaryCachePath, "HeroDefensePhase9PlayMode", System.Guid.NewGuid().ToString("N"));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach(var audio in Object.FindObjectsByType<AudioManager>(FindObjectsInactive.Include,FindObjectsSortMode.None))Object.Destroy(audio.gameObject);
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
            Assert.That(AudioManager.Instance.PlayEvent(GameAudioEvent.AttackHit),Is.False);
        }

        [UnityTest]
        public IEnumerator AudioManagerDoesNotRestartSameMusicRequest()
        {
            var manager=new GameObject("Phase9AudioManager").AddComponent<AudioManager>();
            var clip=AudioClip.Create("TestMusic",4410,1,44100,false);
            manager.ConfigureEventForTests(GameAudioEvent.BattleMusic,clip,AudioChannel.Music);
            manager.NotifyUserGesture();Assert.That(manager.PlayEvent(GameAudioEvent.BattleMusic),Is.True);yield return null;
            Assert.That(manager.CurrentMusicClip,Is.SameAs(clip));Assert.That(manager.PlayEvent(GameAudioEvent.BattleMusic),Is.True);Assert.That(manager.CurrentMusicClip,Is.SameAs(clip));
            Object.Destroy(clip);
        }

        [UnityTest]
        public IEnumerator AudioManagerAppliesSfxVolumeAndPauseState()
        {
            var save=new GameObject("Phase9SaveManager").AddComponent<SaveGameManager>();save.InitializeForTests(new JsonFileSaveRepository(directory));save.Data.settings.sfxVolume=.25f;
            var manager=AudioManager.Instance!=null?AudioManager.Instance:new GameObject("Phase9AudioManager").AddComponent<AudioManager>();
            var clip=AudioClip.Create("TestSfx",44100,1,44100,false);Assert.That(clip,Is.Not.Null);Assert.That(manager.transform.childCount,Is.EqualTo(14));int available=0;foreach(var source in manager.GetComponentsInChildren<AudioSource>())if(source.clip==null)available++;Assert.That(available,Is.EqualTo(14));
            manager.ConfigureEventForTests(GameAudioEvent.BuildingPlaced,clip,AudioChannel.Sfx,.8f);manager.NotifyUserGesture();Assert.That(manager.PlayEvent(GameAudioEvent.BuildingPlaced),Is.True);yield return null;
            AudioSource match=null;foreach(var source in manager.GetComponentsInChildren<AudioSource>())if(source.clip==clip){match=source;break;}Assert.That(match,Is.Not.Null);Assert.That(match.volume,Is.EqualTo(.2f).Within(.001f));
            manager.SendMessage("OnApplicationPause",true);Assert.That(AudioListener.pause,Is.True);manager.SendMessage("OnApplicationPause",false);Assert.That(AudioListener.pause,Is.False);Object.Destroy(clip);
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
