using System;
using HeroDefense.Battle.Statistics;
using HeroDefense.Core;
using HeroDefense.Save;
using UnityEngine;

namespace HeroDefense.Meta
{
    public readonly struct PermanentReward{public readonly int Coin,SoulGem,MasteryXp;public PermanentReward(int coin,int soul,int mastery){Coin=coin;SoulGem=soul;MasteryXp=mastery;}}
    public sealed class BattleResultProgressService
    {
        private readonly SaveGameManager save;private readonly CurrencyWallet wallet;public BattleResultProgressService(SaveGameManager manager){save=manager;wallet=new(manager);}
        public PermanentReward Record(string resultId,bool won,BattleStatistics stats,int baseHp)
        {
            if(string.IsNullOrWhiteSpace(resultId)||save.Data.claimedBattleResultIds.Contains(resultId))return default;save.Data.claimedBattleResultIds.Add(resultId);var profile=save.Data.profile;profile.battles++;if(won)profile.wins++;else profile.losses++;profile.totalPlaySeconds+=Math.Max(0,stats.PlayTime);
            var stage=SaveRecords.Stage(save.Data,BattleLaunchConfig.SelectedStageId);bool first=false;if(stage!=null){stage.plays++;if(won){if(BattleLaunchConfig.Difficulty==GameDifficulty.Easy){first=!stage.easyCleared;stage.easyCleared=true;}else if(BattleLaunchConfig.Difficulty==GameDifficulty.Normal){first=!stage.normalCleared;stage.normalCleared=true;}else{first=!stage.hardCleared;stage.hardCleared=true;}stage.bestBaseHp=Math.Max(stage.bestBaseHp,baseHp);if(stage.bestTime<=0||stats.PlayTime<stage.bestTime)stage.bestTime=stats.PlayTime;UnlockNext(stage.stageId);}}
            int coin=BattleLaunchConfig.Mode==GameMode.Endless?Mathf.Max(20,stats.ReachedWave*8+stats.BossKills*40):won?StageCoin(BattleLaunchConfig.SelectedStageId,BattleLaunchConfig.Difficulty):Mathf.Max(5,stats.ReachedWave*3);float rewardBonus=1+new MetaUpgradeService(save).Effect("meta_stage_reward");coin=Mathf.RoundToInt(coin*rewardBonus);int soul=won?(first?15:5):0;int mastery=Mathf.Max(20,stats.ReachedWave*8+(won?50:0));wallet.Add(coin,soul);
            var hero=SaveRecords.Hero(save.Data,stats.SelectedHeroId);if(hero!=null){hero.uses++;if(won)hero.wins++;else hero.losses++;hero.damage+=(long)stats.HeroDamageDealt;hero.kills+=stats.HeroKillCount;hero.masteryXp+=mastery;hero.masteryLevel=Mathf.Clamp(1+hero.masteryXp/500,1,20);}
            if(BattleLaunchConfig.Mode==GameMode.Endless&&stats.ReachedWave>save.Data.endless.highestWave){save.Data.endless.highestWave=stats.ReachedWave;save.Data.endless.highestKills=stats.TotalKills;save.Data.endless.longestPlayTime=stats.PlayTime;save.Data.endless.highestBattleLevel=stats.BattleLevelReached;save.Data.endless.mostBosses=stats.BossKills;save.Data.endless.heroId=stats.SelectedHeroId;save.Data.endless.achievedAtUtc=DateTime.UtcNow.ToString("O");}
            var lifetime=save.Data.statistics;lifetime.enemyKills+=stats.NormalKills;lifetime.eliteKills+=stats.EliteKills;lifetime.bossKills+=stats.BossKills;lifetime.unitsProduced+=stats.ProducedAllies;lifetime.buildingsPlaced+=stats.InstalledBuildings;lifetime.heroDamage+=stats.HeroDamageDealt;lifetime.healing+=stats.HealingDone;lifetime.skillsUsed+=stats.HeroSkillUseCount;lifetime.ultimatesUsed+=stats.HeroUltimateUseCount;HeroDefense.QA.BalanceTelemetryWriter.Record(won,stats,baseHp,coin);EvaluateAchievements();save.NotifyChanged(SaveReason.BattleResult);return new(coin,soul,mastery);
        }
        private void UnlockNext(string id){string next=id switch{"stage_01_grassland"=>"stage_02_red_canyon","stage_02_red_canyon"=>"stage_03_frozen_fortress","stage_03_frozen_fortress"=>"stage_04_dead_sanctuary",_=>null};if(next!=null){var value=SaveRecords.Stage(save.Data,next);if(value!=null)value.unlocked=true;}}
        private static int StageCoin(string id,GameDifficulty difficulty){int basis=id switch{"stage_02_red_canyon"=>230,"stage_03_frozen_fortress"=>320,"stage_04_dead_sanctuary"=>430,_=>150};return difficulty==GameDifficulty.Easy?Mathf.RoundToInt(basis*.68f):difficulty==GameDifficulty.Hard?Mathf.RoundToInt(basis*1.45f):basis;}
        private void EvaluateAchievements(){AchievementService.Evaluate(save);}
    }

