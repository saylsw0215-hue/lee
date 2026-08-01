using System.IO;
using HeroDefense.Meta;
using HeroDefense.Save;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotent Phase 9 folder setup, save diagnostics and content-count validation.</summary>
    public static class Phase9Setup
    {
        [MenuItem("Tools/Hero Defense/Setup Phase 9")]
        public static void Setup(){string[] folders={"Assets/Audio/Music","Assets/Audio/SFX","Assets/Audio/Mixers","Assets/Prefabs/UI/Research","Assets/Prefabs/UI/Collection","Assets/Prefabs/UI/Achievements","Assets/Prefabs/UI/Tutorial","Assets/ScriptableObjects/Meta/Upgrades","Assets/ScriptableObjects/Meta/Rewards","Assets/ScriptableObjects/Meta/Unlocks","Assets/ScriptableObjects/Achievements","Assets/ScriptableObjects/Tutorials","Assets/ScriptableObjects/Localization","Assets/Scripts/Save/Core","Assets/Scripts/Save/Migration","Assets/Scripts/Save/Validation","Assets/Scripts/Save/Repository","Assets/Scripts/Meta"};foreach(string folder in folders)Directory.CreateDirectory(folder);Phase8Setup.Setup();if(MetaUpgradeCatalog.All.Length<18)throw new System.InvalidOperationException("At least 18 meta upgrades are required.");if(AchievementCatalog.All.Length<25)throw new System.InvalidOperationException("At least 25 achievements are required.");AssetDatabase.Refresh();Debug.Log($"Hero Defense Phase 9 setup complete: save v{GameSaveData.CurrentVersion}, {MetaUpgradeCatalog.All.Length} research items, {AchievementCatalog.All.Length} achievements.");}
        [MenuItem("Tools/Hero Defense/Save/Open Save Folder")]public static void OpenSaveFolder(){Directory.CreateDirectory(Application.persistentDataPath);EditorUtility.RevealInFinder(Application.persistentDataPath);}
        [MenuItem("Tools/Hero Defense/Save/Create New Save")]public static void CreateSave(){var repo=new JsonFileSaveRepository(Application.persistentDataPath);var result=repo.Save(DefaultSaveFactory.Create());Debug.Log(result.Success?"New save created.":"Save failed: "+result.Error);}
        [MenuItem("Tools/Hero Defense/Save/Delete Save")]public static void DeleteSave(){var result=new JsonFileSaveRepository(Application.persistentDataPath).Delete();Debug.Log(result.Success?"Save files deleted.":result.Error);}
        [MenuItem("Tools/Hero Defense/Save/Validate Save")]public static void ValidateSave(){var result=new JsonFileSaveRepository(Application.persistentDataPath).Load();if(!result.Success)throw new System.InvalidOperationException(result.Error);if(!SaveValidationService.Normalize(result.Data,out string warning))throw new System.InvalidOperationException("Save validation failed.");Debug.Log("Save validation passed. "+warning);}
        [MenuItem("Tools/Hero Defense/Save/Unlock All Content")]public static void UnlockAll(){var repo=new JsonFileSaveRepository(Application.persistentDataPath);var result=repo.Load();var data=result.Success?result.Data:DefaultSaveFactory.Create();foreach(var hero in data.heroes)hero.unlocked=true;foreach(var stage in data.stages){stage.unlocked=true;stage.normalCleared=true;}repo.Save(data);Debug.Log("All content unlocked in local save.");}
        [MenuItem("Tools/Hero Defense/Save/Add Currency")]public static void AddCurrency(){var repo=new JsonFileSaveRepository(Application.persistentDataPath);var result=repo.Load();var data=result.Success?result.Data:DefaultSaveFactory.Create();data.currencies.coin+=5000;data.currencies.soulGem+=100;repo.Save(data);Debug.Log("Added 5000 Coin and 100 Soul Gem.");}
    }
}
