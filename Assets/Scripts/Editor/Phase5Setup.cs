using System.IO;
using HeroDefense.Heroes;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotently creates Phase 5 hero data, placeholder visuals, prefabs, and scene flow.</summary>
    public static class Phase5Setup
    {
        private const string HeroFolder="Assets/Resources/HeroData",SkillFolder="Assets/Resources/HeroSkills",PassiveFolder="Assets/Resources/HeroPassives";
        [MenuItem("Tools/Hero Defense/Setup Phase 5")]
        public static void Setup()
        {
            Phase4Setup.Setup();Directory.CreateDirectory(HeroFolder);Directory.CreateDirectory(SkillFolder);Directory.CreateDirectory(PassiveFolder);Directory.CreateDirectory("Assets/Art/Heroes/Portraits");Directory.CreateDirectory("Assets/Art/Heroes/FullBody");Directory.CreateDirectory("Assets/Art/Heroes/Icons");Directory.CreateDirectory("Assets/Prefabs/Heroes");Directory.CreateDirectory("Assets/Prefabs/HeroSkills");Directory.CreateDirectory("Assets/Prefabs/UI/Heroes");
            HeroPassiveData steel=Passive("SteelWill","passive_steel_will","강철 의지","본진 HP 50% 이하에서 받는 피해 20% 감소",HeroPassiveKind.SteelWill,.2f,new Color(.2f,.55f,1));
            HeroPassiveData shots=Passive("ConsecutiveShot","passive_consecutive_shot","연속 사격","같은 대상 세 번째 공격 피해 50% 증가",HeroPassiveKind.ConsecutiveShot,.5f,new Color(.25f,.8f,.35f));
            HeroPassiveData ember=Passive("Ember","passive_ember","불씨","다섯 번째 기본 공격이 주변 추가 피해",HeroPassiveKind.Ember,.4f,new Color(1,.3f,.08f));
            var knightA=Skill("KnightShieldBash","skill_knight_shield_bash","방패 강타","주변 최대 5명에게 180% 피해와 경직",HeroSkillKind.KnightShieldBash,8,.25f,3,2.2f,1.8f,.45f,5,0,new Color(.2f,.55f,1));
            var knightU=Skill("KnightGuardianOath","ultimate_knight_guardian_oath","수호자의 맹세","8초 피해 감소와 주변 충격",HeroSkillKind.KnightGuardianOath,0,.35f,3,3,1.2f,8,20,100,new Color(.15f,.7f,1));
            var rangerA=Skill("RangerArrowRain","skill_ranger_arrow_rain","화살비","3초 동안 5회 범위 피해",HeroSkillKind.RangerArrowRain,10,.25f,12,3, .6f,3,35,0,new Color(.25f,.8f,.35f));
            var rangerU=Skill("RangerHawkeye","ultimate_ranger_hawkeye","매의 눈","8초 공격속도·사거리·관통 강화",HeroSkillKind.RangerHawkeye,0,.25f,12,2,1,8,2,100,new Color(.45f,1f,.35f));
            var mageA=Skill("MageFireExplosion","skill_mage_fire_explosion","화염 폭발","220% 범위 피해와 화상",HeroSkillKind.MageFireExplosion,12,.3f,12,3,2.2f,3,10,0,new Color(1,.28f,.05f));
            var mageU=Skill("MageMeteor","ultimate_mage_meteor","메테오","밀집 지역에 강력한 운석",HeroSkillKind.MageMeteor,0,.55f,14,4.5f,5,1,35,100,new Color(1,.12f,.02f));
            Hero("ArdenKnight","hero_arden_knight","아르덴","근접 탱커","강철 의지로 본진을 지키는 기사",HeroArchetype.Knight,500,2.1f,35,1.4f,1,10,12,knightA,knightU,steel,new Color(.12f,.38f,.85f),HeroVisualShape.Knight);
            Hero("RianRanger","hero_rian_ranger","리안","원거리 지속 딜러","빠른 연속 사격과 화살비를 사용하는 레인저",HeroArchetype.Ranger,320,2.5f,28,6,.8f,12,10,rangerA,rangerU,shots,new Color(.14f,.62f,.28f),HeroVisualShape.Ranger);
            Hero("SeraFireMage","hero_sera_fire_mage","세라","광역 폭발 딜러","화염 폭발과 메테오를 사용하는 마법사",HeroArchetype.FireMage,280,1.9f,45,5.2f,1.5f,12,14,mageA,mageU,ember,new Color(.82f,.18f,.08f),HeroVisualShape.FireMage);
            ClearPlayerUnsafeSpriteReferences();AssetDatabase.SaveAssets();AssetDatabase.Refresh();Phase1Setup.ApplyBuildScenes();Validate();Debug.Log("Hero Defense Phase 5 setup complete. Existing hero assets were preserved.");
        }
        /// <summary>
        /// The original placeholder sprites were created as embedded Texture2D/Sprite .asset files.
        /// Unity 6 can import them in the Editor but its macOS player resource reader can trap while
        /// deserializing those references. Runtime UI already has colour/shape fallbacks, so keep the
        /// data and remove only the unsafe optional sprite references.
        /// </summary>
        private static void ClearPlayerUnsafeSpriteReferences()
        {
            string[] folders={HeroFolder,SkillFolder,PassiveFolder};
            string[] guids=AssetDatabase.FindAssets("t:ScriptableObject",folders);
            for(int i=0;i<guids.Length;i++)
            {
                string path=AssetDatabase.GUIDToAssetPath(guids[i]);
                Object asset=AssetDatabase.LoadMainAssetAtPath(path);if(asset==null)continue;
                var serialized=new SerializedObject(asset);bool changed=false;
                string[] spriteProperties={"portrait","fullBodyImage","icon"};
                for(int p=0;p<spriteProperties.Length;p++)
                {
                    SerializedProperty property=serialized.FindProperty(spriteProperties[p]);
                    if(property==null||property.objectReferenceValue==null)continue;
                    property.objectReferenceValue=null;changed=true;
                }
                if(changed){serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);}
            }
        }
        private static HeroPassiveData Passive(string file,string id,string name,string description,HeroPassiveKind kind,float value,Color color){string path=$"{PassiveFolder}/{file}.asset";var data=AssetDatabase.LoadAssetAtPath<HeroPassiveData>(path);if(data!=null)return data;data=ScriptableObject.CreateInstance<HeroPassiveData>();data.Configure(id,name,description,kind,value,color);data.SetIcon(CreateSprite($"Assets/Art/Heroes/Icons/{file}.asset",color));AssetDatabase.CreateAsset(data,path);return data;}
        private static HeroSkillData Skill(string file,string id,string name,string description,HeroSkillKind kind,float cooldown,float cast,float range,float radius,float multiplier,float duration,int max,int energy,Color color){string path=$"{SkillFolder}/{file}.asset";var data=AssetDatabase.LoadAssetAtPath<HeroSkillData>(path);if(data!=null)return data;data=ScriptableObject.CreateInstance<HeroSkillData>();data.Configure(id,name,description,kind,cooldown,cast,range,radius,multiplier,duration,max,energy,color);data.SetIcon(CreateSprite($"Assets/Art/Heroes/Icons/{file}.asset",color));AssetDatabase.CreateAsset(data,path);return data;}
        private static void Hero(string file,string id,string name,string role,string description,HeroArchetype archetype,float hp,float speed,float damage,float range,float interval,float detection,float respawn,HeroSkillData active,HeroSkillData ultimate,HeroPassiveData passive,Color color,HeroVisualShape shape)
        {string path=$"{HeroFolder}/{file}.asset";if(AssetDatabase.LoadAssetAtPath<HeroData>(path)!=null)return;var data=ScriptableObject.CreateInstance<HeroData>();data.Configure(id,name,role,description,archetype,hp,speed,damage,range,interval,detection,respawn,active,ultimate,passive,color,shape);Sprite portrait=CreateSprite($"Assets/Art/Heroes/Portraits/{file}.asset",color);Sprite full=CreateSprite($"Assets/Art/Heroes/FullBody/{file}.asset",Color.Lerp(color,Color.white,.18f));GameObject prefab=CreatePrefab(file,color);data.SetVisualAssets(portrait,full,prefab);AssetDatabase.CreateAsset(data,path);}
        private static Sprite CreateSprite(string path,Color color){Sprite existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing!=null)return existing;var texture=new Texture2D(16,16){name=Path.GetFileNameWithoutExtension(path)+"Texture"};var pixels=new Color[256];for(int i=0;i<pixels.Length;i++)pixels[i]=color;texture.SetPixels(pixels);texture.Apply();AssetDatabase.CreateAsset(texture,path);var sprite=Sprite.Create(texture,new Rect(0,0,16,16),new Vector2(.5f,.5f),16);sprite.name=Path.GetFileNameWithoutExtension(path)+"Sprite";AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;}
        private static GameObject CreatePrefab(string file,Color color){string path=$"Assets/Prefabs/Heroes/{file}.prefab";var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(existing!=null)return existing;var go=new GameObject(file+"Placeholder",typeof(RectTransform));var result=PrefabUtility.SaveAsPrefabAsset(go,path);Object.DestroyImmediate(go);return result;}
        private static void Validate(){HeroData[] heroes=Resources.LoadAll<HeroData>("HeroData");if(heroes.Length<3)throw new System.InvalidOperationException("At least three HeroData assets are required.");for(int i=0;i<heroes.Length;i++)if(!heroes[i].Validate(out string reason))throw new System.InvalidOperationException($"Invalid hero {heroes[i].name}: {reason}");if(!File.Exists(Phase1Setup.ScenePath(HeroDefense.Core.SceneNames.HeroSelect)))throw new System.InvalidOperationException("HeroSelect scene is missing.");}
    }
}
