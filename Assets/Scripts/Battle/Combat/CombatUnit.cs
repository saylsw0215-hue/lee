using System;
using System.Collections;
using System.Collections.Generic;
using HeroDefense.Battle.Effects;
using HeroDefense.Battle.Projectiles;
using UnityEngine;
using HeroDefense.Core;
using HeroDefense.Meta;

namespace HeroDefense.Battle.Combat
{
    /// <summary>Coordinates simple single-lane targeting, movement and attacks for one pooled unit.</summary>
    [RequireComponent(typeof(RectTransform), typeof(HealthComponent), typeof(UnitVisualController))]
    public sealed class CombatUnit : MonoBehaviour, IAdvancedCombatant
    {
        public const float PixelsPerUnit = 72f;
        public event Action<CombatUnit, DamageInfo> Died;
        public event Action<CombatUnit, DamageInfo, DamageResult> DamageResolved;
        public UnitData Data { get; private set; }
        public Team Team => Data.Team;
        public bool IsAlive => health != null && health.IsAlive && State != CombatUnitState.Dead && State != CombatUnitState.Inactive;
        public Transform TargetTransform => transform;
        public float CollisionRadius => Data.CollisionRadius * PixelsPerUnit;
        public CombatUnitState State { get; private set; } = CombatUnitState.Inactive;
        public IDamageable CurrentTarget { get; private set; }
        public HealthComponent Health=>health;
        public RuntimeCombatStats RuntimeStats{get;private set;}public StatusEffectController Statuses{get;private set;}public ShieldController Shields{get;private set;}=new();public DamageResult LastDamageResult{get;private set;}

        private HealthComponent health;
        private UnitVisualController visual;
        private CombatRegistry registry;
        private FloatingDamageTextPool damageTexts;
        private CombatPool ownerPool;
        private ProjectilePool projectiles;
        private readonly AttackCooldown cooldown = new();
        private float targetTimer;
        private float forwardLimit;
        private bool suppressReward;
        private float stunRemaining,damageReductionRemaining,damageReduction;
        private float specialTimer;private StatusEffectData poisonStatus,shamanPower;private readonly List<IDamageable> specialScratch=new(40);

        public void Construct(UnitData data, CombatRegistry valueRegistry, CombatPool pool, FloatingDamageTextPool texts,ProjectilePool projectilePool)
        {
            Data = data; registry = valueRegistry; ownerPool = pool; damageTexts = texts;projectiles=projectilePool;
            health = GetComponent<HealthComponent>(); visual = GetComponent<UnitVisualController>(); visual.Build(data);RuntimeStats=new RuntimeCombatStats(data.AdvancedStats);Statuses=new StatusEffectController(this);Statuses.Changed+=()=>visual.SetStatuses(Statuses.Active);if(data.VisualShape==UnitVisualShape.PoisonGoblin)poisonStatus=RuntimeStatusCatalog.Get("Poison");if(data.VisualShape==UnitVisualShape.ShamanGoblin)shamanPower=RuntimeStatusCatalog.Get("ShamanPower");
            health.HealthChanged += visual.SetHealth; health.Damaged += OnDamaged; health.Died += OnDeath;
            gameObject.SetActive(false);
        }

        public void Spawn(Vector2 localPosition, float limit)
        {
            suppressReward = false; forwardLimit = limit; transform.localPosition = localPosition; transform.localScale = Vector3.one;
            CurrentTarget = null; targetTimer = 0f;specialTimer=3f; cooldown.Reset();RuntimeStats.Clear();HeroDefense.Progression.BattleModifierRepository.Current?.Apply(RuntimeStats,false,Data.UnitId);float spawnHealth=Data.MaxHealth;if(Data.Team==Team.Player){spawnHealth*=MetaRuntimeModifierProvider.UnitHealthMultiplier;RuntimeStats.Add(new StatModifier("meta_unit_attack",CombatStat.AttackPower,StatModifierType.MultiplicativePercent,MetaRuntimeModifierProvider.UnitAttackMultiplier-1));RuntimeStats.Add(new StatModifier("meta_unit_critical",CombatStat.CriticalChance,StatModifierType.Flat,MetaRuntimeModifierProvider.UnitCriticalBonus));}else{DifficultyModifiers m=DifficultyModifiers.For(BattleLaunchConfig.Difficulty);RuntimeStats.Add(new StatModifier("difficulty",CombatStat.AttackPower,StatModifierType.MultiplicativePercent,m.EnemyDamage-1));RuntimeStats.Add(new StatModifier("difficulty",CombatStat.MoveSpeed,StatModifierType.MultiplicativePercent,m.EnemySpeed-1));spawnHealth*=m.EnemyHealth;if(BattleLaunchConfig.Mode==GameMode.Stage){var selectedStage=BattleLaunchConfig.SelectedStage;spawnHealth*=selectedStage.EnemyHealthMultiplier;RuntimeStats.Add(new StatModifier("stage_balance",CombatStat.AttackPower,StatModifierType.MultiplicativePercent,selectedStage.EnemyDamageMultiplier-1));}else{int wave=Mathf.Max(1,HeroDefense.Battle.Waves.EndlessSession.CurrentWave);spawnHealth*=HeroDefense.Battle.Waves.EndlessWaveGenerator.HealthMultiplier(wave);RuntimeStats.Add(new StatModifier("endless",CombatStat.AttackPower,StatModifierType.MultiplicativePercent,HeroDefense.Battle.Waves.EndlessWaveGenerator.DamageMultiplier(wave)-1));}}Statuses.Clear();Shields.Clear();health.Initialize(spawnHealth); visual.ResetVisual();
            State = CombatUnitState.Idle; gameObject.SetActive(true); registry.Register(this);
        }

