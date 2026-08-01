using System.IO;
using HeroDefense.Progression;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotently creates editable Phase 7 authoring assets outside Resources.</summary>
    public static class Phase7Setup
    {
        private const string Root="Assets/ScriptableObjects/Progression";
        [MenuItem("Tools/Hero Defense/Setup Phase 7")]
        public static void Setup()
        {
            Directory.CreateDirectory(Root+"/Experience");Directory.CreateDirectory(Root+"/Upgrades/Common");Directory.CreateDirectory(Root+"/Upgrades/Rare");Directory.CreateDirectory(Root+"/Upgrades/Epic");Directory.CreateDirectory(Root+"/Upgrades/Legendary");Directory.CreateDirectory("Assets/Art/Upgrades/Icons");Directory.CreateDirectory("Assets/Prefabs/UI/Upgrades");
            string experiencePath=Root+"/Experience/DefaultExperienceTable.asset";if(AssetDatabase.LoadAssetAtPath<ExperienceTableData>(experiencePath)==null)AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ExperienceTableData>(),experiencePath);
            var all=RuntimeUpgradeCatalog.All;for(int i=0;i<all.Count;i++){UpgradeData source=all[i];string folder=Root+"/Upgrades/"+source.Rarity;string path=$"{folder}/{source.UpgradeId}.asset";if(AssetDatabase.LoadAssetAtPath<UpgradeData>(path)!=null)continue;var copy=ScriptableObject.CreateInstance<UpgradeData>();copy.Configure(source.UpgradeId,source.DisplayName,source.Description,source.Category,source.Rarity,source.MaxLevel,source.SelectionWeight,source.Effects,source.HeroId,source.Requirement);AssetDatabase.CreateAsset(copy,path);}
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate();Debug.Log($"Hero Defense Phase 7 setup complete: {all.Count} upgrades.");
        }
        private static void Validate(){var all=RuntimeUpgradeCatalog.All;var ids=new System.Collections.Generic.HashSet<string>();if(all.Count<40)throw new System.InvalidOperationException("At least 40 upgrades are required.");for(int i=0;i<all.Count;i++){if(!all[i].Validate(out string reason))throw new System.InvalidOperationException(reason);if(!ids.Add(all[i].UpgradeId))throw new System.InvalidOperationException("Duplicate upgrade ID: "+all[i].UpgradeId);}}
    }
}
