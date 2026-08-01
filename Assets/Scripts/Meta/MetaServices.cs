using System;
using HeroDefense.Core;
using HeroDefense.Save;
using UnityEngine;

namespace HeroDefense.Meta
{
    public enum CurrencyId{Coin,SoulGem} public enum MetaUpgradeCategory{Economy,Base,Hero,Unit,Building,Endless}
    public sealed class CurrencyWallet
    {
        private readonly SaveGameManager save;public int Coin=>save.Data.currencies.coin;public int SoulGem=>save.Data.currencies.soulGem;public CurrencyWallet(SaveGameManager manager){save=manager;}
        public bool CanAfford(int coin,int soul=0)=>coin>=0&&soul>=0&&Coin>=coin&&SoulGem>=soul;public bool TrySpend(int coin,int soul=0){if(!CanAfford(coin,soul))return false;save.Data.currencies.coin-=coin;save.Data.currencies.soulGem-=soul;save.NotifyChanged();return true;}public void Add(int coin,int soul=0){save.Data.currencies.coin=Mathf.Max(0,Coin+coin);save.Data.currencies.soulGem=Mathf.Max(0,SoulGem+soul);save.Data.statistics.coinEarned+=Mathf.Max(0,coin);save.Data.statistics.soulGemEarned+=Mathf.Max(0,soul);save.NotifyChanged();}
    }
    public static class SaveRecords
    {
        public static HeroProgressRecord Hero(GameSaveData data,string id){for(int i=0;i<data.heroes.Count;i++)if(data.heroes[i].heroId==id)return data.heroes[i];return null;}public static StageCompletionRecord Stage(GameSaveData data,string id){for(int i=0;i<data.stages.Count;i++)if(data.stages[i].stageId==id)return data.stages[i];return null;}public static int Upgrade(GameSaveData data,string id){for(int i=0;i<data.metaUpgrades.Count;i++)if(data.metaUpgrades[i].id==id)return data.metaUpgrades[i].value;return 0;}public static StringIntRecord UpgradeRecord(GameSaveData data,string id){for(int i=0;i<data.metaUpgrades.Count;i++)if(data.metaUpgrades[i].id==id)return data.metaUpgrades[i];var value=new StringIntRecord(id,0);data.metaUpgrades.Add(value);return value;}
    }
    public sealed class HeroUnlockService
    {
        private readonly SaveGameManager save;private readonly CurrencyWallet wallet;public HeroUnlockService(SaveGameManager value){save=value;wallet=new(value);}public bool IsUnlocked(string id)=>SaveRecords.Hero(save.Data,id)?.unlocked==true;
        public string Requirement(string id)=>id switch{"hero_kai_engineer"=>"초원의 관문 보통 클리어 + 500 Coin","hero_elia_saint"=>"붉은 협곡 보통 클리어 + 800 Coin + Soul Gem 10","hero_nox_assassin"=>"얼어붙은 성채 보통 클리어 + 1200 Coin + Soul Gem 20",_=>"기본 해금"};
        public bool CanUnlock(string id){if(IsUnlocked(id))return true;(string stage,int coin,int soul)=Costs(id);return !string.IsNullOrEmpty(stage)&&SaveRecords.Stage(save.Data,stage)?.normalCleared==true&&wallet.CanAfford(coin,soul);}public bool TryUnlock(string id){var record=SaveRecords.Hero(save.Data,id);if(record==null||record.unlocked)return record!=null;(string stage,int coin,int soul)=Costs(id);if(SaveRecords.Stage(save.Data,stage)?.normalCleared!=true||!wallet.TrySpend(coin,soul))return false;record.unlocked=true;save.NotifyChanged(SaveReason.ProgressChanged);return true;}
        private static (string,int,int) Costs(string id)=>id switch{"hero_kai_engineer"=>("stage_01_grassland",500,0),"hero_elia_saint"=>("stage_02_red_canyon",800,10),"hero_nox_assassin"=>("stage_03_frozen_fortress",1200,20),_=>(null,0,0)};
    }
    public static class MetaUpgradeCatalog
    {
        public readonly struct Definition{public readonly string Id,Name;public readonly MetaUpgradeCategory Category;public readonly int MaxLevel,BaseCost;public readonly float PerLevel;public Definition(string id,string name,MetaUpgradeCategory category,int max,int cost,float effect){Id=id;Name=name;Category=category;MaxLevel=max;BaseCost=cost;PerLevel=effect;}}
        public static readonly Definition[] All={new("meta_starting_gold","시작 자금",MetaUpgradeCategory.Economy,10,100,20),new("meta_kill_gold","전리품 연구",MetaUpgradeCategory.Economy,10,120,.02f),new("meta_stage_reward","전투 보상",MetaUpgradeCategory.Economy,10,150,.03f),new("meta_base_health","성벽 강화",MetaUpgradeCategory.Base,10,120,.03f),new("meta_base_shield","방어 결계",MetaUpgradeCategory.Base,5,250,.02f),new("meta_base_repair","긴급 복구",MetaUpgradeCategory.Base,5,220,.02f),new("meta_hero_health","영웅 체력",MetaUpgradeCategory.Hero,10,150,.02f),new("meta_hero_attack","영웅 공격",MetaUpgradeCategory.Hero,10,180,.02f),new("meta_hero_respawn","신속한 부활",MetaUpgradeCategory.Hero,10,160,.02f),new("meta_ultimate_gain","궁극기 연구",MetaUpgradeCategory.Hero,10,190,.02f),new("meta_unit_attack","병력 훈련",MetaUpgradeCategory.Unit,10,130,.015f),new("meta_unit_health","생존 훈련",MetaUpgradeCategory.Unit,10,130,.02f),new("meta_unit_critical","정밀 훈련",MetaUpgradeCategory.Unit,10,180,.005f),new("meta_production_speed","생산 자동화",MetaUpgradeCategory.Building,10,160,.015f),new("meta_build_cost","건축 기술",MetaUpgradeCategory.Building,10,150,.01f),new("meta_upgrade_cost","업그레이드 효율",MetaUpgradeCategory.Building,10,170,.01f),new("meta_endless_start_gold","끝없는 준비",MetaUpgradeCategory.Endless,5,240,30),new("meta_endless_xp","장기전 훈련",MetaUpgradeCategory.Endless,10,200,.03f),new("meta_boss_damage","보스 사냥꾼",MetaUpgradeCategory.Endless,10,250,.015f)};
        public static Definition Find(string id){for(int i=0;i<All.Length;i++)if(All[i].Id==id)return All[i];return default;}public static int Cost(Definition item,int level)=>Mathf.CeilToInt(item.BaseCost*Mathf.Pow(level+1,1.5f));
    }
    public sealed class MetaUpgradeService
    {
        private readonly SaveGameManager save;private readonly CurrencyWallet wallet;public MetaUpgradeService(SaveGameManager manager){save=manager;wallet=new(manager);}public int Level(string id)=>SaveRecords.Upgrade(save.Data,id);public bool TryPurchase(string id){var item=MetaUpgradeCatalog.Find(id);if(string.IsNullOrEmpty(item.Id))return false;var record=SaveRecords.UpgradeRecord(save.Data,id);if(record.value>=item.MaxLevel||!wallet.TrySpend(MetaUpgradeCatalog.Cost(item,record.value)))return false;record.value++;AchievementService.Evaluate(save);save.NotifyChanged();return true;}public float Effect(string id)=>Level(id)*MetaUpgradeCatalog.Find(id).PerLevel;
    }
}
