using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Waves;
using UnityEngine;

namespace HeroDefense.Battle.Stages
{
    /// <summary>Creates Stage 1 and its waves without accessing the broken Resources archive.</summary>
    public static class RuntimeStageCatalog
    {
        private static StageData stage;private static StageData[] all;
        public static StageData[] GetAll(){if(all==null)all=new[]{GetStageOne(),CreateStage("stage_02_red_canyon","붉은 협곡","뜨거운 대지: 화염 강화, 빙결 약화",12,new Color(.48f,.22f,.1f),RuntimeUnitCatalog.Get("EnemyArmoredOrc"),RuntimeUnitCatalog.Get("EnemyChargeBoar"),RuntimeUnitCatalog.Get("EnemyEliteArmoredOrc"),RuntimeUnitCatalog.Get("BossOrcWarlord")),CreateStage("stage_03_frozen_fortress","얼어붙은 성채","혹한: 아군 이동 감소, 냉기 지속 증가",12,new Color(.16f,.38f,.55f),RuntimeUnitCatalog.Get("EnemyFrostSpirit"),RuntimeUnitCatalog.Get("EnemySkeletonArcher"),RuntimeUnitCatalog.Get("EnemyEliteFrostSpirit"),RuntimeUnitCatalog.Get("BossFrostQueen")),CreateStage("stage_04_dead_sanctuary","망자의 성역","죽음의 기운: 회복 감소와 해골 잔재",15,new Color(.2f,.12f,.28f),RuntimeUnitCatalog.Get("EnemySkeletonArcher"),RuntimeUnitCatalog.Get("EnemyVampireBat"),RuntimeUnitCatalog.Get("EnemyEliteSkeletonKnight"),RuntimeUnitCatalog.Get("BossDeathKnight"))};return all;}
        public static StageData GetById(string id){StageData[] values=GetAll();for(int i=0;i<values.Length;i++)if(values[i].StageId==id)return values[i];return null;}
        public static StageData GetStageOne()
        {
#if UNITY_EDITOR
            StageData authored=Resources.Load<StageData>("StageData/Stage01Grassland");if(authored!=null)return authored;
#endif
            if(stage!=null)return stage;
            UnitData slime=RuntimeUnitCatalog.Get("EnemySlime"),goblin=RuntimeUnitCatalog.Get("EnemyGoblin"),eliteSlime=RuntimeUnitCatalog.Get("EnemyEliteSlime"),eliteGoblin=RuntimeUnitCatalog.Get("EnemyEliteGoblin"),boss=RuntimeUnitCatalog.Get("BossGoblinChieftain");
            WaveData[] waves=
            {
                Wave(1,8,30,false,false,"초원의 첫 습격",G(slime,5,0,1.2f,0)),
                Wave(2,7,35,false,false,"슬라임 무리",G(slime,8,0,1,1)),
                Wave(3,7,40,false,false,"혼합 부대",G(slime,6,0,1,0),G(goblin,3,.5f,1,1)),
                Wave(4,6,45,false,false,"고블린 돌격",G(goblin,7,0,.9f,0)),
                Wave(5,8,80,true,false,"정예 고블린 등장",G(slime,6,0,1,0),G(goblin,5,0,.9f,1),G(eliteGoblin,1,2,1,1)),
                Wave(6,6,55,false,false,"대규모 공세",G(slime,10,0,.8f,0),G(goblin,6,0,.8f,1)),
                Wave(7,6,65,true,false,"정예 슬라임 등장",G(goblin,10,0,.8f,0),G(eliteSlime,2,1,1.2f,1)),
                Wave(8,5,75,false,false,"총공세",G(slime,12,0,.7f,0),G(goblin,10,0,.7f,1)),
                Wave(9,8,100,true,false,"보스 전위대",G(eliteGoblin,3,0,1,0),G(eliteSlime,3,0,1.1f,1),G(goblin,8,1,.8f,0)),
                Wave(10,12,200,true,true,"고블린 대장이 등장합니다.",G(boss,1,1,1,0),G(goblin,8,0,.8f,1),G(eliteGoblin,2,2,1.2f,1))
            };
            stage=ScriptableObject.CreateInstance<StageData>();stage.name="stage_01_grassland";stage.hideFlags=HideFlags.DontSave;
            stage.Configure("stage_01_grassland","초원의 관문","초원 관문을 지키고 고블린 대장을 처치하십시오.",500,100,300,"보통",new Color(.2f,.38f,.22f),waves);return stage;
        }
        private static WaveSpawnGroup G(UnitData unit,int count,float delay,float interval,int point)=>new(unit,count,delay,interval,point);
        private static WaveData Wave(int number,float preparation,int reward,bool elite,bool boss,string announcement,params WaveSpawnGroup[] groups)
        {var data=ScriptableObject.CreateInstance<WaveData>();data.name=$"stage01_wave_{number:00}";data.hideFlags=HideFlags.DontSave;data.Configure(data.name,$"Wave {number}",preparation,2,reward,elite,boss,announcement,groups);return data;}
        private static StageData CreateStage(string id,string display,string description,int count,Color color,UnitData first,UnitData second,UnitData elite,UnitData boss)
        {
            var waves=new WaveData[count];for(int i=0;i<count;i++){int number=i+1;bool isBoss=number==count;bool isElite=!isBoss&&number%5==0;UnitData featured=isBoss?boss:isElite?elite:(number%2==0?second:first);int amount=isBoss?1:Mathf.Min(4+number,16);waves[i]=Wave(number,Mathf.Max(4,8-number*.25f),25+number*10,isElite,isBoss,isBoss?boss.DisplayName+" 등장":display+" Wave "+number,G(featured,amount,0,Mathf.Max(.65f,1.2f-number*.025f),number%2));}
            var result=ScriptableObject.CreateInstance<StageData>();result.name=id;result.hideFlags=HideFlags.DontSave;result.Configure(id,display,description,500,100,300,"보통",color,waves);return result;
        }
    }
}
