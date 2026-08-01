using UnityEngine;
using HeroDefense.Core;

namespace HeroDefense.Heroes.Selection
{
    /// <summary>
    /// Builds the three prototype heroes without reading Unity Resources. This avoids a Unity 6
    /// macOS player crash caused by legacy editor-generated sub-assets in resources.assets.
    /// </summary>
    public static class RuntimeHeroCatalog
    {
        private static HeroData[] heroes;

        public static HeroData[] GetHeroes()
        {
            if(heroes==null)heroes=new[]{CreateKnight(),CreateRanger(),CreateMage(),CreateEngineer(),CreateSaint(),CreateAssassin()};
            return heroes;
        }

        public static HeroData GetDefault()=>GetHeroes()[0];

        private static HeroData CreateKnight()
        {
            var passive=Passive("passive_steel_will","강철 의지","본진 HP 50% 이하에서 받는 피해 20% 감소",HeroPassiveKind.SteelWill,.2f,new Color(.2f,.55f,1));
            var active=Skill("skill_knight_shield_bash","방패 강타","주변 최대 5명에게 180% 피해와 경직",HeroSkillKind.KnightShieldBash,8,.25f,3,2.2f,1.8f,.45f,5,0,SkillTargetingMode.Cone,new Color(.2f,.55f,1));
            var ultimate=Skill("ultimate_knight_guardian_oath","수호자의 맹세","8초 피해 감소와 주변 충격",HeroSkillKind.KnightGuardianOath,0,.35f,3,3,1.2f,8,20,100,SkillTargetingMode.Self,new Color(.15f,.7f,1));
            return Hero("hero_arden_knight","아르덴","근접 탱커","강철 의지로 본진을 지키는 기사",HeroArchetype.Knight,500,2.1f,35,1.4f,1,10,12,active,ultimate,passive,new Color(.12f,.38f,.85f),HeroVisualShape.Knight,55,30,.08f,1.6f,.03f);
        }

        private static HeroData CreateRanger()
        {
            var passive=Passive("passive_consecutive_shot","연속 사격","같은 대상 세 번째 공격 피해 50% 증가",HeroPassiveKind.ConsecutiveShot,.5f,new Color(.25f,.8f,.35f));
            var active=Skill("skill_ranger_arrow_rain","화살비","3초 동안 5회 범위 피해",HeroSkillKind.RangerArrowRain,10,.25f,12,3,.6f,3,35,0,SkillTargetingMode.Circle,new Color(.25f,.8f,.35f));
            var ultimate=Skill("ultimate_ranger_hawkeye","매의 눈","8초 공격속도·사거리·관통 강화",HeroSkillKind.RangerHawkeye,0,.25f,12,2,1,8,2,100,SkillTargetingMode.Self,new Color(.45f,1,.35f));
            return Hero("hero_rian_ranger","리안","원거리 지속 딜러","빠른 연속 사격과 화살비를 사용하는 레인저",HeroArchetype.Ranger,320,2.5f,28,6,.8f,12,10,active,ultimate,passive,new Color(.14f,.62f,.28f),HeroVisualShape.Ranger,18,20,.18f,1.75f,.12f);
        }

        private static HeroData CreateMage()
        {
            var passive=Passive("passive_ember","불씨","다섯 번째 기본 공격이 주변 추가 피해",HeroPassiveKind.Ember,.4f,new Color(1,.3f,.08f));
            var active=Skill("skill_mage_fire_explosion","화염 폭발","220% 범위 피해와 화상",HeroSkillKind.MageFireExplosion,12,.3f,12,3,2.2f,3,10,0,SkillTargetingMode.Circle,new Color(1,.28f,.05f));
            var ultimate=Skill("ultimate_mage_meteor","메테오","밀집 지역에 강력한 운석",HeroSkillKind.MageMeteor,0,.55f,14,4.5f,5,1,35,100,SkillTargetingMode.Circle,new Color(1,.12f,.02f));
            return Hero("hero_sera_fire_mage","세라","광역 폭발 딜러","화염 폭발과 메테오를 사용하는 마법사",HeroArchetype.FireMage,280,1.9f,45,5.2f,1.5f,12,14,active,ultimate,passive,new Color(.82f,.18f,.08f),HeroVisualShape.FireMage,12,45,.12f,1.65f,.05f);
        }

