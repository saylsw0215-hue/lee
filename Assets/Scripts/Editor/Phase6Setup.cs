using System.IO;
using HeroDefense.Battle.Combat;
using HeroDefense.Heroes;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotent Phase 6 migration for advanced stats, statuses, targeting, and specialist enemies.</summary>
    public static class Phase6Setup
    {
        private const string StatusFolder="Assets/Resources/StatusEffects",UnitFolder="Assets/Resources/UnitData";
        [MenuItem("Tools/Hero Defense/Setup Phase 6")]
        public static void Setup()
        {
            Phase5Setup.Setup();Directory.CreateDirectory(StatusFolder);CleanupBrokenStatusCopies();Directory.CreateDirectory("Assets/ScriptableObjects/Combat");Directory.CreateDirectory("Assets/ScriptableObjects/StatusEffects");Directory.CreateDirectory("Assets/Prefabs/CombatEffects/StatusEffects");Directory.CreateDirectory("Assets/Prefabs/CombatEffects/DamageNumbers");Directory.CreateDirectory("Assets/Prefabs/CombatEffects/Shields");Directory.CreateDirectory("Assets/Prefabs/SkillPreviews");
            Status("Stun",StatusId.Stun,"기절",StatusEffectType.CrowdControl,1.5f,1,1,1,StatusRefreshRule.RefreshDuration,new Color(1,.85f,.2f));
            Status("Freeze",StatusId.Freeze,"빙결",StatusEffectType.CrowdControl,2.5f,1,1,1,StatusRefreshRule.ReplaceIfStronger,new Color(.2f,.75f,1));
            Status("Burn",StatusId.Burn,"화상",StatusEffectType.DamageOverTime,5,1,4,3,StatusRefreshRule.Stack,new Color(1,.25f,.05f));
            Status("Poison",StatusId.Poison,"독",StatusEffectType.DamageOverTime,5,1,2,5,StatusRefreshRule.Stack,new Color(.3f,.8f,.15f));
            Status("Shock",StatusId.Shock,"감전",StatusEffectType.Debuff,4,1,.15f,1,StatusRefreshRule.RefreshDuration,new Color(.7f,.55f,1));
            Status("Slow",StatusId.Slow,"둔화",StatusEffectType.Debuff,4,1,.3f,1,StatusRefreshRule.ReplaceIfStronger,new Color(.35f,.7f,1));
            Status("Silence",StatusId.Silence,"침묵",StatusEffectType.CrowdControl,3,1,1,1,StatusRefreshRule.RefreshDuration,new Color(.6f,.25f,.75f));
            Status("Taunt",StatusId.Taunt,"도발",StatusEffectType.CrowdControl,3,1,1,1,StatusRefreshRule.RefreshDuration,new Color(1,.25f,.2f));
            Status("Invincible",StatusId.Invincible,"무적",StatusEffectType.Buff,2,1,1,1,StatusRefreshRule.RefreshDuration,new Color(.3f,.9f,1),false);
            Status("ShamanPower",StatusId.ShamanPower,"주술 강화",StatusEffectType.Buff,5,1,.15f,1,StatusRefreshRule.RefreshDuration,new Color(.8f,.25f,1),false);
            MigrateUnits();MigrateHeroes();UpdateSkills();CreateEnemy("EnemyPoisonGoblin","enemy_poison_goblin","독 고블린",130,1.7f,12,4.5f,1.6f,9,.44f,18,new Color(.32f,.7f,.14f),UnitVisualShape.PoisonGoblin,10,20,0,1.5f,.04f,0);
            CreateEnemy("EnemyShamanGoblin","enemy_shaman_goblin","주술 고블린",160,1.4f,18,5,2,10,.46f,25,new Color(.55f,.18f,.72f),UnitVisualShape.ShamanGoblin,8,35,.05f,1.5f,.03f,0);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate();Debug.Log("Hero Defense Phase 6 setup complete. Existing authored advanced values were preserved.");
        }
        private static void CleanupBrokenStatusCopies(){string[] files={"Burn 2","Freeze 2","Invincible 2","Poison 2","ShamanPower 2","Shock 2","Silence 2","Slow 2","Stun 2","Taunt 2"};for(int i=0;i<files.Length;i++){string path=$"{StatusFolder}/{files[i]}.asset";if(File.Exists(path))AssetDatabase.DeleteAsset(path);}}
        private static void Status(string file,string id,string name,StatusEffectType type,float duration,float tick,float potency,int stacks,StatusRefreshRule rule,Color color,bool resistance=true){string path=$"{StatusFolder}/{file}.asset";if(AssetDatabase.LoadAssetAtPath<StatusEffectData>(path)!=null)return;if(File.Exists(path))AssetDatabase.DeleteAsset(path);var data=ScriptableObject.CreateInstance<StatusEffectData>();data.Configure(id,name,name,type,duration,tick,potency,stacks,rule,resistance,true,color);AssetDatabase.CreateAsset(data,path);}
        private static void MigrateUnits(){Configure("PlayerSwordsman",30,10,.05f,1.5f,.02f,0,0);Configure("PlayerArcher",10,10,.12f,1.7f,.08f,0,0);Configure("PlayerMage",5,25,.08f,1.5f,.03f,0,0);Configure("EnemySlime",10,5,0,1.5f,0,0,0);Configure("EnemyGoblin",15,10,.05f,1.5f,.05f,0,0);Configure("EnemyEliteSlime",40,25,.05f,1.5f,0,.3f,.15f);Configure("EnemyEliteGoblin",50,25,.1f,1.6f,.1f,.3f,.15f);Configure("BossGoblinChieftain",80,60,.12f,1.7f,.05f,.6f,.4f);}
        private static void Configure(string file,float defense,float magic,float critical,float multiplier,float dodge,float cc,float status){var data=AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitFolder}/{file}.asset");if(data==null||data.AdvancedStats.Defense>0||data.AdvancedStats.MagicDefense>0)return;data.ConfigureAdvanced(defense,magic,critical,multiplier,dodge,0,0,0,cc,status);EditorUtility.SetDirty(data);}
        private static void MigrateHeroes(){Hero("ArdenKnight",55,30,.08f,1.6f,.03f);Hero("RianRanger",18,20,.18f,1.75f,.12f);Hero("SeraFireMage",12,45,.12f,1.65f,.05f);}
        private static void Hero(string file,float defense,float magic,float critical,float multiplier,float dodge){var data=AssetDatabase.LoadAssetAtPath<HeroData>($"Assets/Resources/HeroData/{file}.asset");if(data==null||data.AdvancedStats.Defense>0||data.AdvancedStats.MagicDefense>0)return;data.ConfigureAdvanced(defense,magic,critical,multiplier,dodge,.05f,0,0,.1f,.1f);EditorUtility.SetDirty(data);}
        private static void UpdateSkills(){Target("KnightShieldBash",SkillTargetingMode.Cone);Target("KnightGuardianOath",SkillTargetingMode.Self);Target("RangerArrowRain",SkillTargetingMode.Circle);Target("RangerHawkeye",SkillTargetingMode.Self);Target("MageFireExplosion",SkillTargetingMode.Circle);Target("MageMeteor",SkillTargetingMode.Circle);}
        private static void Target(string file,SkillTargetingMode mode){var data=AssetDatabase.LoadAssetAtPath<HeroSkillData>($"Assets/Resources/HeroSkills/{file}.asset");if(data==null)return;data.SetTargetingMode(mode);EditorUtility.SetDirty(data);}
        private static void CreateEnemy(string file,string id,string name,float hp,float speed,float damage,float range,float interval,float detection,float radius,int reward,Color color,UnitVisualShape shape,float defense,float magic,float critical,float multiplier,float dodge,float cc){string path=$"{UnitFolder}/{file}.asset";var data=AssetDatabase.LoadAssetAtPath<UnitData>(path);if(data!=null)return;data=ScriptableObject.CreateInstance<UnitData>();data.Configure(id,name,Team.Enemy,hp,speed,damage,range,interval,detection,radius,reward,color,shape);data.ConfigureAdvanced(defense,magic,critical,multiplier,dodge,0,0,0,cc,.1f);AssetDatabase.CreateAsset(data,path);}
        private static void Validate(){var effects=Resources.LoadAll<StatusEffectData>("StatusEffects");if(effects.Length<10)throw new System.InvalidOperationException("Ten Phase 6 status assets are required.");var ids=new System.Collections.Generic.HashSet<string>();for(int i=0;i<effects.Length;i++){if(!effects[i].Validate(out string reason))throw new System.InvalidOperationException(reason);if(!ids.Add(effects[i].EffectId))throw new System.InvalidOperationException("Duplicate status ID: "+effects[i].EffectId);}if(AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitFolder}/EnemyPoisonGoblin.asset")==null||AssetDatabase.LoadAssetAtPath<UnitData>($"{UnitFolder}/EnemyShamanGoblin.asset")==null)throw new System.InvalidOperationException("Phase 6 specialist enemies are missing.");}
    }
}
