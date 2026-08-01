using System;
using System.Collections.Generic;
using HeroDefense.Battle.Buildings;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Stages;
using HeroDefense.Battle.Waves;
using HeroDefense.Heroes;
using HeroDefense.Heroes.Selection;

namespace HeroDefense.Core
{
    /// <summary>Cached ID registry and player-safe validation for all runtime content definitions.</summary>
    public static class GameContentDatabase
    {
        private static Dictionary<string,HeroData> heroes;
        private static Dictionary<string,UnitData> units;
        private static Dictionary<string,BuildingData> buildings;
        private static Dictionary<string,StageData> stages;
        private static Dictionary<string,StatusEffectData> statuses;

        public static IReadOnlyCollection<HeroData> Heroes { get { Ensure(); return heroes.Values; } }
        public static IReadOnlyCollection<UnitData> Units { get { Ensure(); return units.Values; } }
        public static IReadOnlyCollection<BuildingData> Buildings { get { Ensure(); return buildings.Values; } }
        public static IReadOnlyCollection<StageData> Stages { get { Ensure(); return stages.Values; } }
        public static IReadOnlyCollection<StatusEffectData> Statuses { get { Ensure(); return statuses.Values; } }

        public static HeroData Hero(string id) { Ensure(); return id != null && heroes.TryGetValue(id,out var value) ? value : null; }
        public static UnitData Unit(string id) { Ensure(); return id != null && units.TryGetValue(id,out var value) ? value : null; }
        public static BuildingData Building(string id) { Ensure(); return id != null && buildings.TryGetValue(id,out var value) ? value : null; }
        public static StageData Stage(string id) { Ensure(); return id != null && stages.TryGetValue(id,out var value) ? value : null; }
        public static StatusEffectData Status(string id) { Ensure(); return id != null && statuses.TryGetValue(id,out var value) ? value : null; }

        public static void Rebuild()
        {
            HeroData[] heroValues=RuntimeHeroCatalog.GetHeroes();
            UnitData[] unitValues=RuntimeUnitCatalog.GetAll();
            BuildingData[] buildingValues=RuntimeBuildingCatalog.GetAll();
            StageData[] stageValues=RuntimeStageCatalog.GetAll();
            StatusEffectData[] statusValues=RuntimeStatusCatalog.GetAll();
            if(!ValidateCatalogs(heroValues,unitValues,buildingValues,stageValues,statusValues,out string reason))
                throw new InvalidOperationException(reason);
            BuildMaps(heroValues,unitValues,buildingValues,stageValues,statusValues);
        }

        public static bool Validate(out string reason)
        {
            try
            {
                HeroData[] heroValues=RuntimeHeroCatalog.GetHeroes();
                UnitData[] unitValues=RuntimeUnitCatalog.GetAll();
                BuildingData[] buildingValues=RuntimeBuildingCatalog.GetAll();
                StageData[] stageValues=RuntimeStageCatalog.GetAll();
                StatusEffectData[] statusValues=RuntimeStatusCatalog.GetAll();
                if(!ValidateCatalogs(heroValues,unitValues,buildingValues,stageValues,statusValues,out reason))return false;
                BuildMaps(heroValues,unitValues,buildingValues,stageValues,statusValues);
                return true;
            }
            catch(Exception exception)
            {
                reason=exception.Message;
                return false;
            }
        }

