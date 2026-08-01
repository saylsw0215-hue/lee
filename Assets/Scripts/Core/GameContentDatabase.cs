using System.Collections.Generic;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Stages;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;

namespace HeroDefense.Core
{
    /// <summary>Cached ID registry for all player-safe Phase 8 runtime definitions.</summary>
    public static class GameContentDatabase
    {
        private static Dictionary<string,HeroData> heroes;private static Dictionary<string,UnitData> units;private static Dictionary<string,BuildingData> buildings;private static Dictionary<string,StageData> stages;
        public static IReadOnlyCollection<HeroData> Heroes{get{Ensure();return heroes.Values;}} public static IReadOnlyCollection<UnitData> Units{get{Ensure();return units.Values;}} public static IReadOnlyCollection<BuildingData> Buildings{get{Ensure();return buildings.Values;}} public static IReadOnlyCollection<StageData> Stages{get{Ensure();return stages.Values;}}
        public static HeroData Hero(string id){Ensure();return id!=null&&heroes.TryGetValue(id,out var v)?v:null;} public static UnitData Unit(string id){Ensure();return id!=null&&units.TryGetValue(id,out var v)?v:null;} public static BuildingData Building(string id){Ensure();return id!=null&&buildings.TryGetValue(id,out var v)?v:null;} public static StageData Stage(string id){Ensure();return id!=null&&stages.TryGetValue(id,out var v)?v:null;}
        public static void Rebuild(){heroes=new();units=new();buildings=new();stages=new();foreach(var v in RuntimeHeroCatalog.GetHeroes())Add(heroes,v.HeroId,v);foreach(var v in RuntimeUnitCatalog.GetAll())Add(units,v.UnitId,v);foreach(var v in RuntimeBuildingCatalog.GetAll())Add(buildings,v.BuildingId,v);foreach(var v in RuntimeStageCatalog.GetAll())Add(stages,v.StageId,v);}
        public static bool Validate(out string reason){try{Rebuild();reason=string.Empty;return heroes.Count==6&&buildings.Count==6&&stages.Count==4&&units.Count>=22;}catch(System.Exception e){reason=e.Message;return false;}}
        private static void Ensure(){if(heroes==null)Rebuild();} private static void Add<T>(Dictionary<string,T> map,string id,T value){if(string.IsNullOrWhiteSpace(id)||value==null)throw new System.InvalidOperationException("Content ID/reference is missing.");if(!map.TryAdd(id,value))throw new System.InvalidOperationException("Duplicate content ID: "+id);}
    }
}
