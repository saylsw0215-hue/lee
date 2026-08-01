using System;
using UnityEngine;

namespace HeroDefense.Save
{
    public enum SaveReason{Boot,SettingsChanged,ProgressChanged,BattleResult,ApplicationPaused,ApplicationQuit,Manual,Reset}
    /// <summary>Persistent owner of validated save state; batches ordinary writes and flushes lifecycle writes.</summary>
    public sealed class SaveGameManager:MonoBehaviour
    {
        public static SaveGameManager Instance{get;private set;}public static event Action<string> SaveFailed;public GameSaveData Data{get;private set;}public ISaveRepository Repository{get;private set;}public bool LastLoadRecovered{get;private set;}public event Action DataChanged;private bool dirty;private float saveDelay;
        private void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);Repository=new JsonFileSaveRepository(Application.persistentDataPath);LoadOrCreate();}
        public void InitializeForTests(ISaveRepository repository){Repository=repository;LoadOrCreate();}
        public void LoadOrCreate(){var result=Repository.Load();Data=result.Success?result.Data:DefaultSaveFactory.Create();LastLoadRecovered=result.Recovered;if(!new SaveMigrationService().TryMigrate(Data)||!SaveValidationService.Normalize(Data,out _))Data=DefaultSaveFactory.Create();ApplySettings();if(!result.Success)SaveNow(SaveReason.Boot);DataChanged?.Invoke();}
        public void RequestSave(SaveReason reason){dirty=true;saveDelay=.25f;if(reason==SaveReason.ApplicationPaused||reason==SaveReason.ApplicationQuit||reason==SaveReason.BattleResult||reason==SaveReason.Reset)SaveNow(reason);}
        public bool SaveNow(SaveReason reason){if(Data==null||Repository==null)return false;Data.profile.lastPlayedAtUtc=DateTime.UtcNow.ToString("O");SaveValidationService.Normalize(Data,out _);var result=Repository.Save(Data);dirty=!result.Success;if(!result.Success){Debug.LogError("Game data save failed: "+result.Error);SaveFailed?.Invoke(result.Error);}return result.Success;}
        public void ResetData(){Repository.Delete();Data=DefaultSaveFactory.Create();ApplySettings();SaveNow(SaveReason.Reset);DataChanged?.Invoke();}
        public void NotifyChanged(SaveReason reason=SaveReason.ProgressChanged){DataChanged?.Invoke();RequestSave(reason);}
        private void Update(){if(!dirty)return;saveDelay-=Time.unscaledDeltaTime;if(saveDelay<=0)SaveNow(SaveReason.ProgressChanged);}
        private void OnApplicationPause(bool paused){if(paused)SaveNow(SaveReason.ApplicationPaused);}private void OnApplicationQuit()=>SaveNow(SaveReason.ApplicationQuit);
        private void OnDestroy(){if(Instance==this)Instance=null;}
        public void ApplySettings(){if(Data?.settings==null)return;Application.targetFrameRate=Data.settings.targetFrameRate;QualitySettings.SetQualityLevel((int)Data.settings.graphicsQuality,true);AudioListener.volume=Data.settings.masterVolume;}
    }
}
