namespace HeroDefense.Battle.Buildings
{
    /// <summary>UI-independent occupancy state for one uniquely identified construction slot.</summary>
    public sealed class BuildSlotState
    {
        public string SlotId { get; }
        public bool IsActive { get; set; }=true;
        public BuildingRuntimeState Occupant { get; private set; }
        public bool IsOccupied=>Occupant!=null;
        public BuildSlotState(string id){SlotId=id;}
        public bool TryOccupy(BuildingRuntimeState building){if(!IsActive||IsOccupied||building==null)return false;Occupant=building;return true;}
        public void Release(BuildingRuntimeState building){if(Occupant==building)Occupant=null;}
        public void Reset()=>Occupant=null;
    }
}
