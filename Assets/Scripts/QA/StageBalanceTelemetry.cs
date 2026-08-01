using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Statistics;
using HeroDefense.Battle.Waves;
using HeroDefense.Core;
using UnityEngine;

namespace HeroDefense.QA
{
    [Serializable] public sealed class UnitBalanceTelemetry{public string unitId;public int produced;public int kills;public float damage;}
    [Serializable] public sealed class WaveBalanceTelemetry
    {
        public int wave;public float startTime;public float endTime;public int enemiesSpawned;public int enemiesKilled;public int alliesProduced;public int buildingsInstalled;public int buildingsUpgraded;public int goldSpent;public int goldEarned;public int remainingGold;public int baseHp;public float heroDamage;public int heroKills;public int skillUses;public int ultimateUses;public bool won;public string outcome;public List<UnitBalanceTelemetry> units=new();
    }
    [Serializable] public sealed class StageBalanceTelemetry
    {
        public string generatedAtUtc;public string stageId;public string heroId;public string difficulty;public bool won;public string outcome;public float duration;public int finalBaseHp;public int finalGold;public int maximumActiveObjects;public bool bossSpawned;public List<string> errors=new();public List<WaveBalanceTelemetry> waves=new();
        public bool HasInvalidNumbers()
        {
            if(float.IsNaN(duration)||float.IsInfinity(duration)||finalBaseHp<0||finalGold<0)return true;
            for(int i=0;i<waves.Count;i++){WaveBalanceTelemetry w=waves[i];if(float.IsNaN(w.startTime)||float.IsInfinity(w.startTime)||float.IsNaN(w.endTime)||float.IsInfinity(w.endTime)||w.baseHp<0||w.remainingGold<0)return true;for(int u=0;u<w.units.Count;u++)if(float.IsNaN(w.units[u].damage)||float.IsInfinity(w.units[u].damage))return true;}
            return false;
        }
    }

