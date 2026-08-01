using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Core
{
    /// <summary>Loads texture-backed runtime sprites without requiring editor-authored Sprite sub-assets.</summary>
    public static class RuntimeArtworkCatalog
    {
        private static readonly Dictionary<string,Sprite> Sprites=new();
        public static Sprite Load(string resourcesPath)
        {
            if(Sprites.TryGetValue(resourcesPath,out Sprite sprite)&&sprite!=null)return sprite;
            Texture2D texture=Resources.Load<Texture2D>(resourcesPath);if(texture==null)return null;
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);Sprites[resourcesPath]=sprite;return sprite;
        }
        public static Sprite Monster(string unitId)
        {
            string name=unitId switch
            {
                "enemy_slime" or "enemy_elite_slime" or "enemy_frost_spirit" or "enemy_elite_frost_spirit"=>"monster_slime",
                "enemy_goblin" or "enemy_poison_goblin" or "enemy_shaman_goblin" or "enemy_bomber_goblin" or "enemy_elite_goblin"=>"monster_goblin",
                "enemy_skeleton_archer" or "enemy_elite_skeleton_knight"=>"monster_skeleton",
                "enemy_armored_orc" or "enemy_elite_armored_orc" or "enemy_charge_boar" or "boss_orc_warlord"=>"monster_orc",
                "boss_death_knight" or "boss_frost_queen" or "boss_goblin_chieftain"=>"monster_dark_knight",
                "enemy_vampire_bat"=>"monster_dragon",
                _=>null
            };return name==null?null:Load("MonsterArt/"+name);
        }
        public static Sprite Unit(string unitId)
        {
            string hero=unitId switch{"player_swordsman" or "player_guard"=>"hero_arden_knight","player_archer"=>"hero_rian_ranger","player_mage"=>"hero_sera_fire_mage","player_priest"=>"hero_elia_saint","player_cannoneer"=>"hero_kai_engineer",_=>null};
            return hero!=null?Load("HeroArt/"+hero+"_full"):Monster(unitId);
        }
        public static Sprite Skill(string skillId)
        {
            string hero=skillId.Contains("knight")?"hero_arden_knight":skillId.Contains("ranger")?"hero_rian_ranger":skillId.Contains("mage")?"hero_sera_fire_mage":skillId.Contains("saint")?"hero_elia_saint":skillId.Contains("engineer")?"hero_kai_engineer":null;
            if(hero!=null)return Load($"SkillArt/{hero}_{(skillId.StartsWith("ultimate_")?"ultimate":"active")}");
            string legacy=skillId.Contains("assassin")?"skill_assassin":null;return legacy==null?null:Load("SkillArt/"+legacy);
        }
        public static Sprite HeroPoster(string heroId)=>Load("HeroPosters/"+heroId);
        public static Sprite HeroEffect(string heroId)=>Load("HeroEffects/"+heroId);
        public static Sprite HeroEmote(string heroId,bool happy)=>Load($"HeroEmotes/{heroId}_{(happy?"happy":"hurt")}");
    }
}
