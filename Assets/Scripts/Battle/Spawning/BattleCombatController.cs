using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Effects;
using HeroDefense.Battle.Projectiles;
using System;
using HeroDefense.Meta;
using HeroDefense.Collection;
using HeroDefense.Build;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Core;

namespace HeroDefense.Battle
{
    /// <summary>Composes Phase 2 combat, debug spawning, reset, rewards, and defeat UI.</summary>
    public sealed class BattleCombatController
    {
        private const int MaxPlayers = 15, MaxEnemies = 20;
        private const int MaxProducedPlayers = 30;
        public event Action BattleReset;
        public event Action<bool> DefeatStateChanged;
        public event Action<CombatUnit,DamageInfo> UnitDied;
        public event Action<UnitData> PlayerUnitProduced;
        public int ActivePlayerCount=>registry.PlayerCount;
        public int ActiveEnemyCount=>registry.EnemyCount;
        public bool IsDefeated{get;private set;}
        public bool IsVictorious{get;private set;}
        public bool IsStageEnded=>IsDefeated||IsVictorious;
        public CombatRegistry Registry=>registry;public RectTransform World{get;}
        private readonly BattleSessionState state;
        private readonly PauseController pause;
        private readonly BuildingSelectionModel buildingSelection;
        private readonly CombatRegistry registry = new();
        private readonly CombatPool pool;
        private readonly UnitData swordsman, slime, goblin,poisonGoblin,shamanGoblin;
        private readonly PlayerBase playerBase;
        private readonly BattleSpawnPoint playerSpawn, enemySpawn,enemySpawnSecond;
        private readonly GameObject defeatPanel;
        private Text defeatTitle;
        private readonly Text status;
        private int playerLane, enemyLane;

        public BattleCombatController(RectTransform safe, RectTransform world, BattleSessionState session, PauseController pauseController, BuildingSelectionModel selection)
        {
            state = session; pause = pauseController; buildingSelection = selection;World=world;
            swordsman = Load("PlayerSwordsman"); slime = Load("EnemySlime"); goblin = Load("EnemyGoblin");poisonGoblin=Load("EnemyPoisonGoblin");shamanGoblin=Load("EnemyShamanGoblin");
            var effectObject = new GameObject("FloatingDamagePool", typeof(FloatingDamageTextPool)); effectObject.transform.SetParent(world, false);
            var projectileObject=new GameObject("ProjectilePool",typeof(ProjectilePool));projectileObject.transform.SetParent(world,false);
            pool = new CombatPool(world, registry, effectObject.GetComponent<FloatingDamageTextPool>(),projectileObject.GetComponent<ProjectilePool>());
            if (swordsman != null) pool.Prewarm(swordsman, 5); if (slime != null) pool.Prewarm(slime, 6); if (goblin != null) pool.Prewarm(goblin, 4);
            if(poisonGoblin!=null)pool.Prewarm(poisonGoblin,2);if(shamanGoblin!=null)pool.Prewarm(shamanGoblin,2);
            playerBase = CreateBase(world); registry.SetPlayerBase(playerBase); playerBase.Defeated += ShowDefeat;
            playerSpawn = CreateSpawnPoint(world, "PlayerSpawnPoint", Team.Player, new Vector2(-540,0));
            enemySpawn = CreateSpawnPoint(world, "EnemySpawnPoint", Team.Enemy, new Vector2(650,0));
            enemySpawnSecond = CreateSpawnPoint(world, "EnemySpawnPoint_02", Team.Enemy, new Vector2(650,115));
            status = BuildSpawnUi(safe);status.transform.parent.gameObject.SetActive(BuildEnvironmentService.DebugUiEnabled); defeatPanel = BuildDefeat(safe); defeatPanel.SetActive(false);
        }

