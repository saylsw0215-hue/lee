using System.IO;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotently creates Phase 3 unit/building data while preserving edited assets.</summary>
    public static class Phase3Setup
    {
        private const string UnitFolder="Assets/Resources/UnitData",BuildingFolder="Assets/Resources/BuildingData";
        [MenuItem("Tools/Hero Defense/Setup Phase 3")]
        public static void Setup()
        {
            Phase2Setup.Setup();Directory.CreateDirectory(BuildingFolder);Directory.CreateDirectory("Assets/Prefabs/Buildings");Directory.CreateDirectory("Assets/Prefabs/BuildSlots");Directory.CreateDirectory("Assets/Prefabs/Projectiles");
            UnitData archer=CreateUnit("PlayerArcher","player_archer","궁수",65,1.8f,14,4.5f,1.2f,10,.4f,new Color(.18f,.62f,.82f),UnitVisualShape.Archer);
            UnitData mage=CreateUnit("PlayerMage","player_mage","마법사",55,1.6f,24,4f,1.7f,10,.42f,new Color(.58f,.25f,.88f),UnitVisualShape.Mage);
            UnitData sword=AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitFolder}/PlayerSwordsman.asset");
            CreateBuilding("Barracks","building_barracks","병영","검사를 자동 생산합니다.",100,sword,new[]{new BuildingLevelData(5f,100),new BuildingLevelData(4.2f,180),new BuildingLevelData(3.5f,0)},new Color(.2f,.34f,.55f),new Vector2(112,105),BuildingVisualShape.Barracks);
            CreateBuilding("ArcheryRange","building_archery_range","사격장","궁수를 자동 생산합니다.",140,archer,new[]{new BuildingLevelData(6f,140),new BuildingLevelData(5f,220),new BuildingLevelData(4f,0)},new Color(.48f,.31f,.15f),new Vector2(108,100),BuildingVisualShape.ArcheryRange);
            CreateBuilding("MagicTower","building_magic_tower","마법소","마법사를 자동 생산합니다.",180,mage,new[]{new BuildingLevelData(8f,180),new BuildingLevelData(6.8f,280),new BuildingLevelData(5.5f,0)},new Color(.38f,.2f,.58f),new Vector2(104,120),BuildingVisualShape.MagicTower);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();ValidateAll();Debug.Log("Hero Defense Phase 3 setup complete. Existing data assets were preserved.");
        }
        private static UnitData CreateUnit(string file,string id,string name,float hp,float speed,float damage,float range,float interval,float detection,float radius,Color color,UnitVisualShape shape)
        {
            string path=$"{UnitFolder}/{file}.asset";var existing=AssetDatabase.LoadAssetAtPath<UnitData>(path);if(existing!=null)return existing;
            var data=ScriptableObject.CreateInstance<UnitData>();data.Configure(id,name,Team.Player,hp,speed,damage,range,interval,detection,radius,0,color,shape);AssetDatabase.CreateAsset(data,path);return data;
        }
        private static void CreateBuilding(string file,string id,string name,string description,int cost,UnitData unit,BuildingLevelData[] levels,Color color,Vector2 size,BuildingVisualShape shape)
        {
            string path=$"{BuildingFolder}/{file}.asset";if(AssetDatabase.LoadAssetAtPath<BuildingData>(path)!=null)return;
            var data=ScriptableObject.CreateInstance<BuildingData>();data.Configure(id,name,description,cost,.5f,unit,levels,color,size,shape);AssetDatabase.CreateAsset(data,path);
        }
        private static void ValidateAll()
        {
            string[] units={"PlayerArcher","PlayerMage"};foreach(string name in units){var data=AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitFolder}/{name}.asset");if(data==null)throw new System.InvalidOperationException($"Missing {name}.");if(!data.Validate(out string reason))throw new System.InvalidOperationException($"Invalid {name}: {reason}");}
            string[] buildings={"Barracks","ArcheryRange","MagicTower"};foreach(string name in buildings){var data=AssetDatabase.LoadAssetAtPath<BuildingData>($"{BuildingFolder}/{name}.asset");if(data==null)throw new System.InvalidOperationException($"Missing {name}.");if(!data.Validate(out string reason))throw new System.InvalidOperationException($"Invalid {name}: {reason}");}
        }
    }
}
