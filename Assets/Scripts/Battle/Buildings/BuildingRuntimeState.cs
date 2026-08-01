namespace HeroDefense.Battle.Buildings
{
    /// <summary>Mutable per-instance state; never modifies its ScriptableObject definition.</summary>
    public sealed class BuildingRuntimeState
    {
        public BuildingData Data { get; }
        public int CurrentLevel { get; private set; }=1;
        public bool IsSold { get; private set; }
        public bool IsConstructing { get; set; }=true;
        public bool IsMaxLevel=>CurrentLevel>=Data.MaxLevel;
        public BuildingRuntimeState(BuildingData data){Data=data;}
        public bool TryUpgrade(){if(IsSold||IsConstructing||IsMaxLevel)return false;CurrentLevel++;return true;}
        public bool TryMarkSold(){if(IsSold)return false;IsSold=true;return true;}
    }
}
