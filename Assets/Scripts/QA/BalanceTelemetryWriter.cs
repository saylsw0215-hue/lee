using System;using System.Globalization;using System.IO;using HeroDefense.Battle.Statistics;using HeroDefense.Build;using HeroDefense.Core;using HeroDefense.Logging;using UnityEngine;
namespace HeroDefense.QA
{
    /// <summary>Writes anonymous, local-only QA balance rows in development/internal builds.</summary>
    public static class BalanceTelemetryWriter
    {
        public static void Record(bool won,BattleStatistics stats,int baseHp,int permanentCoin)
        {
            if(stats==null||BuildEnvironmentService.BuildType>HeroDefenseBuildType.InternalTest)return;try{string path=Path.Combine(Application.persistentDataPath,"qa_balance.csv");bool header=!File.Exists(path);using var writer=new StreamWriter(path,true);if(header)writer.WriteLine("utc,stage,difficulty,mode,hero,won,wave,seconds,base_hp,kills,hero_damage,buildings,units,upgrades,coin");writer.WriteLine(string.Join(",",DateTime.UtcNow.ToString("O"),BattleLaunchConfig.SelectedStageId,BattleLaunchConfig.Difficulty,BattleLaunchConfig.Mode,stats.SelectedHeroId,won,stats.ReachedWave,stats.PlayTime.ToString("0.0",CultureInfo.InvariantCulture),baseHp,stats.TotalKills,stats.HeroDamageDealt.ToString("0.0",CultureInfo.InvariantCulture),stats.InstalledBuildings,stats.ProducedAllies,stats.UpgradeSelectionCount,permanentCoin));}catch(Exception exception){GameLogger.Log(GameLogLevel.Warning,LogCategory.Performance,"Local balance telemetry write failed: "+exception.GetType().Name);}
        }
    }
}
