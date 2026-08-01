using System;
using System.IO;
using UnityEngine;

namespace HeroDefense.Save
{
    public readonly struct SaveLoadResult{public readonly bool Success,Recovered;public readonly GameSaveData Data;public readonly string Error;public SaveLoadResult(bool success,GameSaveData data,string error="",bool recovered=false){Success=success;Data=data;Error=error;Recovered=recovered;}}
    public readonly struct SaveWriteResult{public readonly bool Success;public readonly string Error;public SaveWriteResult(bool success,string error=""){Success=success;Error=error;}}
    public interface ISaveRepository{bool HasSaveData{get;}SaveLoadResult Load();SaveWriteResult Save(GameSaveData data);SaveWriteResult Delete();}

    /// <summary>Atomic JSON save with last-known-good backup and corrupt-file preservation.</summary>
    public sealed class JsonFileSaveRepository:ISaveRepository
    {
        public readonly string MainPath,BackupPath,TempPath;public bool HasSaveData=>File.Exists(MainPath);
        public JsonFileSaveRepository(string directory){Directory.CreateDirectory(directory);MainPath=Path.Combine(directory,"hero_defense_save.json");BackupPath=Path.Combine(directory,"hero_defense_save.backup.json");TempPath=Path.Combine(directory,"hero_defense_save.temp.json");}
        public SaveLoadResult Load()
        {
            RecoverTemp();if(TryRead(MainPath,out var data,out string error))return new(true,data);if(TryRead(BackupPath,out data,out string backupError))return new(true,data,error,true);return new(false,null,string.IsNullOrEmpty(error)?backupError:error);
        }
        public SaveWriteResult Save(GameSaveData data)
        {
            try{data.lastSavedAtUtc=DateTime.UtcNow.ToString("O");string json=JsonUtility.ToJson(data,true);File.WriteAllText(TempPath,json);if(File.Exists(MainPath)){File.Copy(MainPath,BackupPath,true);File.Delete(MainPath);}File.Move(TempPath,MainPath);return new(true);}catch(Exception e){return new(false,e.Message);}
        }
        public SaveWriteResult Delete(){try{DeleteIf(MainPath);DeleteIf(BackupPath);DeleteIf(TempPath);return new(true);}catch(Exception e){return new(false,e.Message);}}
        private void RecoverTemp(){try{if(!File.Exists(TempPath))return;if(!File.Exists(MainPath))File.Move(TempPath,MainPath);else File.Delete(TempPath);}catch(Exception e){Debug.LogWarning("Save temp recovery failed: "+e.Message);}}
        private static bool TryRead(string path,out GameSaveData data,out string error){data=null;error="";if(!File.Exists(path)){error="Save file not found.";return false;}try{data=JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));if(data==null)throw new InvalidDataException("JSON produced null data.");if(data.saveVersion>GameSaveData.CurrentVersion)throw new InvalidDataException("Save file is from a newer game version.");return true;}catch(Exception e){error=e.Message;return false;}}
        private static void DeleteIf(string path){if(File.Exists(path))File.Delete(path);}
    }
}