        private static bool ValidateCatalogs(HeroData[] heroValues,UnitData[] unitValues,BuildingData[] buildingValues,StageData[] stageValues,StatusEffectData[] statusValues,out string reason)
        {
            var ids=new HashSet<string>(StringComparer.Ordinal);
            if(heroValues==null||heroValues.Length==0)return Fail("Hero catalog: at least one entry is required.",out reason);
            if(unitValues==null||unitValues.Length==0)return Fail("Unit catalog: at least one entry is required.",out reason);
            if(buildingValues==null||buildingValues.Length==0)return Fail("Building catalog: at least one entry is required.",out reason);
            if(stageValues==null||stageValues.Length==0)return Fail("Stage catalog: at least one entry is required.",out reason);
            if(statusValues==null||statusValues.Length==0)return Fail("Status catalog: at least one entry is required.",out reason);

            for(int i=0;i<heroValues.Length;i++)
            {
                HeroData hero=heroValues[i];
                if(hero==null)return Fail($"Hero catalog[{i}]: reference is null.",out reason);
                if(!ValidateId(ids,hero.HeroId,"Hero",i,out reason))return false;
                string id=hero.HeroId;
                if(hero.ActiveSkill==null)return Fail($"Hero '{id}'.ActiveSkill: reference is null.",out reason);
                if(hero.UltimateSkill==null)return Fail($"Hero '{id}'.UltimateSkill: reference is null.",out reason);
                if(hero.Passive==null)return Fail($"Hero '{id}'.Passive: reference is null.",out reason);
                if(hero.MaxHealth<=0)return Fail($"Hero '{id}'.MaxHealth must be greater than zero.",out reason);
                if(hero.MoveSpeed<0)return Fail($"Hero '{id}'.MoveSpeed cannot be negative.",out reason);
                if(hero.AttackDamage<0)return Fail($"Hero '{id}'.AttackDamage cannot be negative.",out reason);
                if(hero.AttackRange<0)return Fail($"Hero '{id}'.AttackRange cannot be negative.",out reason);
                if(hero.AttackInterval<=0)return Fail($"Hero '{id}'.AttackInterval must be greater than zero.",out reason);
                if(hero.DetectionRange<hero.AttackRange)return Fail($"Hero '{id}'.DetectionRange must be at least AttackRange.",out reason);
                if(hero.RespawnDuration<0)return Fail($"Hero '{id}'.RespawnDuration cannot be negative.",out reason);
                if(!hero.ActiveSkill.Validate(out string detail))return Fail($"Hero '{id}'.ActiveSkill: {detail}",out reason);
                if(!hero.UltimateSkill.Validate(out detail))return Fail($"Hero '{id}'.UltimateSkill: {detail}",out reason);
                if(!hero.Passive.Validate(out detail))return Fail($"Hero '{id}'.Passive: {detail}",out reason);
                if(!hero.AdvancedStats.Validate(out detail))return Fail($"Hero '{id}'.AdvancedStats: {detail}",out reason);
            }

            for(int i=0;i<statusValues.Length;i++)
            {
                StatusEffectData status=statusValues[i];
                if(status==null)return Fail($"Status catalog[{i}]: reference is null.",out reason);
                if(!ValidateId(ids,status.EffectId,"Status",i,out reason))return false;
                if(!status.Validate(out string detail))return Fail($"Status '{status.EffectId}': {detail}",out reason);
            }

            for(int i=0;i<unitValues.Length;i++)
            {
                UnitData unit=unitValues[i];
                if(unit==null)return Fail($"Unit catalog[{i}]: reference is null.",out reason);
                if(!ValidateId(ids,unit.UnitId,"Unit",i,out reason))return false;
                if(!unit.Validate(out string detail))return Fail($"Unit '{unit.UnitId}': {detail}",out reason);
                if(unit.MoveSpeed<0)return Fail($"Unit '{unit.UnitId}'.MoveSpeed cannot be negative.",out reason);
                if(unit.AttackDamage<0)return Fail($"Unit '{unit.UnitId}'.AttackDamage cannot be negative.",out reason);
                if(unit.RewardGold<0)return Fail($"Unit '{unit.UnitId}'.RewardGold cannot be negative.",out reason);
            }
            var unitsById=new Dictionary<string,UnitData>(StringComparer.Ordinal);
            for(int i=0;i<unitValues.Length;i++)unitsById.Add(unitValues[i].UnitId,unitValues[i]);

            for(int i=0;i<buildingValues.Length;i++)
            {
                BuildingData building=buildingValues[i];
                if(building==null)return Fail($"Building catalog[{i}]: reference is null.",out reason);
                if(!ValidateId(ids,building.BuildingId,"Building",i,out reason))return false;
                if(building.ProducedUnit==null)return Fail($"Building '{building.BuildingId}'.ProducedUnit: reference is null.",out reason);
                if(!unitsById.TryGetValue(building.ProducedUnit.UnitId,out UnitData produced)||!ReferenceEquals(produced,building.ProducedUnit))return Fail($"Building '{building.BuildingId}'.ProducedUnit '{building.ProducedUnit.UnitId}' is not the shared Unit catalog entry.",out reason);
                if(!building.Validate(out string detail))return Fail($"Building '{building.BuildingId}': {detail}",out reason);
                if(building.BuildCost<0)return Fail($"Building '{building.BuildingId}'.BuildCost cannot be negative.",out reason);
                if(building.BuildingSize.x<=0||building.BuildingSize.y<=0)return Fail($"Building '{building.BuildingId}'.BuildingSize must be positive.",out reason);
                for(int level=1;level<=building.MaxLevel;level++)
                {
                    if(building.GetProductionInterval(level)<=0)return Fail($"Building '{building.BuildingId}'.Levels[{level-1}].ProductionInterval must be greater than zero.",out reason);
                    if(level<building.MaxLevel&&building.GetUpgradeCost(level)<0)return Fail($"Building '{building.BuildingId}'.Levels[{level-1}].UpgradeCost cannot be negative.",out reason);
                }
            }

            for(int i=0;i<stageValues.Length;i++)
            {
                StageData stage=stageValues[i];
                if(stage==null)return Fail($"Stage catalog[{i}]: reference is null.",out reason);
                if(!ValidateId(ids,stage.StageId,"Stage",i,out reason))return false;
                string stageId=stage.StageId;
                if(stage.StartingGold<0)return Fail($"Stage '{stageId}'.StartingGold cannot be negative.",out reason);
                if(stage.BaseMaxHealth<=0)return Fail($"Stage '{stageId}'.BaseMaxHealth must be greater than zero.",out reason);
                if(stage.VictoryReward<0)return Fail($"Stage '{stageId}'.VictoryReward cannot be negative.",out reason);
                if(stage.EnemyHealthMultiplier<=0||stage.EnemyDamageMultiplier<=0)return Fail($"Stage '{stageId}' balance multipliers must be greater than zero.",out reason);
                if(stage.Waves==null||stage.Waves.Length==0)return Fail($"Stage '{stageId}'.Waves: at least one wave is required.",out reason);
                var waveIds=new HashSet<string>(StringComparer.Ordinal);
                for(int waveIndex=0;waveIndex<stage.Waves.Length;waveIndex++)
                {
                    WaveData wave=stage.Waves[waveIndex];
                    if(wave==null)return Fail($"Stage '{stageId}'.Waves[{waveIndex}]: reference is null.",out reason);
                    if(string.IsNullOrWhiteSpace(wave.WaveId))return Fail($"Stage '{stageId}'.Waves[{waveIndex}].WaveId is null or whitespace.",out reason);
                    if(!waveIds.Add(wave.WaveId))return Fail($"Duplicate Wave ID '{wave.WaveId}' at Stage '{stageId}'.Waves[{waveIndex}].",out reason);
                    if(wave.PreparationDuration<0)return Fail($"Wave '{wave.WaveId}'.PreparationDuration cannot be negative.",out reason);
                    if(wave.CompletionDelay<0)return Fail($"Wave '{wave.WaveId}'.CompletionDelay cannot be negative.",out reason);
                    if(wave.ClearRewardGold<0)return Fail($"Wave '{wave.WaveId}'.ClearRewardGold cannot be negative.",out reason);
                    if(wave.SpawnGroups==null||wave.SpawnGroups.Length==0)return Fail($"Wave '{wave.WaveId}'.SpawnGroups: at least one group is required.",out reason);
                    for(int groupIndex=0;groupIndex<wave.SpawnGroups.Length;groupIndex++)
                    {
                        WaveSpawnGroup group=wave.SpawnGroups[groupIndex];
                        if(group==null)return Fail($"Wave '{wave.WaveId}'.SpawnGroups[{groupIndex}]: reference is null.",out reason);
                        if(group.EnemyData==null)return Fail($"Wave '{wave.WaveId}'.SpawnGroups[{groupIndex}].EnemyData: reference is null.",out reason);
                        if(!unitsById.TryGetValue(group.EnemyData.UnitId,out UnitData enemy)||!ReferenceEquals(enemy,group.EnemyData))return Fail($"Wave '{wave.WaveId}'.SpawnGroups[{groupIndex}].EnemyData '{group.EnemyData.UnitId}' is not the shared Unit catalog entry.",out reason);
                        if(group.Count<1)return Fail($"Wave '{wave.WaveId}'.SpawnGroups[{groupIndex}].Count must be at least one.",out reason);
                        if(group.InitialDelay<0)return Fail($"Wave '{wave.WaveId}'.SpawnGroups[{groupIndex}].InitialDelay cannot be negative.",out reason);
                        if(group.SpawnInterval<0)return Fail($"Wave '{wave.WaveId}'.SpawnGroups[{groupIndex}].SpawnInterval cannot be negative.",out reason);
                        if(group.SpawnPointIndex<0||group.SpawnPointIndex>=2)return Fail($"Wave '{wave.WaveId}'.SpawnGroups[{groupIndex}].SpawnPointIndex is outside [0, 1].",out reason);
                    }
                }
                if(!stage.Waves[stage.Waves.Length-1].IsBossWave)return Fail($"Stage '{stageId}'.Waves[{stage.Waves.Length-1}] must be a boss wave.",out reason);
            }

            reason=string.Empty;
            return true;
        }

