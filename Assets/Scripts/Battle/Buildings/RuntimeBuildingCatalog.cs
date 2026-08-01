using System.Collections.Generic;
using HeroDefense.Battle.Combat;
using UnityEngine;

namespace HeroDefense.Battle.Buildings
{
    /// <summary>Runtime building definitions independent from Unity Resources serialization.</summary>
    public static class RuntimeBuildingCatalog
    {
        private static readonly string[] Names={"Barracks","ArcheryRange","MagicTower","GuardBarracks","SiegeWorkshop","Sanctuary"};
        private static readonly Dictionary<string,BuildingData> Items=new();
        public static BuildingData[] GetAll(){var result=new BuildingData[Names.Length];for(int i=0;i<Names.Length;i++)result[i]=Get(Names[i]);return result;}
        public static BuildingData Get(string name)
        {
#if UNITY_EDITOR
            BuildingData authored=Resources.Load<BuildingData>("BuildingData/"+name);if(authored!=null)return authored;
#endif
            if(Items.TryGetValue(name,out BuildingData item))return item;
            item=name switch
            {
                "Barracks"=>Create("building_barracks","병영","검사를 자동 생산합니다.",100,RuntimeUnitCatalog.Get("PlayerSwordsman"),new[]{new BuildingLevelData(5,100),new BuildingLevelData(4.2f,180),new BuildingLevelData(3.5f,0)},new Color(.2f,.34f,.55f),new Vector2(112,105),BuildingVisualShape.Barracks),
                "ArcheryRange"=>Create("building_archery_range","사격장","궁수를 자동 생산합니다.",140,RuntimeUnitCatalog.Get("PlayerArcher"),new[]{new BuildingLevelData(6,140),new BuildingLevelData(5,220),new BuildingLevelData(4,0)},new Color(.48f,.31f,.15f),new Vector2(108,100),BuildingVisualShape.ArcheryRange),
                "MagicTower"=>Create("building_magic_tower","마법소","마법사를 자동 생산합니다.",180,RuntimeUnitCatalog.Get("PlayerMage"),new[]{new BuildingLevelData(8,180),new BuildingLevelData(6.8f,280),new BuildingLevelData(5.5f,0)},new Color(.38f,.2f,.58f),new Vector2(104,120),BuildingVisualShape.MagicTower),
                "GuardBarracks"=>Create("building_guard_barracks","수호 훈련소","방패병을 자동 생산합니다.",160,RuntimeUnitCatalog.Get("PlayerGuard"),new[]{new BuildingLevelData(7,160),new BuildingLevelData(5.8f,240),new BuildingLevelData(4.8f,0)},new Color(.18f,.38f,.58f),new Vector2(112,112),BuildingVisualShape.GuardBarracks),
                "SiegeWorkshop"=>Create("building_siege_workshop","공성 작업장","대포병을 자동 생산합니다.",220,RuntimeUnitCatalog.Get("PlayerCannoneer"),new[]{new BuildingLevelData(10,220),new BuildingLevelData(8.5f,330),new BuildingLevelData(7,0)},new Color(.48f,.3f,.12f),new Vector2(120,105),BuildingVisualShape.SiegeWorkshop),
                "Sanctuary"=>Create("building_sanctuary","성소","사제를 자동 생산합니다.",200,RuntimeUnitCatalog.Get("PlayerPriest"),new[]{new BuildingLevelData(9,200),new BuildingLevelData(7.5f,300),new BuildingLevelData(6.2f,0)},new Color(.72f,.62f,.28f),new Vector2(105,125),BuildingVisualShape.Sanctuary),
                _=>null
            };
            if(item!=null)Items[name]=item;else Debug.LogError("Unknown runtime BuildingData: "+name);return item;
        }
        private static BuildingData Create(string id,string display,string description,int cost,UnitData unit,BuildingLevelData[] levels,Color color,Vector2 size,BuildingVisualShape shape)
        {var data=ScriptableObject.CreateInstance<BuildingData>();data.name=id;data.hideFlags=HideFlags.DontSave;data.Configure(id,display,description,cost,.5f,unit,levels,color,size,shape);return data;}
    }
}
