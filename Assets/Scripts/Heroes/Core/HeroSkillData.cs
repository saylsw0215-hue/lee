using UnityEngine;

namespace HeroDefense.Heroes
{
    public enum SkillTargetingMode{Automatic,Self,TargetUnit,TargetPosition,Direction,Cone,Line,Circle}
    public enum HeroSkillKind{KnightShieldBash,KnightGuardianOath,RangerArrowRain,RangerHawkeye,MageFireExplosion,MageMeteor,EngineerTurret,EngineerOverdrive,SaintSanctuary,SaintDivineBarrier,AssassinShadowLeap,AssassinDeathMark}
    [CreateAssetMenu(fileName="HeroSkillData",menuName="Hero Defense/Heroes/Hero Skill Data")]
    public sealed class HeroSkillData:ScriptableObject
    {
        [SerializeField]private string skillId;[SerializeField]private string displayName;[SerializeField]private string description;[SerializeField]private HeroSkillKind kind;[SerializeField,Min(0)]private float cooldown;[SerializeField,Min(0)]private float castTime=.2f;[SerializeField,Min(0)]private float range=8;[SerializeField,Min(0)]private float radius=2;[SerializeField,Min(0)]private float damageMultiplier=1;[SerializeField,Min(0)]private float duration;[SerializeField,Min(1)]private int maxTargets=5;[SerializeField,Range(0,100)]private int requiredEnergy;[SerializeField]private SkillTargetingMode targetingMode;[SerializeField]private Sprite icon;[SerializeField]private Color placeholderColor=Color.white;
        public string SkillId=>skillId;public string DisplayName=>displayName;public string Description=>description;public HeroSkillKind Kind=>kind;public float Cooldown=>cooldown;public float CastTime=>castTime;public float Range=>range;public float Radius=>radius;public float DamageMultiplier=>damageMultiplier;public float Duration=>duration;public int MaxTargets=>maxTargets;public int RequiredEnergy=>requiredEnergy;public SkillTargetingMode TargetingMode=>targetingMode;public Sprite Icon=>icon;public Color PlaceholderColor=>placeholderColor;
        public bool Validate(out string reason){if(string.IsNullOrWhiteSpace(skillId)||string.IsNullOrWhiteSpace(displayName)||cooldown<0||requiredEnergy<0||requiredEnergy>100||range<0||radius<0){reason="Skill ID, name, cooldown, energy, range, and radius must be valid.";return false;}reason=string.Empty;return true;}
        public void Configure(string id,string name,string details,HeroSkillKind valueKind,float cd,float cast,float valueRange,float valueRadius,float multiplier,float valueDuration,int targets,int energy,Color color){skillId=id;displayName=name;description=details;kind=valueKind;cooldown=cd;castTime=cast;range=valueRange;radius=valueRadius;damageMultiplier=multiplier;duration=valueDuration;maxTargets=targets;requiredEnergy=energy;placeholderColor=color;}
        public void SetIcon(Sprite value)=>icon=value;
        public void SetTargetingMode(SkillTargetingMode value)=>targetingMode=value;
    }
}