        private void Update() => Simulate(Time.deltaTime);
        public void Simulate(float deltaTime)
        {
            if (!IsAlive || deltaTime <= 0f) return;Statuses.Tick(deltaTime);Shields.Tick(deltaTime);if(Statuses.IsStunned||Statuses.IsFrozen)return;if(stunRemaining>0f){stunRemaining=Mathf.Max(0,stunRemaining-deltaTime);return;}if(damageReductionRemaining>0f){damageReductionRemaining=Mathf.Max(0,damageReductionRemaining-deltaTime);if(damageReductionRemaining<=0)damageReduction=0;}
            cooldown.Tick(deltaTime); targetTimer -= deltaTime;if(Data.VisualShape==UnitVisualShape.ShamanGoblin){specialTimer-=deltaTime;if(specialTimer<=0){specialTimer=10f;BuffNearbyEnemies();}}
            if (CurrentTarget == null || !CurrentTarget.IsAlive || !CurrentTarget.TargetTransform.gameObject.activeInHierarchy || targetTimer <= 0f)
            { CurrentTarget = Statuses.TauntTarget??registry.FindTarget(this); targetTimer = .2f; }
            if (CurrentTarget != null && CombatRegistry.CanAttack(this, CurrentTarget))
            {
                Vector3 delta = CurrentTarget.TargetTransform.localPosition - transform.localPosition;
                float effectiveRange = Data.AttackRange * RuntimeStats.AttackRangeMultiplier * PixelsPerUnit + CollisionRadius + CurrentTarget.CollisionRadius;
                if (delta.sqrMagnitude <= effectiveRange * effectiveRange) { State = CombatUnitState.Attacking; TryAttack(); }
                else { State = CombatUnitState.Moving; Move(delta, deltaTime); }
            }
            else { State = CombatUnitState.Moving; Move(new Vector3(Team == Team.Player ? 1f : -1f, 0f), deltaTime); }
        }
        private void Move(Vector3 direction, float deltaTime)
        {
            direction.y *= .35f; if (direction.sqrMagnitude > 1f) direction.Normalize();
            Vector3 next = transform.localPosition + direction * (Data.MoveSpeed * RuntimeStats.MoveSpeedMultiplier * Statuses.MoveMultiplier * PixelsPerUnit * deltaTime);
            next.x = Team == Team.Player ? Mathf.Min(next.x, forwardLimit) : Mathf.Max(next.x, forwardLimit);
            transform.localPosition = next;
        }
        private void TryAttack()
        {
            if (!cooldown.TryConsume(Data.AttackInterval/(RuntimeStats.AttackSpeedMultiplier*Statuses.AttackSpeedMultiplier)) || !CombatRegistry.CanAttack(this, CurrentTarget)) return;
            visual.PlayAttack();projectiles?.Show(Data.VisualShape,transform.parent,transform.localPosition,CurrentTarget.TargetTransform.localPosition);DamageType type=(Data.VisualShape==UnitVisualShape.Mage||Data.VisualShape==UnitVisualShape.ShamanGoblin)?DamageType.Magical:DamageType.Physical;float attack=Data.AttackDamage*RuntimeStats.AttackPowerMultiplier;if(Team==Team.Player&&CurrentTarget is CombatUnit boss&&boss.Data.UnitId.StartsWith("boss_"))attack*=MetaRuntimeModifierProvider.BossDamageMultiplier;CurrentTarget.TakeDamage(new DamageInfo(attack,Team,gameObject,type,true,type==DamageType.Physical));if(Data.VisualShape==UnitVisualShape.PoisonGoblin&&poisonStatus!=null&&UnityEngine.Random.value<.4f){if(CurrentTarget is CombatUnit unit)unit.ApplyStatus(poisonStatus,gameObject,Mathf.Min(CurrentTarget is IAdvancedCombatant advanced?advanced.Health.MaxHealth*.005f+Data.AttackDamage*.1f:Data.AttackDamage*.1f,Data.AttackDamage*.5f));else if(CurrentTarget is HeroDefense.Heroes.HeroController hero)hero.ApplyStatus(poisonStatus,gameObject,hero.Health.MaxHealth*.005f+Data.AttackDamage*.1f);}
        }
        private int BuffNearbyEnemies(){if(shamanPower==null)shamanPower=RuntimeStatusCatalog.Get("ShamanPower");if(shamanPower==null||Team!=Team.Enemy)return 0;registry.CollectEnemies(specialScratch);float radius=5f*PixelsPerUnit,squared=radius*radius;int applied=0;for(int i=0;i<specialScratch.Count;i++){if(specialScratch[i] is not CombatUnit unit||unit==this||(unit.transform.localPosition-transform.localPosition).sqrMagnitude>squared)continue;var result=unit.ApplyStatus(shamanPower,gameObject,.15f);if(result==StatusApplyResult.Applied||result==StatusApplyResult.Refreshed)applied++;}return applied;}
        public int TriggerSupportBuffForDebug()=>BuffNearbyEnemies();
        public void TakeDamage(DamageInfo damageInfo)=>ApplyAdvancedDamage(damageInfo);
        public DamageResult ApplyAdvancedDamage(DamageInfo damageInfo){if(damageInfo.SourceTeam==Team)return default;var adjusted=new DamageInfo(damageInfo.Amount*(1f-damageReduction),damageInfo.SourceTeam,damageInfo.Source,damageInfo.DamageType,damageInfo.CanCritical,damageInfo.CanDodge,damageInfo.IsSkill,damageInfo.IsUltimate,damageInfo.IsDamageOverTime,damageInfo.SkillId,damageInfo.HitSequence,damageInfo.CriticalChanceBonus,damageInfo.CriticalDamageBonus,damageInfo.ArmorPenetrationFlat,damageInfo.ArmorPenetrationPercent,gameObject);LastDamageResult=AdvancedCombatResolver.Apply(this,adjusted);DamageResolved?.Invoke(this,adjusted,LastDamageResult);if(LastDamageResult.WasDodged)damageTexts?.ShowText(transform.parent,transform.localPosition,"DODGE",new Color(.7f,.9f,1f),34);else if(LastDamageResult.ShieldAbsorbed>0&&LastDamageResult.HealthDamage<=0)damageTexts?.ShowText(transform.parent,transform.localPosition,$"SHIELD {Mathf.RoundToInt(LastDamageResult.ShieldAbsorbed)}",new Color(.3f,.8f,1f),25);else if(LastDamageResult.WasApplied)damageTexts?.ShowAdvanced(transform.parent,transform.localPosition,LastDamageResult.HealthDamage,LastDamageResult.WasCritical,damageInfo.DamageType);return LastDamageResult;}
        public StatusApplyResult ApplyStatus(StatusEffectData data,GameObject source=null,float potency=-1,IDamageable tauntSource=null)=>Statuses.Apply(data,source,potency,tauntSource);
        public void ApplyStun(float duration)=>stunRemaining=Mathf.Max(stunRemaining,duration);
        public void ApplyDamageReduction(float amount,float duration){damageReduction=Mathf.Max(damageReduction,Mathf.Clamp01(amount));damageReductionRemaining=Mathf.Max(damageReductionRemaining,duration);}
        private void OnDamaged(DamageInfo info)
        {
            visual.PlayHit();
        }
        private void OnDeath(DamageInfo info)
        {
            State = CombatUnitState.Dead; CurrentTarget = null; registry.Unregister(this); Died?.Invoke(this, suppressReward ? default : info);
            StartCoroutine(DeathRoutine());
        }
        private IEnumerator DeathRoutine()
        {
            float elapsed = 0f; Vector3 initial = transform.localScale;
            while (elapsed < .3f) { elapsed += Time.deltaTime; transform.localScale = Vector3.Lerp(initial, Vector3.zero, elapsed / .3f); yield return null; }
            ownerPool.Return(this);
        }
        public void ReturnWithoutReward()
        {
            suppressReward = true; StopAllCoroutines(); registry.Unregister(this); CurrentTarget = null;stunRemaining=damageReductionRemaining=damageReduction=0;Statuses?.Clear();Shields.Clear();RuntimeStats?.Clear();State = CombatUnitState.Inactive; gameObject.SetActive(false);
        }
        internal void MarkPooled() { CurrentTarget = null; State = CombatUnitState.Inactive; gameObject.SetActive(false); }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Data == null) return;
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, Data.AttackRange * PixelsPerUnit);
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, Data.DetectionRange * PixelsPerUnit);
            if (CurrentTarget != null) { Gizmos.color = Color.magenta; Gizmos.DrawLine(transform.position, CurrentTarget.TargetTransform.position); }
        }
#endif
    }
}
