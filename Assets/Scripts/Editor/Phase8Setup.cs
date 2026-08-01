using System.Collections.Generic;
using System.IO;
using HeroDefense.Core;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotent Phase 8 scene/content setup and registry validation.</summary>
    public static class Phase8Setup
    {
        [MenuItem("Tools/Hero Defense/Setup Phase 8")]
        public static void Setup()
        {
            string[] folders={"Assets/ScriptableObjects/Content","Assets/ScriptableObjects/Difficulty","Assets/ScriptableObjects/Stages/Phase8","Assets/Prefabs/Environment/Phase8","Assets/Art/Placeholders/Phase8","Assets/Scripts/UI/Stages"};foreach(string folder in folders)Directory.CreateDirectory(folder);
            Phase1Setup.Setup();ValidateAllContent();AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("Hero Defense Phase 8 setup complete: 6 heroes, 6 buildings, 4 stages and expanded enemy catalog registered.");
        }

        [MenuItem("Tools/Hero Defense/Validate All Content")]
        public static void ValidateAllContent()
        {
            if(!GameContentDatabase.Validate(out string reason))throw new System.InvalidOperationException("Content validation failed: "+reason);
            var ids=new HashSet<string>();foreach(var hero in GameContentDatabase.Heroes){if(!ids.Add(hero.HeroId))throw new System.InvalidOperationException("Duplicate hero ID: "+hero.HeroId);if(hero.ActiveSkill==null||hero.UltimateSkill==null||hero.Passive==null)throw new System.InvalidOperationException("Hero skill reference missing: "+hero.HeroId);}
            foreach(var building in GameContentDatabase.Buildings)if(building.ProducedUnit==null)throw new System.InvalidOperationException("Produced unit missing: "+building.BuildingId);
            foreach(var stage in GameContentDatabase.Stages)if(!stage.Validate(out reason))throw new System.InvalidOperationException(stage.StageId+": "+reason);
            string[] scenes={SceneNames.Boot,SceneNames.MainMenu,SceneNames.HeroSelect,SceneNames.StageSelect,SceneNames.Battle};foreach(string scene in scenes)if(!File.Exists(Phase1Setup.ScenePath(scene)))throw new FileNotFoundException("Build scene missing",scene);
            Debug.Log($"Content validation passed: {GameContentDatabase.Heroes.Count} heroes, {GameContentDatabase.Units.Count} units, {GameContentDatabase.Buildings.Count} buildings, {GameContentDatabase.Stages.Count} stages.");
        }
    }
}
