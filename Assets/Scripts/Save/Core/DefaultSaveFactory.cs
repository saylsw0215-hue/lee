using System;
using HeroDefense.Core;

namespace HeroDefense.Save
{
    public static class DefaultSaveFactory
    {
        public static GameSaveData Create()
        {
            string now=DateTime.UtcNow.ToString("O");var data=new GameSaveData{saveId=Guid.NewGuid().ToString("N"),createdAtUtc=now,lastSavedAtUtc=now};
            data.profile.firstPlayedAtUtc=now;data.profile.lastPlayedAtUtc=now;data.currencies.coin=0;data.currencies.soulGem=0;
            foreach(var hero in GameContentDatabase.Heroes)data.heroes.Add(new HeroProgressRecord{heroId=hero.HeroId,unlocked=hero.HeroId=="hero_arden_knight"||hero.HeroId=="hero_rian_ranger"||hero.HeroId=="hero_sera_fire_mage"});
            data.collection.Add(new ContentRecord{id="hero_arden_knight",discovered=true,firstSeenUtc=now});data.collection.Add(new ContentRecord{id="hero_rian_ranger",discovered=true,firstSeenUtc=now});data.collection.Add(new ContentRecord{id="hero_sera_fire_mage",discovered=true,firstSeenUtc=now});data.collection.Add(new ContentRecord{id="stage_01_grassland",discovered=true,firstSeenUtc=now});
            int index=0;foreach(var stage in GameContentDatabase.Stages)data.stages.Add(new StageCompletionRecord{stageId=stage.StageId,unlocked=index++==0});return data;
        }
    }
}
