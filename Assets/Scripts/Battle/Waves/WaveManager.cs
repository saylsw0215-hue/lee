using System;
using System.Collections.Generic;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Stages;
using HeroDefense.Battle.Statistics;
using HeroDefense.Core;
using HeroDefense.UI.Waves;
using UnityEngine;
using HeroDefense.Save;
using HeroDefense.Meta;

namespace HeroDefense.Battle.Waves
{
    /// <summary>Runs stage timing and spawn scheduling while delegating combat, UI, and data ownership.</summary>
    public sealed class WaveManager:MonoBehaviour
    {
        public event Action<int> WaveStarted;public event Action<int> EliteWaveStarted;public event Action<int> BossWaveStarted;public event Action<int,int> WaveCleared;public event Action StageWon;public event Action StageFailed;
        public WaveRuntimeState Runtime{get;}=new();public BattleStatistics Statistics{get;}=new();public StageData Stage=>stage;
        private StageData stage;private BattleSessionState session;private PauseController pause;private BattleCombatController combat;private WaveHudView view;
        private readonly HashSet<CombatUnit> owned=new();private GroupRuntime[] groups;private float completionRemaining,announcementRemaining;private bool resolutionHandled;
        private float hudRefreshRemaining;private WaveData endlessWave;private string battleResultId;private BattleResultProgressService permanentProgress;

        private sealed class GroupRuntime{public WaveSpawnGroup Data;public int Remaining;public float Timer;public GroupRuntime(WaveSpawnGroup data){Data=data;Remaining=data.Count;Timer=data.InitialDelay;}}

