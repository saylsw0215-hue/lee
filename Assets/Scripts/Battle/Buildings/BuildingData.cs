using HeroDefense.Battle.Combat;
using UnityEngine;

namespace HeroDefense.Battle.Buildings
{
    [System.Serializable]
    public struct BuildingLevelData
    {
        [SerializeField] private float productionInterval;
        [SerializeField] private int upgradeCost;
        public float ProductionInterval => productionInterval;
        public int UpgradeCost => upgradeCost;
        public BuildingLevelData(float interval, int cost) { productionInterval = interval; upgradeCost = cost; }
    }

    public enum BuildingVisualShape { Barracks, ArcheryRange, MagicTower, GuardBarracks, SiegeWorkshop, Sanctuary }

    /// <summary>Immutable authoring data for a production building and its levels.</summary>
    [CreateAssetMenu(fileName="BuildingData", menuName="Hero Defense/Buildings/Building Data")]
    public sealed class BuildingData : ScriptableObject
    {
        [SerializeField] private string buildingId;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField, Min(0)] private int buildCost;
        [SerializeField, Range(0f,1f)] private float sellRatio=.5f;
        [SerializeField] private UnitData producedUnit;
        [SerializeField] private BuildingLevelData[] levels;
        [SerializeField] private Color buildingColor=Color.white;
        [SerializeField] private Vector2 buildingSize=new(120,120);
        [SerializeField] private BuildingVisualShape visualShape;

        public string BuildingId=>buildingId; public string DisplayName=>displayName; public string Description=>description;
        public int BuildCost=>buildCost; public UnitData ProducedUnit=>producedUnit; public int MaxLevel=>levels?.Length??0;
        public Color BuildingColor=>buildingColor; public Vector2 BuildingSize=>buildingSize; public BuildingVisualShape VisualShape=>visualShape;
        public float GetProductionInterval(int level)=>levels[Mathf.Clamp(level-1,0,MaxLevel-1)].ProductionInterval;
        public int GetUpgradeCost(int currentLevel)=>currentLevel>=MaxLevel?0:levels[Mathf.Clamp(currentLevel-1,0,MaxLevel-1)].UpgradeCost;
        public int CalculateSellValue(int currentLevel)
        {
            int invested=buildCost; for(int level=1;level<currentLevel;level++) invested+=GetUpgradeCost(level);
            return Mathf.FloorToInt(invested*sellRatio);
        }
        public bool Validate(out string reason)
        {
            if(string.IsNullOrWhiteSpace(buildingId)||string.IsNullOrWhiteSpace(displayName)){reason="Building ID and display name are required.";return false;}
            if(buildCost<0||producedUnit==null||levels==null||levels.Length==0){reason="Cost, produced unit, and levels must be configured.";return false;}
            for(int i=0;i<levels.Length;i++) if(levels[i].ProductionInterval<=0f){reason="Production intervals must be positive.";return false;}
            reason=string.Empty;return true;
        }
        public void Configure(string id,string name,string details,int cost,float saleRatio,UnitData unit,BuildingLevelData[] levelData,Color color,Vector2 size,BuildingVisualShape shape)
        {buildingId=id;displayName=name;description=details;buildCost=cost;sellRatio=saleRatio;producedUnit=unit;levels=levelData;buildingColor=color;buildingSize=size;visualShape=shape;}
    }
}
