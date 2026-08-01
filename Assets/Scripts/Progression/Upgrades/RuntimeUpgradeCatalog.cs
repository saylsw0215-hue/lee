using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Progression
{
    /// <summary>Forty-one Phase 7 cards, built safely at runtime because this project bypasses a damaged Resources archive.</summary>
    public static class RuntimeUpgradeCatalog
    {
        private static List<UpgradeData> items;public static IReadOnlyList<UpgradeData> All=>items??=Build();
        private static UpgradeEffectData E(UpgradeEffectKind kind,float value)=>new(kind,value);
        private static UpgradeData U(string id,string name,string text,UpgradeCategory category,UpgradeRarity rarity,int max,UpgradeEffectData effect,string hero=null,int level=1,string prerequisite=null,int prerequisiteLevel=0)
        {var d=ScriptableObject.CreateInstance<UpgradeData>();d.name=id;d.hideFlags=HideFlags.DontSave;d.Configure(id,name,text,category,rarity,max,1,new[]{effect},hero,new UpgradeRequirementData(level,prerequisite,prerequisiteLevel));return d;}
        private static List<UpgradeData> Build()=>new()
        {
            U("upgrade_global_attack","공격 훈련","모든 아군 공격력 +8%",UpgradeCategory.Global,UpgradeRarity.Common,5,E(UpgradeEffectKind.AttackPercent,.08f)),
            U("upgrade_global_defense","강철 갑옷","모든 아군 물리 방어력 +15",UpgradeCategory.Global,UpgradeRarity.Common,5,E(UpgradeEffectKind.DefenseFlat,15)),
            U("upgrade_global_magic_defense","마법 저항 훈련","모든 아군 마법 방어력 +15",UpgradeCategory.Global,UpgradeRarity.Common,5,E(UpgradeEffectKind.MagicDefenseFlat,15)),
            U("upgrade_global_attack_speed","신속한 손놀림","모든 아군 공격속도 +8%",UpgradeCategory.Global,UpgradeRarity.Rare,4,E(UpgradeEffectKind.AttackSpeedPercent,.08f)),
            U("upgrade_global_critical","치명적인 훈련","모든 아군 치명타 확률 +5%",UpgradeCategory.Global,UpgradeRarity.Rare,4,E(UpgradeEffectKind.CriticalFlat,.05f)),
            U("upgrade_global_armor_penetration","약점 분석","모든 아군 방어력 관통 +10",UpgradeCategory.Global,UpgradeRarity.Rare,4,E(UpgradeEffectKind.ArmorPenetrationFlat,10)),
            U("upgrade_hero_max_health","영웅의 활력","영웅 최대 체력 +20%",UpgradeCategory.Hero,UpgradeRarity.Common,4,E(UpgradeEffectKind.HeroHealthPercent,.2f)),
            U("upgrade_hero_attack","영웅의 힘","영웅 공격력 +15%",UpgradeCategory.Hero,UpgradeRarity.Common,5,E(UpgradeEffectKind.HeroAttackPercent,.15f)),
            U("upgrade_hero_respawn","빠른 재기","영웅 부활시간 15% 감소",UpgradeCategory.Hero,UpgradeRarity.Rare,3,E(UpgradeEffectKind.Special,.15f)),
            U("upgrade_hero_ultimate_gain","궁극의 흐름","궁극기 에너지 획득량 +20%",UpgradeCategory.HeroUltimate,UpgradeRarity.Rare,3,E(UpgradeEffectKind.Special,.2f)),
            U("upgrade_hero_skill_cooldown","전투 집중","액티브 스킬 쿨타임 10% 감소",UpgradeCategory.HeroSkill,UpgradeRarity.Rare,4,E(UpgradeEffectKind.Special,.1f)),
            U("upgrade_arden_shield_bash_damage","강화 방패","방패 강타 피해 +35%",UpgradeCategory.HeroSkill,UpgradeRarity.Common,3,E(UpgradeEffectKind.SkillPower,.35f),"hero_arden_knight"),
            U("upgrade_arden_shield_bash_area","충격 확산","방패 강타 범위 +25%",UpgradeCategory.HeroSkill,UpgradeRarity.Rare,2,E(UpgradeEffectKind.Special,.25f),"hero_arden_knight"),
            U("upgrade_arden_damage_reduction","굳건한 의지","아르덴이 받는 피해 10% 감소",UpgradeCategory.Hero,UpgradeRarity.Epic,2,E(UpgradeEffectKind.Special,.1f),"hero_arden_knight"),
            U("upgrade_arden_ultimate_upgrade","불굴의 수호자","궁극기 보호막 +50%, 지속시간 +2초",UpgradeCategory.HeroUltimate,UpgradeRarity.Legendary,1,E(UpgradeEffectKind.Special,.5f),"hero_arden_knight",10),
            U("upgrade_rian_multishot","다중 사격","추가 적을 60% 피해로 공격",UpgradeCategory.Hero,UpgradeRarity.Epic,2,E(UpgradeEffectKind.Special,.6f),"hero_rian_ranger"),
            U("upgrade_rian_arrow_rain_damage","폭풍 화살","화살비 피해 +25%",UpgradeCategory.HeroSkill,UpgradeRarity.Common,4,E(UpgradeEffectKind.SkillPower,.25f),"hero_rian_ranger"),
            U("upgrade_rian_critical","사냥꾼의 감각","치명타 확률 +10%",UpgradeCategory.Hero,UpgradeRarity.Rare,3,E(UpgradeEffectKind.CriticalFlat,.1f),"hero_rian_ranger"),
            U("upgrade_rian_arrow_rain_duration","끝없는 화살비","화살비 Tick +3",UpgradeCategory.HeroSkill,UpgradeRarity.Legendary,1,E(UpgradeEffectKind.Special,3),"hero_rian_ranger",1,"upgrade_rian_arrow_rain_damage",2),
            U("upgrade_sera_burn_damage","뜨거운 불씨","화상 피해 +30%",UpgradeCategory.HeroSkill,UpgradeRarity.Common,4,E(UpgradeEffectKind.SkillPower,.3f),"hero_sera_fire_mage"),
            U("upgrade_sera_explosion_radius","확산 화염","화염 폭발 범위 +20%",UpgradeCategory.HeroSkill,UpgradeRarity.Rare,3,E(UpgradeEffectKind.Special,.2f),"hero_sera_fire_mage"),
            U("upgrade_sera_burn_spread","불타는 전염","화상 적 사망 시 주변 전파",UpgradeCategory.HeroSkill,UpgradeRarity.Epic,2,E(UpgradeEffectKind.Special,2),"hero_sera_fire_mage"),
            U("upgrade_sera_meteor_upgrade","종말의 운석","메테오 중심 피해 +100%",UpgradeCategory.HeroUltimate,UpgradeRarity.Legendary,1,E(UpgradeEffectKind.SkillPower,1),"hero_sera_fire_mage",10),
            U("upgrade_swordsman","검사의 맹세","검사 체력 +20%, 방어력 +10",UpgradeCategory.Unit,UpgradeRarity.Common,4,E(UpgradeEffectKind.DefenseFlat,10)),
            U("upgrade_archer","정밀 사격","궁수 공격력 +15%",UpgradeCategory.Unit,UpgradeRarity.Common,4,E(UpgradeEffectKind.AttackPercent,.15f)),
            U("upgrade_mage","마력 증폭","마법사 공격력 +18%",UpgradeCategory.Unit,UpgradeRarity.Common,4,E(UpgradeEffectKind.AttackPercent,.18f)),
            U("upgrade_all_units","베테랑 병력","일반 아군 공격력 +10%",UpgradeCategory.Unit,UpgradeRarity.Epic,3,E(UpgradeEffectKind.AttackPercent,.1f)),
            U("upgrade_building_production_speed","신속 생산","모든 건물 생산시간 12% 감소",UpgradeCategory.Building,UpgradeRarity.Common,5,E(UpgradeEffectKind.ProductionSpeed,.12f)),
            U("upgrade_building_initial_progress","생산 준비","첫 생산 진행도 50%",UpgradeCategory.Building,UpgradeRarity.Rare,1,E(UpgradeEffectKind.Special,.5f)),
            U("upgrade_building_double_spawn","생산의 대가","확률적으로 유닛 추가 생산",UpgradeCategory.Building,UpgradeRarity.Epic,3,E(UpgradeEffectKind.Special,.15f)),
            U("upgrade_building_specialization","전문 생산 체계","주력 건물 생산속도 +25%",UpgradeCategory.Building,UpgradeRarity.Epic,1,E(UpgradeEffectKind.ProductionSpeed,.25f)),
            U("upgrade_gold_kill_reward","전리품 수집","몬스터 처치 골드 +20%",UpgradeCategory.Economy,UpgradeRarity.Common,5,E(UpgradeEffectKind.KillGold,.2f)),
            U("upgrade_gold_wave_reward","웨이브 보너스","웨이브 클리어 골드 +25%",UpgradeCategory.Economy,UpgradeRarity.Common,4,E(UpgradeEffectKind.WaveGold,.25f)),
            U("upgrade_building_cost","건축 절약","건설 비용 10% 감소",UpgradeCategory.Economy,UpgradeRarity.Rare,3,E(UpgradeEffectKind.BuildingCost,.1f)),
            U("upgrade_building_sell_refund","효율적인 철거","판매 환급률 +15%p",UpgradeCategory.Economy,UpgradeRarity.Rare,2,E(UpgradeEffectKind.Special,.15f)),
            U("upgrade_base_max_health","성벽 보강","본진 최대 체력 +25%",UpgradeCategory.Base,UpgradeRarity.Common,4,E(UpgradeEffectKind.BaseHealthPercent,.25f)),
            U("upgrade_base_heal","긴급 수리","본진 체력 30% 즉시 회복",UpgradeCategory.Base,UpgradeRarity.Rare,3,E(UpgradeEffectKind.BaseHeal,.3f)),
            U("upgrade_base_shield","방어 결계","웨이브 시작 시 본진 보호막",UpgradeCategory.Base,UpgradeRarity.Epic,3,E(UpgradeEffectKind.Special,.1f)),
            U("upgrade_last_stand","최후의 저항","본진 위기 시 공격력과 공격속도 증가",UpgradeCategory.Special,UpgradeRarity.Legendary,1,E(UpgradeEffectKind.Special,.4f)),
            U("upgrade_hero_instant_revive","불사조의 깃털","영웅 사망 시 한 번 즉시 부활",UpgradeCategory.Special,UpgradeRarity.Legendary,1,E(UpgradeEffectKind.Special,1)),
            U("upgrade_hero_aura","전장의 지휘관","영웅 주변 아군 공격력 +20%",UpgradeCategory.Special,UpgradeRarity.Epic,2,E(UpgradeEffectKind.AttackPercent,.2f))
        };
    }
}