    public static class AchievementCatalog
    {
        public readonly struct Definition{public readonly string Id,Name;public readonly long Target;public readonly int Coin,Soul;public Definition(string id,string name,long target,int coin,int soul=0){Id=id;Name=name;Target=target;Coin=coin;Soul=soul;}}
        public static readonly Definition[] All={new("first_win","첫 승리",1,100),new("wins_10","숙련 지휘관",10,300,2),new("battles_100","백전노장",100,1000,5),new("kills_1000","몬스터 사냥꾼",1000,800),new("elites_100","정예 사냥꾼",100,600,5),new("bosses_10","보스 사냥꾼",10,500,10),new("stage1","초원의 수호자",1,150),new("stage2","협곡의 정복자",1,250),new("stage3","혹한을 넘어서",1,350),new("stage4","죽음을 이긴 자",1,500,5),new("perfect","완벽한 방어",1,300),new("hard_clear","어려움의 시작",1,350),new("endless10","끝없는 도전",10,200),new("endless20","한계를 넘어서",20,400),new("endless30","불굴의 지휘관",30,600,5),new("endless50","끝이 없는 전투",50,1000,10),new("build100","건축가",100,400),new("produce1000","생산 관리자",1000,600),new("six_buildings","다양한 전략",1,250),new("guard100","철벽 부대",100,300),new("arden10","기사의 길",10,300),new("kai100","기계 문명",100,400),new("elia_heal","빛의 수호자",10000,500),new("nox_boss","그림자의 주인",10,500,5),new("first_research","첫 연구",1,100),new("research50","연구자",50,700),new("collection50","수집가",50,400),new("collection100","완전한 기록",100,1000,10)};
    }
    public static class AchievementService
    {
        public static event Action<string> Completed;
        public static void Evaluate(SaveGameManager save){foreach(var definition in AchievementCatalog.All){var record=Get(save.Data,definition.Id);record.progress=Progress(save.Data,definition.Id);if(!record.completed&&record.progress>=definition.Target){record.completed=true;record.completedAtUtc=DateTime.UtcNow.ToString("O");Completed?.Invoke(definition.Name);}}}
        public static bool Claim(SaveGameManager save,string id){var definition=Find(id);var record=Get(save.Data,id);if(string.IsNullOrEmpty(definition.Id)||!record.completed||record.claimed)return false;record.claimed=true;new CurrencyWallet(save).Add(definition.Coin,definition.Soul);save.NotifyChanged();return true;}
        public static AchievementRecord Get(GameSaveData data,string id){for(int i=0;i<data.achievements.Count;i++)if(data.achievements[i].id==id)return data.achievements[i];var value=new AchievementRecord{id=id};data.achievements.Add(value);return value;}
        private static AchievementCatalog.Definition Find(string id){foreach(var value in AchievementCatalog.All)if(value.Id==id)return value;return default;}
        private static long Progress(GameSaveData d,string id)=>id switch{"first_win" or "stage1"=>d.profile.wins,"wins_10"=>d.profile.wins,"battles_100"=>d.profile.battles,"kills_1000"=>d.statistics.enemyKills,"elites_100"=>d.statistics.eliteKills,"bosses_10"=>d.statistics.bossKills,"stage2"=>SaveRecords.Stage(d,"stage_02_red_canyon")?.normalCleared==true?1:0,"stage3"=>SaveRecords.Stage(d,"stage_03_frozen_fortress")?.normalCleared==true?1:0,"stage4"=>SaveRecords.Stage(d,"stage_04_dead_sanctuary")?.normalCleared==true?1:0,"endless10" or "endless20" or "endless30" or "endless50"=>d.endless.highestWave,"build100"=>d.statistics.buildingsPlaced,"produce1000"=>d.statistics.unitsProduced,"elia_heal"=>(long)d.statistics.healing,"first_research" or "research50"=>TotalResearch(d),"collection50" or "collection100"=>CollectionPercent(d),_=>0};
        private static long TotalResearch(GameSaveData d){long total=0;for(int i=0;i<d.metaUpgrades.Count;i++)total+=d.metaUpgrades[i].value;return total;}
        private static long CollectionPercent(GameSaveData d){int found=0;for(int i=0;i<d.collection.Count;i++)if(d.collection[i].discovered)found++;int total=GameContentDatabase.Heroes.Count+GameContentDatabase.Units.Count+GameContentDatabase.Buildings.Count+GameContentDatabase.Stages.Count;return total<=0?0:found*100/total;}
    }
}