        private PlayerBase CreateBase(Transform world)
        {
            var go = new GameObject("PlayerBase", typeof(RectTransform), typeof(PlayerBase)); go.transform.SetParent(world, false);
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(-690, 0); var result = go.GetComponent<PlayerBase>(); result.Build(state); return result;
        }
        private static BattleSpawnPoint CreateSpawnPoint(Transform world, string name, Team team, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(BattleSpawnPoint)); go.transform.SetParent(world, false);
            var point = go.GetComponent<BattleSpawnPoint>(); point.Configure(team, position); return point;
        }
        private Text BuildSpawnUi(RectTransform safe)
        {
            var panel = UI.UiFactory.Panel(safe,"Phase2Debug",new Color(.03f,.08f,.08f,.92f),new Vector2(.13f,.238f),new Vector2(.87f,.305f));
            var row = UI.UiFactory.Horizontal(panel,"SpawnButtons",10); UI.UiFactory.Stretch(row,new Vector2(.01f,.08f),new Vector2(.83f,.92f));
            UI.UiFactory.Button(row,"SpawnSwordsman","검사 소환",new Color(.12f,.35f,.65f)).onClick.AddListener(() => Spawn(swordsman));
            UI.UiFactory.Button(row,"SpawnSlime","슬라임 소환",new Color(.15f,.52f,.22f)).onClick.AddListener(() => Spawn(slime));
            UI.UiFactory.Button(row,"SpawnGoblin","고블린 소환",new Color(.62f,.32f,.12f)).onClick.AddListener(() => Spawn(goblin));
            UI.UiFactory.Button(row,"SpawnPoisonGoblin","독 고블린",new Color(.25f,.55f,.12f)).onClick.AddListener(()=>Spawn(poisonGoblin));
            UI.UiFactory.Button(row,"SpawnShamanGoblin","주술 고블린",new Color(.48f,.15f,.62f)).onClick.AddListener(()=>Spawn(shamanGoblin));
            UI.UiFactory.Button(row,"ResetCombat","전투 초기화",new Color(.42f,.18f,.18f)).onClick.AddListener(ResetBattle);
            var message = UI.UiFactory.Label(panel,"DebugStatus","개발용 소환",21,TextAnchor.MiddleCenter,new Color(.8f,.95f,1f));
            message.rectTransform.anchorMin = new Vector2(.84f,.08f); message.rectTransform.anchorMax = new Vector2(.99f,.92f); message.rectTransform.offsetMin = message.rectTransform.offsetMax = Vector2.zero;
            return message;
        }
        private GameObject BuildDefeat(RectTransform safe)
        {
            var panel = UI.UiFactory.Panel(safe,"DefeatOverlay",new Color(0,0,0,.84f),Vector2.zero,Vector2.one).gameObject;
            var column = UI.UiFactory.Vertical(panel.transform,"DefeatMenu",18); UI.UiFactory.Stretch(column,new Vector2(.34f,.22f),new Vector2(.66f,.78f));
            defeatTitle = UI.UiFactory.Label(column,"Title","패배\n본진이 파괴되었습니다.",52,TextAnchor.MiddleCenter,Color.white); defeatTitle.gameObject.AddComponent<LayoutElement>().preferredHeight=190;
            UI.UiFactory.Button(column,"Restart","다시 시작",new Color(.16f,.45f,.32f)).onClick.AddListener(ResetBattle);
            UI.UiFactory.Button(column,"MainMenu","메인 메뉴",new Color(.24f,.31f,.5f)).onClick.AddListener(() => { pause.Resume(); SceneLoader.Instance.Load(SceneNames.MainMenu); });
            return panel;
        }
        public void Spawn(UnitData data)
        {
            if (data == null || pause.IsPaused) return;
            bool player = data.Team == Team.Player; int count = player ? registry.PlayerCount : registry.EnemyCount;
            if (count >= (player ? MaxPlayers : MaxEnemies)) { status.text = player ? "아군 최대 15" : "적 최대 20"; return; }
            int lane = player ? playerLane++ : enemyLane++; float y = ((lane % 7) - 3) * 48f;
            float rowOffset = (lane / 7) * 28f;
            Vector2 origin = player ? playerSpawn.transform.localPosition : enemySpawn.transform.localPosition;
            CombatUnit unit = pool.Spawn(data, new Vector2(origin.x + (player ? -rowOffset : rowOffset), y), player ? 630 : -620);
            unit.Died += OnUnitDied; status.text = $"{data.DisplayName} 소환";
        }
        public bool TrySpawnProduced(UnitData data,Vector2 sourcePosition)
        {
            if(data==null||data.Team!=Team.Player||pause.IsPaused||IsDefeated||registry.PlayerCount>=MaxProducedPlayers)return false;
            float offset=(playerLane++%5)*18f;CombatUnit unit=pool.Spawn(data,new Vector2(sourcePosition.x+offset,sourcePosition.y-(offset*.3f)),630);
            unit.Died+=OnUnitDied;CollectionService.Record(data.UnitId,CollectionEvent.Used);PlayerUnitProduced?.Invoke(data);return true;
        }
        public bool TrySpawnWaveEnemy(UnitData data,int spawnPointIndex,out CombatUnit unit)
        {
            unit=null;if(data==null||data.Team!=Team.Enemy||pause.IsPaused||IsStageEnded||registry.EnemyCount>=35)return false;
            BattleSpawnPoint point=spawnPointIndex==1?enemySpawnSecond:enemySpawn;float offset=(enemyLane++%5)*22f;
            unit=pool.Spawn(data,new Vector2(point.transform.localPosition.x+offset,point.transform.localPosition.y-(offset*.5f)),-620);unit.Died+=OnUnitDied;CollectionService.Record(data.UnitId,CollectionEvent.Encountered);return true;
        }
        public void DamageBaseForDebug(float amount)=>playerBase.TakeDamage(new DamageInfo(amount,Team.Enemy));
        public void ForceReturnEnemies()
        {
            var active=pool.Active;for(int i=active.Count-1;i>=0;i--){CombatUnit unit=active[i];if(unit.Team!=Team.Enemy)continue;unit.Died-=OnUnitDied;unit.ReturnWithoutReward();pool.Return(unit);}
        }
        private void OnUnitDied(CombatUnit unit, DamageInfo killingBlow)
        {
            unit.Died -= OnUnitDied;
            if (unit.Team == Team.Enemy && killingBlow.SourceTeam == Team.Player){state.AddGold(Mathf.RoundToInt(unit.Data.RewardGold*(HeroDefense.Progression.BattleModifierRepository.Current?.KillGoldMultiplier??1f)*DifficultyModifiers.For(BattleLaunchConfig.Difficulty).Gold*MetaRuntimeModifierProvider.KillGoldMultiplier));CollectionService.Record(unit.Data.UnitId,CollectionEvent.Defeated);}
            UnitDied?.Invoke(unit,killingBlow);
        }
        public void ResetBattle()
        {
            pause.Resume(); defeatPanel.SetActive(false);IsDefeated=false;IsVictorious=false;DefeatStateChanged?.Invoke(false);
            var active = pool.Active; for (int i = active.Count - 1; i >= 0; i--) active[i].Died -= OnUnitDied;
            pool.ReturnAll(); state.Reset(); playerBase.ResetBase(); buildingSelection.Clear(); playerLane = enemyLane = 0; status.text = "전투 초기화 완료";BattleReset?.Invoke();
        }
        public void MarkStageCleared(){if(IsStageEnded)return;IsVictorious=true;DefeatStateChanged?.Invoke(false);pause.SuspendForResult();}
        public void SetDefeatDetails(string details){if(defeatTitle!=null)defeatTitle.text="스테이지 실패\n"+details;}
        private void ShowDefeat() { if (defeatPanel.activeSelf||IsVictorious) return;IsDefeated=true;DefeatStateChanged?.Invoke(true); pause.SuspendForResult(); defeatPanel.SetActive(true); }
        public void Dispose() { playerBase.Defeated -= ShowDefeat; }
        private static UnitData Load(string name)
        {
            return RuntimeUnitCatalog.Get(name);
        }
    }
}
