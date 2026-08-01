using System.IO;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Stages;
using HeroDefense.Battle.Waves;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotently authors elite, boss, ten waves, and the first stage.</summary>
    public static class Phase4Setup
    {
        private const string UnitFolder="Assets/Resources/UnitData",WaveFolder="Assets/Resources/WaveData",StageFolder="Assets/Resources/StageData";
        [MenuItem("Tools/Hero Defense/Setup Phase 4")]
        public static void Setup()
        {
            Phase3Setup.Setup();Directory.CreateDirectory(WaveFolder);Directory.CreateDirectory(StageFolder);Directory.CreateDirectory("Assets/Prefabs/Combat/Elite");Directory.CreateDirectory("Assets/Prefabs/Combat/Boss");Directory.CreateDirectory("Assets/Prefabs/UI/Waves");Directory.CreateDirectory("Assets/Prefabs/UI/Results");
            UnitData slime=Unit("EnemySlime"),goblin=Unit("EnemyGoblin");
            UnitData eliteSlime=CreateUnit("EnemyEliteSlime","enemy_elite_slime","정예 슬라임",240,1.3f,18,1.2f,1.4f,8,.65f,40,new Color(.18f,.72f,.68f),UnitVisualShape.EliteSlime);
            UnitData eliteGoblin=CreateUnit("EnemyEliteGoblin","enemy_elite_goblin","정예 고블린",280,2f,24,1.2f,.9f,9,.62f,50,new Color(.78f,.18f,.16f),UnitVisualShape.EliteGoblin);
            UnitData boss=CreateUnit("BossGoblinChieftain","boss_goblin_chieftain","고블린 대장",1800,1.2f,45,1.6f,1.5f,10,.9f,300,new Color(.62f,.08f,.06f),UnitVisualShape.BossGoblin);
            var waves=new WaveData[10];
            waves[0]=CreateWave(1,8,30,false,false,"초원의 첫 습격",G(slime,5,1.2f,0));
            waves[1]=CreateWave(2,7,35,false,false,"슬라임 무리",G(slime,8,1f,1));
            waves[2]=CreateWave(3,7,40,false,false,"혼합 부대",G(slime,6,1f,0),G(goblin,3,1f,1,.5f));
            waves[3]=CreateWave(4,6,45,false,false,"고블린 돌격",G(goblin,7,.9f,0));
            waves[4]=CreateWave(5,8,80,true,false,"정예 고블린 등장",G(slime,6,1f,0),G(goblin,5,.9f,1),G(eliteGoblin,1,1f,1,2f));
            waves[5]=CreateWave(6,6,55,false,false,"대규모 공세",G(slime,10,.8f,0),G(goblin,6,.8f,1));
            waves[6]=CreateWave(7,6,65,true,false,"정예 슬라임 등장",G(goblin,10,.8f,0),G(eliteSlime,2,1.2f,1,1f));
            waves[7]=CreateWave(8,5,75,false,false,"총공세",G(slime,12,.7f,0),G(goblin,10,.7f,1));
            waves[8]=CreateWave(9,8,100,true,false,"보스 전위대",G(eliteGoblin,3,1f,0),G(eliteSlime,3,1.1f,1),G(goblin,8,.8f,0,1f));
            waves[9]=CreateWave(10,12,200,true,true,"고블린 대장이 등장합니다.",G(boss,1,1f,0,1f),G(goblin,8,.8f,1),G(eliteGoblin,2,1.2f,1,2f));
            CreateStage(waves);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate();Debug.Log("Hero Defense Phase 4 setup complete. Existing wave and stage assets were preserved.");
        }
        private static UnitData Unit(string file)=>AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitFolder}/{file}.asset");
        private static UnitData CreateUnit(string file,string id,string name,float hp,float speed,float damage,float range,float interval,float detection,float radius,int reward,Color color,UnitVisualShape shape)
        {string path=$"{UnitFolder}/{file}.asset";var data=AssetDatabase.LoadAssetAtPath<UnitData>(path);if(data!=null)return data;data=ScriptableObject.CreateInstance<UnitData>();data.Configure(id,name,Team.Enemy,hp,speed,damage,range,interval,detection,radius,reward,color,shape);AssetDatabase.CreateAsset(data,path);return data;}
        private static WaveSpawnGroup G(UnitData data,int count,float interval,int point,float delay=0)=>new(data,count,delay,interval,point);
        private static WaveData CreateWave(int number,float preparation,int reward,bool elite,bool boss,string message,params WaveSpawnGroup[] groups)
        {string path=$"{WaveFolder}/Wave{number:00}.asset";var data=AssetDatabase.LoadAssetAtPath<WaveData>(path);if(data!=null)return data;data=ScriptableObject.CreateInstance<WaveData>();data.Configure($"stage01_wave_{number:00}",$"Wave {number}",preparation,2f,reward,elite,boss,message,groups);AssetDatabase.CreateAsset(data,path);return data;}
        private static void CreateStage(WaveData[] waves)
        {string path=$"{StageFolder}/Stage01Grassland.asset";if(AssetDatabase.LoadAssetAtPath<StageData>(path)!=null)return;var data=ScriptableObject.CreateInstance<StageData>();data.Configure("stage_01_grassland","초원의 관문","초원 관문을 지키고 고블린 대장을 처치하십시오.",500,100,300,"보통",new Color(.2f,.38f,.22f),waves);AssetDatabase.CreateAsset(data,path);}
        private static void Validate(){var stage=AssetDatabase.LoadAssetAtPath<StageData>($"{StageFolder}/Stage01Grassland.asset");if(stage==null)throw new System.InvalidOperationException("Stage 1 asset is missing.");if(!stage.Validate(out string reason))throw new System.InvalidOperationException($"Invalid Stage 1: {reason}");if(stage.WaveCount!=10)throw new System.InvalidOperationException("Stage 1 must contain exactly 10 waves.");}
    }
}