        private static HeroData CreateEngineer()
        {
            var passive=Passive("passive_production_automation","생산 자동화","모든 생산속도 8%, 첫 생산 진행도 20%",HeroPassiveKind.ProductionAutomation,.08f,new Color(.95f,.65f,.16f));
            var active=Skill("skill_engineer_turret","자동 포탑","전방에 임시 자동 포탑을 설치",HeroSkillKind.EngineerTurret,12,.25f,7,2,1.1f,10,1,0,SkillTargetingMode.TargetPosition,new Color(.95f,.65f,.16f));
            var ultimate=Skill("ultimate_engineer_overdrive","전면 가동","생산속도 50% 증가와 즉시 생산",HeroSkillKind.EngineerOverdrive,0,.3f,0,0,0,8,8,100,SkillTargetingMode.Self,new Color(1,.82f,.22f));
            return Hero("hero_kai_engineer","카이","기계공","포탑과 생산 가속으로 전선을 지원",HeroArchetype.Engineer,340,2,24,5,1.1f,11,12,active,ultimate,passive,new Color(.76f,.48f,.12f),HeroVisualShape.Engineer,28,35,.08f,1.6f,.06f);
        }
        private static HeroData CreateSaint()
        {
            var passive=Passive("passive_holy_grace","성스러운 은총","웨이브 완료 시 아군과 본진 회복",HeroPassiveKind.HolyGrace,.05f,new Color(.9f,.84f,.45f));
            var active=Skill("skill_saint_sanctuary","치유의 성역","범위 내 아군을 지속 회복",HeroSkillKind.SaintSanctuary,14,.3f,8,3.5f,1.5f,6,12,0,SkillTargetingMode.Circle,new Color(.9f,.9f,.5f));
            var ultimate=Skill("ultimate_saint_barrier","신성한 결계","모든 아군에게 보호막과 저항 부여",HeroSkillKind.SaintDivineBarrier,0,.4f,0,0,2,8,40,100,SkillTargetingMode.Self,new Color(1,.95f,.65f));
            return Hero("hero_elia_saint","엘리아","성녀","회복과 보호막으로 군대를 지키는 지원가",HeroArchetype.Saint,360,2,22,5.5f,1.4f,11,11,active,ultimate,passive,new Color(.82f,.76f,.38f),HeroVisualShape.Saint,22,65,.06f,1.55f,.05f);
        }
        private static HeroData CreateAssassin()
        {
            var passive=Passive("passive_executioner","처형자","낮은 체력의 적에게 추가 피해",HeroPassiveKind.Executioner,.2f,new Color(.56f,.22f,.72f));
            var active=Skill("skill_assassin_shadow_leap","그림자 도약","대상에게 도약해 강한 피해",HeroSkillKind.AssassinShadowLeap,9,.15f,9,1.2f,2.4f,1,1,0,SkillTargetingMode.TargetUnit,new Color(.62f,.25f,.82f));
            var ultimate=Skill("ultimate_assassin_death_mark","죽음의 표식","대상에게 받는 피해를 크게 증가",HeroSkillKind.AssassinDeathMark,0,.2f,10,1,1.5f,8,1,100,SkillTargetingMode.TargetUnit,new Color(.82f,.16f,.62f));
            return Hero("hero_nox_assassin","녹스","암살자","치명타와 도약으로 핵심 적을 처형",HeroArchetype.Assassin,290,3,48,1.3f,.7f,12,13,active,ultimate,passive,new Color(.38f,.12f,.55f),HeroVisualShape.Assassin,20,25,.2f,1.8f,.15f);
        }

        private static HeroData Hero(string id,string name,string role,string description,HeroArchetype archetype,float hp,float speed,float damage,float range,float interval,float detection,float respawn,HeroSkillData active,HeroSkillData ultimate,HeroPassiveData passive,Color color,HeroVisualShape shape,float defense,float magicDefense,float critical,float criticalMultiplier,float dodge)
        {
            var data=ScriptableObject.CreateInstance<HeroData>();data.name=id;data.hideFlags=HideFlags.DontSave;
            data.Configure(id,name,role,description,archetype,hp,speed,damage,range,interval,detection,respawn,active,ultimate,passive,color,shape);
            data.ConfigureAdvanced(defense,magicDefense,critical,criticalMultiplier,dodge,.05f,0,0,.1f,.1f);
            data.SetVisualAssets(RuntimeArtworkCatalog.Load($"HeroArt/{id}_portrait"),RuntimeArtworkCatalog.Load($"HeroArt/{id}_full"),null);return data;
        }

        private static HeroSkillData Skill(string id,string name,string description,HeroSkillKind kind,float cooldown,float cast,float range,float radius,float multiplier,float duration,int maxTargets,int energy,SkillTargetingMode targeting,Color color)
        {
            var data=ScriptableObject.CreateInstance<HeroSkillData>();data.name=id;data.hideFlags=HideFlags.DontSave;
            data.Configure(id,name,description,kind,cooldown,cast,range,radius,multiplier,duration,maxTargets,energy,color);data.SetTargetingMode(targeting);data.SetIcon(RuntimeArtworkCatalog.Skill(id));return data;
        }

        private static HeroPassiveData Passive(string id,string name,string description,HeroPassiveKind kind,float value,Color color)
        {
            var data=ScriptableObject.CreateInstance<HeroPassiveData>();data.name=id;data.hideFlags=HideFlags.DontSave;data.Configure(id,name,description,kind,value,color);return data;
        }
    }
}
