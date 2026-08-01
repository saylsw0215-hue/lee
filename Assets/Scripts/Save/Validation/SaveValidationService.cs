using System;
using System.Collections.Generic;
using HeroDefense.Core;
using UnityEngine;

namespace HeroDefense.Save
{
    public static class SaveValidationService
    {
        public static bool Normalize(GameSaveData data,out string warning)
        {
            warning="";if(data==null)return false;if(data.saveVersion<=0)data.saveVersion=1;if(data.saveVersion>GameSaveData.CurrentVersion)return false;data.profile??=new();data.currencies??=new();data.heroes??=new();data.stages??=new();data.endless??=new();data.metaUpgrades??=new();data.collection??=new();data.achievements??=new();data.tutorial??=new();data.settings??=new();data.statistics??=new();data.claimedBattleResultIds??=new();
            data.currencies.coin=Mathf.Clamp(data.currencies.coin,0,999999999);data.currencies.soulGem=Mathf.Clamp(data.currencies.soulGem,0,9999999);data.endless.highestWave=Mathf.Max(0,data.endless.highestWave);data.profile.totalPlaySeconds=Math.Max(0,data.profile.totalPlaySeconds);data.settings.masterVolume=Mathf.Clamp01(data.settings.masterVolume);data.settings.musicVolume=Mathf.Clamp01(data.settings.musicVolume);data.settings.sfxVolume=Mathf.Clamp01(data.settings.sfxVolume);data.settings.textScale=Mathf.Clamp(data.settings.textScale,.85f,1.35f);data.settings.targetFrameRate=data.settings.targetFrameRate==30?30:60;
            var heroIds=new HashSet<string>();foreach(var hero in GameContentDatabase.Heroes)heroIds.Add(hero.HeroId);for(int i=data.heroes.Count-1;i>=0;i--)if(data.heroes[i]==null||!heroIds.Contains(data.heroes[i].heroId)||!heroIds.Remove(data.heroes[i].heroId))data.heroes.RemoveAt(i);foreach(string id in heroIds)data.heroes.Add(new HeroProgressRecord{heroId=id,unlocked=id=="hero_arden_knight"||id=="hero_rian_ranger"||id=="hero_sera_fire_mage"});
            var stageIds=new HashSet<string>();foreach(var stage in GameContentDatabase.Stages)stageIds.Add(stage.StageId);for(int i=data.stages.Count-1;i>=0;i--)if(data.stages[i]==null||!stageIds.Contains(data.stages[i].stageId)||!stageIds.Remove(data.stages[i].stageId))data.stages.RemoveAt(i);foreach(string id in stageIds)data.stages.Add(new StageCompletionRecord{stageId=id,unlocked=id=="stage_01_grassland"});warning="Save values were normalized where necessary.";return true;
        }
    }
}