        private static bool ValidateId(HashSet<string> ids,string id,string contentType,int index,out string reason)
        {
            if(string.IsNullOrWhiteSpace(id))return Fail($"{contentType} catalog[{index}].ID is null or whitespace.",out reason);
            if(!ids.Add(id))return Fail($"Duplicate content ID '{id}' at {contentType} catalog[{index}].",out reason);
            reason=string.Empty;
            return true;
        }

        private static bool Fail(string message,out string reason) { reason=message; return false; }
        private static void Ensure() { if(heroes==null||statuses==null)Rebuild(); }

        private static void BuildMaps(HeroData[] heroValues,UnitData[] unitValues,BuildingData[] buildingValues,StageData[] stageValues,StatusEffectData[] statusValues)
        {
            heroes=new Dictionary<string,HeroData>(StringComparer.Ordinal);
            units=new Dictionary<string,UnitData>(StringComparer.Ordinal);
            buildings=new Dictionary<string,BuildingData>(StringComparer.Ordinal);
            stages=new Dictionary<string,StageData>(StringComparer.Ordinal);
            statuses=new Dictionary<string,StatusEffectData>(StringComparer.Ordinal);
            for(int i=0;i<heroValues.Length;i++)heroes.Add(heroValues[i].HeroId,heroValues[i]);
            for(int i=0;i<unitValues.Length;i++)units.Add(unitValues[i].UnitId,unitValues[i]);
            for(int i=0;i<buildingValues.Length;i++)buildings.Add(buildingValues[i].BuildingId,buildingValues[i]);
            for(int i=0;i<stageValues.Length;i++)stages.Add(stageValues[i].StageId,stageValues[i]);
            for(int i=0;i<statusValues.Length;i++)statuses.Add(statusValues[i].EffectId,statusValues[i]);
        }
    }
}
