using UnityEngine;

namespace HeroDefense.Heroes
{
    public enum HeroPassiveKind{SteelWill,ConsecutiveShot,Ember,ProductionAutomation,HolyGrace,Executioner}
    [CreateAssetMenu(fileName="HeroPassiveData",menuName="Hero Defense/Heroes/Hero Passive Data")]
    public sealed class HeroPassiveData:ScriptableObject
    {
        [SerializeField]private string passiveId;[SerializeField]private string displayName;[SerializeField]private string description;[SerializeField]private HeroPassiveKind kind;[SerializeField]private float value=.2f;[SerializeField]private Sprite icon;[SerializeField]private Color placeholderColor=Color.white;
        public string PassiveId=>passiveId;public string DisplayName=>displayName;public string Description=>description;public HeroPassiveKind Kind=>kind;public float Value=>value;public Sprite Icon=>icon;public Color PlaceholderColor=>placeholderColor;
        public bool Validate(out string reason){if(string.IsNullOrWhiteSpace(passiveId)||string.IsNullOrWhiteSpace(displayName)){reason="Passive ID and name are required.";return false;}reason=string.Empty;return true;}
        public void Configure(string id,string name,string details,HeroPassiveKind valueKind,float effect,Color color){passiveId=id;displayName=name;description=details;kind=valueKind;value=effect;placeholderColor=color;}
        public void SetIcon(Sprite value)=>icon=value;
    }
}
