using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    [CreateAssetMenu(fileName="StatusEffectData",menuName="Hero Defense/Combat/Status Effect Data")]
    public sealed class StatusEffectData:ScriptableObject
    {
        [SerializeField]private string effectId,displayName,description;[SerializeField]private StatusEffectType effectType;[SerializeField,Min(0)]private float duration=3,tickInterval=1,potency=.2f;[SerializeField,Min(1)]private int maxStacks=1;[SerializeField]private StatusRefreshRule refreshRule=StatusRefreshRule.RefreshDuration;[SerializeField]private bool resistanceApplies=true,allowedOnBoss=true;[SerializeField]private Sprite icon;[SerializeField]private Color color=Color.white;
        public string EffectId=>effectId;public string DisplayName=>displayName;public string Description=>description;public StatusEffectType EffectType=>effectType;public float Duration=>duration;public float TickInterval=>tickInterval;public float Potency=>potency;public int MaxStacks=>maxStacks;public StatusRefreshRule RefreshRule=>refreshRule;public bool ResistanceApplies=>resistanceApplies;public bool AllowedOnBoss=>allowedOnBoss;public Sprite Icon=>icon;public Color Color=>color;
        public bool Validate(out string reason){if(string.IsNullOrWhiteSpace(effectId)||duration<0||maxStacks<1||(effectType==StatusEffectType.DamageOverTime&&tickInterval<=0)){reason="Status ID, duration, tick interval, and stacks must be valid.";return false;}reason=string.Empty;return true;}
        public void Configure(string id,string name,string details,StatusEffectType type,float seconds,float tick,float value,int stacks,StatusRefreshRule rule,bool resist=true,bool boss=true,Color? tint=null){effectId=id;displayName=name;description=details;effectType=type;duration=Mathf.Max(0,seconds);tickInterval=Mathf.Max(.05f,tick);potency=value;maxStacks=Mathf.Max(1,stacks);refreshRule=rule;resistanceApplies=resist;allowedOnBoss=boss;color=tint??Color.white;}
        public void SetIcon(Sprite value)=>icon=value;
    }
}
