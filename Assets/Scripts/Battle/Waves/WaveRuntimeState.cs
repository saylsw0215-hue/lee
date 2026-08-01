using UnityEngine;

namespace HeroDefense.Battle.Waves
{
    /// <summary>Deterministic state machine for one stage wave, independent of spawning and UI.</summary>
    public sealed class WaveRuntimeState
    {
        public WaveState State{get;private set;}=WaveState.NotStarted;public int WaveIndex{get;private set;}=-1;public float PreparationRemaining{get;private set;}public int RemainingToSpawn{get;private set;}public int AliveTracked{get;private set;}public bool RewardClaimed{get;private set;}
        public void Begin(int index,WaveData data){WaveIndex=index;PreparationRemaining=data.PreparationDuration;RemainingToSpawn=data.TotalEnemyCount;AliveTracked=0;RewardClaimed=false;State=WaveState.Preparing;}
        public bool TickPreparation(float dt){if(State!=WaveState.Preparing)return false;PreparationRemaining=Mathf.Max(0,PreparationRemaining-Mathf.Max(0,dt));if(PreparationRemaining>0)return false;State=WaveState.Spawning;return true;}
        public void SkipPreparation(){if(State==WaveState.Preparing){PreparationRemaining=0;State=WaveState.Spawning;}}
        public void RegisterSpawn(){if(State!=WaveState.Spawning&&State!=WaveState.Fighting)return;RemainingToSpawn=Mathf.Max(0,RemainingToSpawn-1);AliveTracked++;if(RemainingToSpawn==0)State=WaveState.Fighting;}
        public void RegisterDeath(){AliveTracked=Mathf.Max(0,AliveTracked-1);}
        public bool CanComplete=>State==WaveState.Fighting&&RemainingToSpawn==0&&AliveTracked==0;
        public bool Complete(bool last){if(!CanComplete)return false;State=last?WaveState.StageCleared:WaveState.Completed;return true;}
        public bool ClaimReward(){if(RewardClaimed||State!=WaveState.Completed&&State!=WaveState.StageCleared)return false;RewardClaimed=true;return true;}
        public void Fail(){if(State!=WaveState.StageCleared)State=WaveState.Failed;}
        public void ForceResolve(){if(State==WaveState.Spawning||State==WaveState.Fighting){RemainingToSpawn=0;AliveTracked=0;State=WaveState.Fighting;}}
        public void Reset(){State=WaveState.NotStarted;WaveIndex=-1;PreparationRemaining=0;RemainingToSpawn=0;AliveTracked=0;RewardClaimed=false;}
    }
}
