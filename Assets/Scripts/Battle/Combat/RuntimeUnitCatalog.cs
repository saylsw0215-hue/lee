using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    /// <summary>Player-safe unit definitions that do not depend on the corrupted Resources archive.</summary>
    public static class RuntimeUnitCatalog
    {
        private static readonly string[] Names={"PlayerSwordsman","PlayerArcher","PlayerMage","PlayerGuard","PlayerCannoneer","PlayerPriest","EnemySlime","EnemyGoblin","EnemyPoisonGoblin","EnemyShamanGoblin","EnemyChargeBoar","EnemyArmoredOrc","EnemySkeletonArcher","EnemyVampireBat","EnemyFrostSpirit","EnemyBomberGoblin","EnemyEliteSlime","EnemyEliteGoblin","EnemyEliteArmoredOrc","EnemyEliteFrostSpirit","EnemyEliteSkeletonKnight","BossGoblinChieftain","BossOrcWarlord","BossFrostQueen","BossDeathKnight"};
        private static readonly Dictionary<string,UnitData> Items=new();
        public static UnitData[] GetAll(){var result=new UnitData[Names.Length];for(int i=0;i<Names.Length;i++)result[i]=Get(Names[i]);return result;}
        public static UnitData Get(string name)
        {
            if(Items.TryGetValue(name,out UnitData existing))return existing;
            UnitData data=name switch
            {
                "PlayerSwordsman"=>Create("player_swordsman","검사",Team.Player,100,2,20,1.1f,1,8,.42f,0,new Color(.12f,.42f,.88f),UnitVisualShape.Swordsman,30,10),
                "PlayerArcher"=>Create("player_archer","궁수",Team.Player,65,1.8f,14,4.5f,1.2f,10,.4f,0,new Color(.18f,.62f,.82f),UnitVisualShape.Archer,12,12),
                "PlayerMage"=>Create("player_mage","마법사",Team.Player,55,1.6f,24,4,1.7f,10,.42f,0,new Color(.58f,.25f,.88f),UnitVisualShape.Mage,8,28),
                "EnemySlime"=>Create("enemy_slime","슬라임",Team.Enemy,60,1.6f,8,1,1.2f,7,.48f,10,new Color(.15f,.68f,.26f),UnitVisualShape.Slime,8,5),
                "EnemyGoblin"=>Create("enemy_goblin","고블린",Team.Enemy,90,2.2f,12,1,.9f,8,.43f,15,new Color(.78f,.38f,.12f),UnitVisualShape.Goblin,12,6),
                "EnemyEliteSlime"=>Create("enemy_elite_slime","정예 슬라임",Team.Enemy,240,1.3f,18,1.2f,1.4f,8,.65f,40,new Color(.18f,.72f,.68f),UnitVisualShape.EliteSlime,25,18),
                "EnemyEliteGoblin"=>Create("enemy_elite_goblin","정예 고블린",Team.Enemy,280,2,24,1.2f,.9f,9,.62f,50,new Color(.78f,.18f,.16f),UnitVisualShape.EliteGoblin,30,15),
                "BossGoblinChieftain"=>Create("boss_goblin_chieftain","고블린 대장",Team.Enemy,1800,1.2f,45,1.6f,1.5f,10,.9f,300,new Color(.62f,.08f,.06f),UnitVisualShape.BossGoblin,55,35),
                "EnemyPoisonGoblin"=>Create("enemy_poison_goblin","독 고블린",Team.Enemy,130,1.7f,12,4.5f,1.6f,9,.44f,18,new Color(.32f,.7f,.14f),UnitVisualShape.PoisonGoblin,10,18),
                "EnemyShamanGoblin"=>Create("enemy_shaman_goblin","주술 고블린",Team.Enemy,160,1.4f,18,5,2,10,.46f,25,new Color(.55f,.18f,.72f),UnitVisualShape.ShamanGoblin,14,24),
                "PlayerGuard"=>Create("player_guard","방패병",Team.Player,210,1.5f,12,1.1f,1.3f,8,.52f,0,new Color(.16f,.38f,.62f),UnitVisualShape.Guard,65,25),
                "PlayerCannoneer"=>Create("player_cannoneer","대포병",Team.Player,85,1.2f,42,6.5f,2.4f,12,.46f,0,new Color(.42f,.28f,.14f),UnitVisualShape.Cannoneer,15,10),
                "PlayerPriest"=>Create("player_priest","사제",Team.Player,70,1.7f,10,5,1.8f,10,.4f,0,new Color(.9f,.78f,.38f),UnitVisualShape.Priest,5,35),
                "EnemyChargeBoar"=>Create("enemy_charge_boar","돌격 멧돼지",Team.Enemy,150,2.8f,22,1.1f,1.4f,9,.5f,20,new Color(.55f,.27f,.12f),UnitVisualShape.ChargeBoar,25,10),
                "EnemyArmoredOrc"=>Create("enemy_armored_orc","갑옷 오크",Team.Enemy,360,1.2f,24,1.2f,1.5f,9,.58f,35,new Color(.32f,.45f,.2f),UnitVisualShape.ArmoredOrc,80,20),
                "EnemySkeletonArcher"=>Create("enemy_skeleton_archer","해골 궁수",Team.Enemy,95,1.6f,18,5.5f,1.3f,10,.42f,18,new Color(.72f,.72f,.65f),UnitVisualShape.SkeletonArcher,10,5),
                "EnemyVampireBat"=>Create("enemy_vampire_bat","흡혈 박쥐",Team.Enemy,80,3.2f,14,1,.8f,10,.36f,16,new Color(.42f,.08f,.22f),UnitVisualShape.VampireBat,4,12),
                "EnemyFrostSpirit"=>Create("enemy_frost_spirit","얼음 정령",Team.Enemy,120,1.8f,16,4.5f,1.6f,10,.44f,24,new Color(.25f,.72f,.9f),UnitVisualShape.FrostSpirit,10,40),
                "EnemyBomberGoblin"=>Create("enemy_bomber_goblin","폭탄 고블린",Team.Enemy,70,2.3f,60,.8f,2,8,.4f,20,new Color(.9f,.42f,.08f),UnitVisualShape.BomberGoblin,5,5),
                "EnemyEliteArmoredOrc"=>Create("enemy_elite_armored_orc","정예 갑옷 오크",Team.Enemy,950,1,42,1.3f,1.4f,10,.75f,100,new Color(.48f,.22f,.12f),UnitVisualShape.EliteArmoredOrc,130,45),
                "EnemyEliteFrostSpirit"=>Create("enemy_elite_frost_spirit","정예 얼음 정령",Team.Enemy,650,1.5f,30,5,1.5f,11,.68f,90,new Color(.25f,.65f,1f),UnitVisualShape.EliteFrostSpirit,35,90),
                "EnemyEliteSkeletonKnight"=>Create("enemy_elite_skeleton_knight","정예 해골 기사",Team.Enemy,800,1.7f,48,1.3f,1.2f,10,.7f,110,new Color(.55f,.55f,.62f),UnitVisualShape.EliteSkeletonKnight,75,55),
                "BossOrcWarlord"=>Create("boss_orc_warlord","오크 전쟁군주 그롬바",Team.Enemy,4000,1.1f,65,1.8f,1.4f,11,1,450,new Color(.42f,.18f,.08f),UnitVisualShape.BossOrc,110,70),
                "BossFrostQueen"=>Create("boss_frost_queen","서리 여왕 프레이나",Team.Enemy,3600,1,58,6,1.8f,12,.95f,500,new Color(.18f,.5f,.9f),UnitVisualShape.BossFrostQueen,60,120),
                "BossDeathKnight"=>Create("boss_death_knight","사령 기사 모르칸",Team.Enemy,4600,1.3f,72,1.6f,1.3f,11,1,600,new Color(.25f,.08f,.3f),UnitVisualShape.BossDeathKnight,100,100),
                _=>null
            };
            if(data!=null)Items[name]=data;else Debug.LogError("Unknown runtime UnitData: "+name);return data;
        }
        private static UnitData Create(string id,string display,Team team,float hp,float speed,float damage,float range,float interval,float detection,float radius,int reward,Color color,UnitVisualShape shape,float defense,float magicDefense)
        {
            var data=ScriptableObject.CreateInstance<UnitData>();data.name=id;data.hideFlags=HideFlags.DontSave;
            data.Configure(id,display,team,hp,speed,damage,range,interval,detection,radius,reward,color,shape);
            data.ConfigureAdvanced(defense,magicDefense,.05f,1.5f,0,0,0,0,0,0);return data;
        }
    }
}
