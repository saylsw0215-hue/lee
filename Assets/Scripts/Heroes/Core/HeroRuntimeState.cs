using UnityEngine;

namespace HeroDefense.Heroes
{
    /// <summary>UI-independent hero state, cooldown, energy, and respawn timers.</summary>
    public sealed class HeroRuntimeState
    {
        public HeroState State{get;private set;}=HeroState.Inactive;public float SkillCooldownRemaining{get;private set;}public float UltimateEnergy{get;private set;}public float RespawnRemaining{get;private set;}public float InvincibilityRemaining{get;private set;}
        public bool CanUseSkill=>State==HeroState.Alive&&SkillCooldownRemaining<=0;public bool CanUseUltimate=>State==HeroState.Alive&&UltimateEnergy>=100;
        public void Spawn(float invincibility=0){State=HeroState.Alive;RespawnRemaining=0;InvincibilityRemaining=Mathf.Max(0,invincibility);}
        public bool BeginSkill(float cooldown){if(!CanUseSkill)return false;State=HeroState.CastingSkill;SkillCooldownRemaining=Mathf.Max(0,cooldown);return true;}
        public bool BeginUltimate(){if(!CanUseUltimate)return false;State=HeroState.CastingUltimate;UltimateEnergy=0;return true;}
        public void FinishCast(){if(State==HeroState.CastingSkill||State==HeroState.CastingUltimate)State=HeroState.Alive;}
        public void AddEnergy(float amount){if(State==HeroState.Victory||State==HeroState.Defeat)return;UltimateEnergy=Mathf.Clamp(UltimateEnergy+Mathf.Max(0,amount),0,100);}
        public void Die(float respawn){if(State==HeroState.Victory||State==HeroState.Defeat)return;State=HeroState.Respawning;RespawnRemaining=Mathf.Max(0,respawn);InvincibilityRemaining=0;}
        public bool Tick(float dt){if(dt<=0)return false;if(State==HeroState.Alive){SkillCooldownRemaining=Mathf.Max(0,SkillCooldownRemaining-dt);InvincibilityRemaining=Mathf.Max(0,InvincibilityRemaining-dt);}if(State==HeroState.Respawning){RespawnRemaining=Mathf.Max(0,RespawnRemaining-dt);return RespawnRemaining<=0;}return false;}
        public void SetOutcome(bool victory)=>State=victory?HeroState.Victory:HeroState.Defeat;
        public void Reset(){State=HeroState.Inactive;SkillCooldownRemaining=UltimateEnergy=RespawnRemaining=InvincibilityRemaining=0;}
    }
}
