using HeroDefense.Heroes;
using HeroDefense.Heroes.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.UI.Heroes
{
    /// <summary>Event-fed hero HUD with active, ultimate, cooldown, energy, and respawn feedback.</summary>
    public sealed class HeroHudController:MonoBehaviour
    {
        private HeroController hero;private SkillAimingController aiming;private Text nameText,hpText,stateText,skillLabel,ultimateLabel,statusText;private Image hpFill,shieldFill,energyFill,portrait;private Button skillButton,ultimateButton;private float refreshTimer;
        public void Initialize(RectTransform safe,HeroController controller,SkillAimingController aim=null)
        {
            hero=controller;aiming=aim;var panel=UiFactory.Panel(safe,"HeroHUD",new Color(.025f,.05f,.09f,.95f),new Vector2(.01f,.31f),new Vector2(.245f,.62f));
            portrait=UiFactory.Panel(panel,"HeroPortrait",hero.Data.PlaceholderColor,new Vector2(.04f,.48f),new Vector2(.3f,.93f)).GetComponent<Image>();if(hero.Data.Portrait!=null){portrait.sprite=hero.Data.Portrait;portrait.color=Color.white;portrait.preserveAspect=true;}
            nameText=UiFactory.Label(panel,"HeroName",hero.Data.DisplayName,25,TextAnchor.MiddleLeft,Color.white);nameText.rectTransform.anchorMin=new Vector2(.33f,.75f);nameText.rectTransform.anchorMax=new Vector2(.96f,.96f);nameText.rectTransform.offsetMin=nameText.rectTransform.offsetMax=Vector2.zero;
            hpText=UiFactory.Label(panel,"HeroHP","",20,TextAnchor.MiddleLeft,Color.white);hpText.rectTransform.anchorMin=new Vector2(.33f,.53f);hpText.rectTransform.anchorMax=new Vector2(.96f,.76f);hpText.rectTransform.offsetMin=hpText.rectTransform.offsetMax=Vector2.zero;
            var hpBar=UiFactory.Panel(panel,"HPBar",new Color(.1f,.1f,.1f),new Vector2(.33f,.47f),new Vector2(.96f,.55f));hpFill=UiFactory.Panel(hpBar,"Fill",new Color(.18f,.82f,.28f),Vector2.zero,Vector2.one).GetComponent<Image>();hpFill.type=Image.Type.Filled;hpFill.fillMethod=Image.FillMethod.Horizontal;
            shieldFill=UiFactory.Panel(hpBar,"ShieldFill",new Color(.25f,.65f,1f,.75f),Vector2.zero,Vector2.one).GetComponent<Image>();shieldFill.type=Image.Type.Filled;shieldFill.fillMethod=Image.FillMethod.Horizontal;
            stateText=UiFactory.Label(panel,"HeroState","",20,TextAnchor.MiddleCenter,new Color(1,.85f,.3f));stateText.rectTransform.anchorMin=new Vector2(.03f,.3f);stateText.rectTransform.anchorMax=new Vector2(.97f,.47f);stateText.rectTransform.offsetMin=stateText.rectTransform.offsetMax=Vector2.zero;
            statusText=UiFactory.Label(panel,"HeroStatusIcons","",17,TextAnchor.MiddleCenter,Color.white);statusText.rectTransform.anchorMin=new Vector2(.03f,.27f);statusText.rectTransform.anchorMax=new Vector2(.97f,.34f);statusText.rectTransform.offsetMin=statusText.rectTransform.offsetMax=Vector2.zero;
            skillButton=UiFactory.Button(panel,"HeroSkill",hero.Data.ActiveSkill.DisplayName,hero.Data.ActiveSkill.PlaceholderColor);ApplySkillIcon(skillButton,hero.Data.ActiveSkill);Place(skillButton,new Vector2(.03f,.03f),new Vector2(.47f,.27f));skillButton.onClick.AddListener(()=>BeginOrUse(hero.Data.ActiveSkill,false));skillLabel=skillButton.GetComponentInChildren<Text>();
            ultimateButton=UiFactory.Button(panel,"HeroUltimate",hero.Data.UltimateSkill.DisplayName,hero.Data.UltimateSkill.PlaceholderColor);ApplySkillIcon(ultimateButton,hero.Data.UltimateSkill);Place(ultimateButton,new Vector2(.52f,.03f),new Vector2(.97f,.27f));ultimateButton.onClick.AddListener(()=>BeginOrUse(hero.Data.UltimateSkill,true));ultimateLabel=ultimateButton.GetComponentInChildren<Text>();
            var energy=UiFactory.Panel(ultimateButton.transform,"Energy",new Color(.08f,.08f,.12f,.9f),new Vector2(.06f,.03f),new Vector2(.94f,.12f));energyFill=UiFactory.Panel(energy,"Fill",new Color(1f,.7f,.08f),Vector2.zero,Vector2.one).GetComponent<Image>();energyFill.type=Image.Type.Filled;energyFill.fillMethod=Image.FillMethod.Horizontal;
            hero.HealthChanged+=OnHealth;hero.StateChanged+=Refresh;hero.ResourcesChanged+=Refresh;hero.Statuses.Changed+=Refresh;hero.Shields.Changed+=OnShield;OnHealth(hero.Health.CurrentHealth,hero.Health.MaxHealth);OnShield(hero.Shields.Total);Refresh();
        }
        private void BeginOrUse(HeroSkillData data,bool ultimate){if(data.TargetingMode==SkillTargetingMode.Automatic||data.TargetingMode==SkillTargetingMode.Self||aiming==null){if(ultimate)hero.UseUltimate();else hero.UseActiveSkill();}else aiming.Begin(data,ultimate);}
        private void Update(){if(hero==null)return;refreshTimer-=Time.deltaTime;if(refreshTimer<=0){refreshTimer=.1f;Refresh();}}
        private void OnHealth(float current,float max){hpFill.fillAmount=max<=0?0:current/max;hpText.text=$"HP {current:0} / {max:0}";}
        private void OnShield(float amount){shieldFill.fillAmount=hero.Health.MaxHealth<=0?0:Mathf.Clamp01(amount/hero.Health.MaxHealth);}
        private void Refresh(){if(hero==null)return;var runtime=hero.Runtime;bool alive=runtime.State==HeroState.Alive;skillButton.interactable=runtime.CanUseSkill&&!hero.Statuses.IsSilenced;ultimateButton.interactable=runtime.CanUseUltimate&&!hero.Statuses.IsSilenced;skillLabel.text=runtime.SkillCooldownRemaining>0?$"{hero.Data.ActiveSkill.DisplayName}\n{runtime.SkillCooldownRemaining:0.0}":hero.Data.ActiveSkill.DisplayName;ultimateLabel.text=runtime.UltimateEnergy>=100?$"{hero.Data.UltimateSkill.DisplayName}\nREADY":hero.Data.UltimateSkill.DisplayName;energyFill.fillAmount=runtime.UltimateEnergy/100f;stateText.text=runtime.State==HeroState.Respawning?$"부활까지 {runtime.RespawnRemaining:0.0}초":runtime.State.ToString();portrait.color=alive?(hero.Data.Portrait!=null?Color.white:hero.Data.PlaceholderColor):new Color(.25f,.25f,.25f,.8f);var list=hero.Statuses.Active;System.Text.StringBuilder b=new();for(int i=0;i<list.Count&&i<6;i++){if(i>0)b.Append("  ");b.Append(list[i].Data.DisplayName);if(list[i].Stacks>1)b.Append('x').Append(list[i].Stacks);}statusText.text=b.ToString();}
        private static void Place(Button button,Vector2 min,Vector2 max){var rect=button.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;}
        private static void ApplySkillIcon(Button button,HeroSkillData data){if(data.Icon==null)return;Image image=button.GetComponent<Image>();image.sprite=data.Icon;image.color=Color.white;image.preserveAspect=true;Text label=button.GetComponentInChildren<Text>();if(label!=null){label.color=Color.white;label.fontStyle=FontStyle.Bold;}}
        private void OnDestroy(){if(hero==null)return;hero.HealthChanged-=OnHealth;hero.StateChanged-=Refresh;hero.ResourcesChanged-=Refresh;hero.Statuses.Changed-=Refresh;hero.Shields.Changed-=OnShield;}
    }
}
