using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    /// <summary>Editable combat statistics and placeholder visual definition.</summary>
    [CreateAssetMenu(fileName = "UnitData", menuName = "Hero Defense/Combat/Unit Data")]
    public sealed class UnitData : ScriptableObject
    {
        [SerializeField] private string unitId;
        [SerializeField] private string displayName;
        [SerializeField] private Team team;
        [SerializeField, Min(.01f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0f)] private float attackDamage = 10f;
        [SerializeField, Min(.05f)] private float attackRange = 1f;
        [SerializeField, Min(.05f)] private float attackInterval = 1f;
        [SerializeField, Min(0f)] private float detectionRange = 8f;
        [SerializeField, Min(.05f)] private float collisionRadius = .45f;
        [SerializeField, Min(0)] private int rewardGold;
        [SerializeField] private Color placeholderColor = Color.white;
        [SerializeField] private UnitVisualShape visualShape;
        [SerializeField] private CombatStats advancedStats = new();

        public string UnitId => unitId;
        public string DisplayName => displayName;
        public Team Team => team;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public float AttackInterval => attackInterval;
        public float DetectionRange => detectionRange;
        public float CollisionRadius => collisionRadius;
        public int RewardGold => rewardGold;
        public Color PlaceholderColor => placeholderColor;
        public UnitVisualShape VisualShape => visualShape;
        public CombatStats AdvancedStats=>advancedStats??=new CombatStats();

        public bool Validate(out string reason)
        {
            if (string.IsNullOrWhiteSpace(unitId)) { reason = "Unit ID is required."; return false; }
            if (string.IsNullOrWhiteSpace(displayName)) { reason = "Display name is required."; return false; }
            if (maxHealth <= 0f || attackInterval <= 0f || attackRange <= 0f || collisionRadius <= 0f) { reason = "Health, interval, range, and radius must be positive."; return false; }
            if (detectionRange < attackRange) { reason = "Detection range must be at least the attack range."; return false; }
            if(!AdvancedStats.Validate(out reason))return false;
            reason = string.Empty; return true;
        }

        public void Configure(string id, string name, Team valueTeam, float health, float speed, float damage,
            float range, float interval, float detection, float radius, int reward, Color color, UnitVisualShape shape)
        {
            unitId = id; displayName = name; team = valueTeam; maxHealth = health; moveSpeed = speed;
            attackDamage = damage; attackRange = range; attackInterval = interval; detectionRange = detection;
            collisionRadius = radius; rewardGold = reward; placeholderColor = color; visualShape = shape;
        }
        public void ConfigureAdvanced(float defense,float magicDefense,float criticalChance,float criticalDamage,float dodge,float accuracy,float flatPen,float percentPen,float ccResistance,float statusResistance,float maxShield=0)=>AdvancedStats.Configure(defense,magicDefense,criticalChance,criticalDamage,dodge,accuracy,flatPen,percentPen,ccResistance,statusResistance,maxShield);
    }

    public enum UnitVisualShape { Swordsman, Slime, Goblin, Archer, Mage, EliteSlime, EliteGoblin, BossGoblin, PoisonGoblin, ShamanGoblin, Guard, Cannoneer, Priest, ChargeBoar, ArmoredOrc, SkeletonArcher, VampireBat, FrostSpirit, BomberGoblin, EliteArmoredOrc, EliteFrostSpirit, EliteSkeletonKnight, BossOrc, BossFrostQueen, BossDeathKnight }
}
