using HeroDefense.Core;
using HeroDefense.Save;

namespace HeroDefense.Meta
{
    /// <summary>Read-only snapshot access for permanent research; never changes authored content.</summary>
    public static class MetaRuntimeModifierProvider
    {
        private static float Effect(string id)=>SaveGameManager.Instance==null?0:new MetaUpgradeService(SaveGameManager.Instance).Effect(id);
        public static float UnitAttackMultiplier=>1+Effect("meta_unit_attack");public static float UnitHealthMultiplier=>1+Effect("meta_unit_health");public static float UnitCriticalBonus=>Effect("meta_unit_critical");
        public static float HeroAttackMultiplier=>1+Effect("meta_hero_attack");public static float HeroHealthMultiplier=>1+Effect("meta_hero_health");public static float HeroRespawnMultiplier=>1-Effect("meta_hero_respawn");public static float UltimateGainMultiplier=>1+Effect("meta_ultimate_gain");
        public static float ProductionIntervalMultiplier=>1/(1+Effect("meta_production_speed"));public static float BuildCostMultiplier=>1-Effect("meta_build_cost");public static float UpgradeCostMultiplier=>1-Effect("meta_upgrade_cost");public static float KillGoldMultiplier=>1+Effect("meta_kill_gold");public static float EndlessExperienceMultiplier=>BattleLaunchConfig.Mode==GameMode.Endless?1+Effect("meta_endless_xp"):1;public static float BossDamageMultiplier=>1+Effect("meta_boss_damage");
    }
}