        public void Initialize(RectTransform safe,BattleSessionState battleState,PauseController pauseController,BattleCombatController battleCombat)
        {
            session=battleState;pause=pauseController;combat=battleCombat;stage=BattleLaunchConfig.SelectedStage;permanentProgress=SaveGameManager.Instance!=null?new BattleResultProgressService(SaveGameManager.Instance):null;
            view=new WaveHudView(safe,SkipPreparation,Replay,()=>{pause.Resume();SceneLoader.Instance.Load(SceneNames.MainMenu);});
            combat.UnitDied+=OnUnitDied;combat.PlayerUnitProduced+=OnPlayerProduced;combat.BattleReset+=ResetStage;combat.DefeatStateChanged+=OnDefeat;
            if(stage==null){Debug.LogError("Stage 1 data is missing. Run Setup Phase 4.");enabled=false;return;}if(!stage.Validate(out string reason)){Debug.LogError($"Invalid StageData: {reason}");enabled=false;return;}ResetStage();
        }
        private void Update()=>Simulate(Time.deltaTime);
        public void Simulate(float dt)
        {
            if(dt<=0||stage==null||Runtime.State==WaveState.Failed||Runtime.State==WaveState.StageCleared)return;Statistics.Tick(dt);
            if(announcementRemaining>0){announcementRemaining-=dt;if(announcementRemaining<=0)view.HideAnnouncement();}
            hudRefreshRemaining-=dt;if(Runtime.State==WaveState.Preparing){if(Runtime.TickPreparation(dt))StartSpawning();RefreshThrottled();return;}
            if(Runtime.State==WaveState.Spawning||Runtime.State==WaveState.Fighting){TickSpawning(dt);if(Runtime.CanComplete)CompleteWave();RefreshThrottled();return;}
            if(Runtime.State==WaveState.Completed){completionRemaining-=dt;if(completionRemaining<=0)BeginWave(Runtime.WaveIndex+1);}
        }
        private void BeginWave(int index)
        {
            EndlessSession.CurrentWave=index+1;
            if(index<0)return;if(index>=stage.WaveCount&&BattleLaunchConfig.Mode!=GameMode.Endless)return;if(index==4&&SaveGameManager.Instance!=null){float repair=new MetaUpgradeService(SaveGameManager.Instance).Effect("meta_base_repair");if(repair>0)session.SetBaseHp(session.CurrentBaseHp+Mathf.RoundToInt(session.MaxBaseHp*repair));}resolutionHandled=false;WaveData wave=index<stage.WaveCount?stage.Waves[index]:CreateEndlessWave(index+1);endlessWave=wave;Runtime.Begin(index,wave);Statistics.ReachWave(index+1);groups=null;completionRemaining=0;Refresh();
        }
        private void StartSpawning()
        {
            WaveData wave=Current;groups=new GroupRuntime[wave.SpawnGroups.Length];for(int i=0;i<groups.Length;i++)groups[i]=new GroupRuntime(wave.SpawnGroups[i]);
            string title=wave.IsBossWave?$"경고!\nBOSS WAVE\n{wave.Announcement}":wave.IsEliteWave?"ELITE WAVE":$"WAVE {Runtime.WaveIndex+1}";
            view.ShowAnnouncement(title,wave.IsBossWave?new Color(1,.25f,.18f):Color.white);announcementRemaining=1.8f;WaveStarted?.Invoke(Runtime.WaveIndex+1);if(wave.IsEliteWave)EliteWaveStarted?.Invoke(Runtime.WaveIndex+1);if(wave.IsBossWave)BossWaveStarted?.Invoke(Runtime.WaveIndex+1);
        }
        private void TickSpawning(float dt)
        {
            if(groups==null)return;for(int i=0;i<groups.Length;i++){GroupRuntime group=groups[i];if(group.Remaining<=0)continue;group.Timer-=dt;if(group.Timer>0||!EnemyCapacity.CanSpawn(combat.ActiveEnemyCount))continue;
                if(combat.TrySpawnWaveEnemy(group.Data.EnemyData,group.Data.SpawnPointIndex,out CombatUnit unit)){group.Remaining--;group.Timer=group.Data.SpawnInterval;owned.Add(unit);Runtime.RegisterSpawn();if(group.Data.EnemyData.UnitId.StartsWith("boss_"))BindBoss(unit);}}
        }
        private void OnUnitDied(CombatUnit unit,DamageInfo info)
        {
            if(!owned.Remove(unit))return;Runtime.RegisterDeath();Statistics.RecordEnemy(unit.Data.UnitId,unit.Data.RewardGold);if(unit.Data.UnitId.StartsWith("boss_")){unit.Health.HealthChanged-=OnBossHealth;view.HideBoss();}
        }
        private void OnPlayerProduced(UnitData data)=>Statistics.RecordProduced();
        private void CompleteWave()
        {
            if(resolutionHandled||combat.IsDefeated)return;resolutionHandled=true;bool last=BattleLaunchConfig.Mode==GameMode.Stage&&Runtime.WaveIndex==stage.WaveCount-1;if(!Runtime.Complete(last))return;
            int reward=Mathf.RoundToInt(Current.ClearRewardGold*(HeroDefense.Progression.BattleModifierRepository.Current?.WaveGoldMultiplier??1f)*DifficultyModifiers.For(BattleLaunchConfig.Difficulty).Gold);if(Runtime.ClaimReward()){session.AddGold(reward);Statistics.RecordWaveReward(reward);}WaveCleared?.Invoke(Runtime.WaveIndex+1,reward);
            if(last){session.AddGold(stage.VictoryReward);PermanentReward permanentReward=permanentProgress?.Record(battleResultId,true,Statistics,session.CurrentBaseHp)??default;combat.MarkStageCleared();view.HideBoss();ShowResult(permanentReward);StageWon?.Invoke();}
            else{view.ShowAnnouncement($"WAVE CLEAR\n+{reward} Gold",new Color(.35f,1f,.48f));announcementRemaining=1.5f;completionRemaining=Current.CompletionDelay;}
        }
        private void BindBoss(CombatUnit boss){boss.Health.HealthChanged+=OnBossHealth;view.ShowBoss(boss.Data.DisplayName,boss.Health.CurrentHealth,boss.Health.MaxHealth);}
        private void OnBossHealth(float current,float max)=>view.ShowBoss(Current!=null&&Current.SpawnGroups.Length>0?Current.SpawnGroups[0].EnemyData.DisplayName:"BOSS",current,max);
        private void OnDefeat(bool failed){if(!failed)return;Runtime.Fail();PermanentReward reward=permanentProgress?.Record(battleResultId,false,Statistics,session.CurrentBaseHp)??default;combat.SetDefeatDetails($"도달 웨이브 {Statistics.ReachedWave} / {stage.WaveCount}\n처치 {Statistics.TotalKills}\nCoin +{reward.Coin} | 숙련도 +{reward.MasteryXp}");UnbindBosses();owned.Clear();view.HideBoss();StageFailed?.Invoke();}
        public void SkipPreparation(){if(Runtime.State!=WaveState.Preparing||combat.IsStageEnded)return;Runtime.SkipPreparation();StartSpawning();Refresh();}
        public void ForceClearCurrentWave()
        {
            if(Runtime.State!=WaveState.Spawning&&Runtime.State!=WaveState.Fighting)return;combat.ForceReturnEnemies();UnbindBosses();owned.Clear();Runtime.ForceResolve();if(Runtime.CanComplete)CompleteWave();
        }
        private void Replay(){combat.ResetBattle();}
        private void ResetStage()
        {
            UnbindBosses();owned.Clear();Statistics.Reset();battleResultId=System.Guid.NewGuid().ToString("N");Runtime.Reset();groups=null;resolutionHandled=false;completionRemaining=announcementRemaining=0;view.HideTransient();BeginWave(0);
        }
        private void UnbindBosses(){foreach(CombatUnit unit in owned)if(unit!=null&&unit.Data.UnitId.StartsWith("boss_"))unit.Health.HealthChanged-=OnBossHealth;}
        private void ShowResult(PermanentReward permanentReward)
        {
            int seconds=Mathf.RoundToInt(Statistics.PlayTime);view.ResultText.text=$"스테이지 클리어!\n{stage.DisplayName}\n\nCoin +{permanentReward.Coin} | Soul Gem +{permanentReward.SoulGem}\n영웅 숙련도 +{permanentReward.MasteryXp}\n웨이브 {stage.WaveCount} / {stage.WaveCount}\n처치 {Statistics.TotalKills}\n본진 HP {session.CurrentBaseHp} / {session.MaxBaseHp}\n플레이 {seconds/60:00}:{seconds%60:00}\n\n영웅 {Statistics.SelectedHeroId}\n영웅 피해 {Statistics.HeroDamageDealt:0} | 처치 {Statistics.HeroKillCount}";view.ResultPanel.SetActive(true);
        }
        private WaveData Current=>Runtime.WaveIndex<stage.WaveCount?stage.Waves[Mathf.Clamp(Runtime.WaveIndex,0,stage.WaveCount-1)]:endlessWave;
        private void Refresh(){view.Refresh(Runtime.WaveIndex+1,BattleLaunchConfig.Mode==GameMode.Endless?999:stage.WaveCount,Runtime,Current.IsBossWave);}
        private WaveData CreateEndlessWave(int number)
        {
            UnitData enemy;if(EndlessWaveGenerator.IsBoss(number))enemy=EndlessWaveGenerator.BossForWave(number);else if(EndlessWaveGenerator.IsElite(number))enemy=RuntimeUnitCatalog.Get(number%10==5?"EnemyEliteGoblin":"EnemyEliteArmoredOrc");else{string[] pool={"EnemySlime","EnemyGoblin","EnemyPoisonGoblin","EnemyChargeBoar","EnemyArmoredOrc","EnemySkeletonArcher","EnemyVampireBat","EnemyFrostSpirit","EnemyBomberGoblin"};enemy=RuntimeUnitCatalog.Get(pool[(number-1)%pool.Length]);}
            int count=EndlessWaveGenerator.IsBoss(number)?1:Mathf.Min(6+number/2,28);var wave=ScriptableObject.CreateInstance<WaveData>();wave.hideFlags=HideFlags.DontSave;wave.name=$"endless_wave_{number:000}";wave.Configure(wave.name,$"Endless {number}",Mathf.Max(3,7-number*.05f),1.5f,20+number*4,EndlessWaveGenerator.IsElite(number),EndlessWaveGenerator.IsBoss(number),EndlessWaveGenerator.IsBoss(number)?enemy.DisplayName+" 등장":"끝없는 방어",new[]{new WaveSpawnGroup(enemy,count,0,Mathf.Max(.5f,1.1f-number*.01f),number%2)});return wave;
        }
        private void RefreshThrottled(){if(hudRefreshRemaining>0)return;hudRefreshRemaining=.1f;Refresh();}
        private void OnDestroy(){if(combat==null)return;combat.UnitDied-=OnUnitDied;combat.PlayerUnitProduced-=OnPlayerProduced;combat.BattleReset-=ResetStage;combat.DefeatStateChanged-=OnDefeat;}
    }
}