    /// <summary>Event-fed recorder for opt-in, full Battle-scene balance runs.</summary>
    public sealed class StageBalanceTelemetrySession:IDisposable
    {
        public StageBalanceTelemetry Report{get;}
        private readonly WaveManager waves;private readonly BattleCombatController combat;private readonly BattleSessionState session;private readonly BattleStatistics stats;
        private readonly Dictionary<string,UnitBalanceTelemetry> unitTotals=new(StringComparer.Ordinal);private WaveBalanceTelemetry current;private int lastKills,lastProduced,lastBuildings,lastUpgrades,lastEarned,spentPending;private bool disposed;
        public StageBalanceTelemetrySession(string heroId,WaveManager manager,BattleCombatController battleCombat,BattleSessionState state)
        {
            waves=manager;combat=battleCombat;session=state;stats=manager.Statistics;Report=new StageBalanceTelemetry{generatedAtUtc=DateTime.UtcNow.ToString("O"),stageId=manager.Stage.StageId,heroId=heroId,difficulty=BattleLaunchConfig.Difficulty.ToString()};
            current=new WaveBalanceTelemetry{wave=1,startTime=0,remainingGold=state.CurrentGold,baseHp=state.CurrentBaseHp};manager.WaveStarted+=OnWaveStarted;manager.WaveCleared+=OnWaveCleared;manager.StageWon+=OnWon;manager.StageFailed+=OnFailed;battleCombat.UnitSpawned+=OnSpawned;battleCombat.UnitDied+=OnDied;battleCombat.UnitDamageResolved+=OnDamage;battleCombat.PlayerUnitProduced+=OnProduced;
        }
        public void RecordGoldSpent(int amount){if(amount>0)spentPending+=amount;}
        public void SampleActiveObjects(){int active=combat.ActivePlayerCount+combat.ActiveEnemyCount;if(active>Report.maximumActiveObjects)Report.maximumActiveObjects=active;if(active>65&&!Report.errors.Contains("Active combat objects exceeded the configured 30 ally + 35 enemy limits."))Report.errors.Add("Active combat objects exceeded the configured 30 ally + 35 enemy limits.");}
        public void RecordTimeout(string message){Report.errors.Add(message);Finish(false,"timeout");}
        public void Write(string path){Directory.CreateDirectory(Path.GetDirectoryName(path)??".");File.WriteAllText(path,JsonUtility.ToJson(Report,true));}
        private void OnWaveStarted(int number){if(current!=null&&current.wave==number){current.startTime=stats.PlayTime;return;}CloseCurrent(false,"interrupted");current=new WaveBalanceTelemetry{wave=number,startTime=stats.PlayTime,remainingGold=session.CurrentGold,baseHp=session.CurrentBaseHp};lastKills=stats.TotalKills;lastProduced=stats.ProducedAllies;lastBuildings=stats.InstalledBuildings;lastUpgrades=stats.Upgrades;lastEarned=stats.KillGold+stats.WaveGold;spentPending=0;}
        private void OnWaveCleared(int number,int reward){if(current==null||current.wave!=number)return;CloseCurrent(number==waves.Stage.WaveCount,"cleared");}
        private void OnWon()=>Finish(true,"victory");private void OnFailed()=>Finish(false,"defeat");
        private void CloseCurrent(bool won,string outcome)
        {
            if(current==null)return;current.endTime=stats.PlayTime;current.enemiesKilled=stats.TotalKills-lastKills;current.alliesProduced=stats.ProducedAllies-lastProduced;current.buildingsInstalled=stats.InstalledBuildings-lastBuildings;current.buildingsUpgraded=stats.Upgrades-lastUpgrades;current.goldSpent=spentPending;current.goldEarned=(stats.KillGold+stats.WaveGold)-lastEarned;current.remainingGold=session.CurrentGold;current.baseHp=session.CurrentBaseHp;current.heroDamage=stats.HeroDamageDealt;current.heroKills=stats.HeroKillCount;current.skillUses=stats.HeroSkillUseCount;current.ultimateUses=stats.HeroUltimateUseCount;current.won=won;current.outcome=outcome;foreach(UnitBalanceTelemetry unit in unitTotals.Values)current.units.Add(new UnitBalanceTelemetry{unitId=unit.unitId,produced=unit.produced,kills=unit.kills,damage=unit.damage});current.units.Sort((a,b)=>string.CompareOrdinal(a.unitId,b.unitId));Report.waves.Add(current);current=null;
        }
        private void Finish(bool won,string outcome){if(string.IsNullOrEmpty(Report.outcome)){CloseCurrent(won,outcome);Report.won=won;Report.outcome=outcome;Report.duration=stats.PlayTime;Report.finalBaseHp=session.CurrentBaseHp;Report.finalGold=session.CurrentGold;if(Report.HasInvalidNumbers())Report.errors.Add("NaN, Infinity, negative health, or negative gold detected.");}}
        private void OnSpawned(CombatUnit unit){if(unit.Team==Team.Enemy&&current!=null){current.enemiesSpawned++;if(unit.Data.UnitId.StartsWith("boss_",StringComparison.Ordinal))Report.bossSpawned=true;}}
        private void OnProduced(UnitData data){Entry(data.UnitId).produced++;}
        private void OnDied(CombatUnit unit,DamageInfo info){if(unit.Team!=Team.Enemy||info.SourceTeam!=Team.Player)return;string source=SourceUnitId(info);if(!string.IsNullOrEmpty(source))Entry(source).kills++;}
        private void OnDamage(CombatUnit target,DamageInfo info,DamageResult result){if(target.Team!=Team.Enemy||info.SourceTeam!=Team.Player||result.HealthDamage<=0)return;string source=SourceUnitId(info);if(!string.IsNullOrEmpty(source))Entry(source).damage+=result.HealthDamage;}
        private UnitBalanceTelemetry Entry(string id){if(!unitTotals.TryGetValue(id,out UnitBalanceTelemetry value)){value=new UnitBalanceTelemetry{unitId=id};unitTotals.Add(id,value);}return value;}
        private static string SourceUnitId(DamageInfo info){if(info.Source==null)return null;CombatUnit source=info.Source.GetComponent<CombatUnit>();return source!=null?source.Data.UnitId:null;}
        public void Dispose(){if(disposed)return;disposed=true;waves.WaveStarted-=OnWaveStarted;waves.WaveCleared-=OnWaveCleared;waves.StageWon-=OnWon;waves.StageFailed-=OnFailed;combat.UnitSpawned-=OnSpawned;combat.UnitDied-=OnDied;combat.UnitDamageResolved-=OnDamage;combat.PlayerUnitProduced-=OnProduced;}
    }

    public static class StageBalanceTelemetryWriter
    {
        public static string OutputDirectory=>Path.Combine("Builds","Balance");
        public static string PathFor(string heroId)=>Path.Combine(OutputDirectory,$"stage-01-{heroId}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
        public static string CsvNumber(float value)=>value.ToString("0.###",CultureInfo.InvariantCulture);
    }
}
