using System.Collections.Generic;
using HeroDefense.Battle;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Waves;
using HeroDefense.Heroes;
using HeroDefense.UI;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Meta;

namespace HeroDefense.Progression
{
    /// <summary>Coordinates XP events, queued choices, card UI, reroll, modifiers, and reset boundaries.</summary>
    public sealed class BattleProgressionController:MonoBehaviour
    {
        public BattleExperienceManager Experience{get;private set;}public BattleUpgradeInventory Inventory{get;}=new();public BattleModifierRepository Modifiers{get;}=new();
        private BattleCombatController combat;private WaveManager waves;private HeroController hero;private PauseController pause;private GameObject panel;private Text levelText,xpText;private Image xpFill;private readonly Button[] cardButtons=new Button[3];private readonly Text[] cardLabels=new Text[3];private Button rerollButton;private List<UpgradeData> current=new();private UpgradeCandidateService roller;private readonly HashSet<string> shown=new();private int rerolls;private bool ended,processing;

        public void Initialize(RectTransform safe,BattleCombatController battleCombat,WaveManager waveManager,HeroController selectedHero,PauseController pauseController)
        {
            combat=battleCombat;waves=waveManager;hero=selectedHero;pause=pauseController;Experience=new BattleExperienceManager();roller=new UpgradeCandidateService(new SeededRandomProvider(System.Environment.TickCount));BattleModifierRepository.Current=Modifiers;BuildHud(safe);BuildCards(safe);
            combat.UnitDied+=OnUnitDied;combat.BattleReset+=ResetProgression;waves.WaveCleared+=OnWaveCleared;waves.StageWon+=OnEnded;waves.StageFailed+=OnEnded;Experience.Changed+=RefreshHud;Experience.LevelGained+=OnLevelGained;ResetProgression();
        }
        private void BuildHud(RectTransform safe)
        {
            var box=UiFactory.Panel(safe,"ExperienceHUD",new Color(.05f,.08f,.14f,.96f),new Vector2(.405f,.805f),new Vector2(.595f,.858f));levelText=UiFactory.Label(box,"Level","LV.1",24,TextAnchor.MiddleLeft,new Color(.95f,.85f,.3f));levelText.rectTransform.anchorMin=new Vector2(.03f,0);levelText.rectTransform.anchorMax=new Vector2(.3f,1);xpText=UiFactory.Label(box,"XP","0 / 60",20,TextAnchor.MiddleCenter,Color.white);xpText.rectTransform.anchorMin=new Vector2(.3f,0);xpText.rectTransform.anchorMax=new Vector2(.97f,1);
            var fill=UiFactory.Panel(box,"XPFill",new Color(.3f,.75f,1,.28f),Vector2.zero,Vector2.one);xpFill=fill.GetComponent<Image>();xpFill.type=Image.Type.Filled;xpFill.fillMethod=Image.FillMethod.Horizontal;xpFill.raycastTarget=false;
        }
        private void BuildCards(RectTransform safe)
        {
            panel=UiFactory.Panel(safe,"LevelUpSelection",new Color(0,0,0,.9f),Vector2.zero,Vector2.one).gameObject;var title=UiFactory.Label(panel.transform,"Title","LEVEL UP!\n강화 하나를 선택하세요",45,TextAnchor.MiddleCenter,new Color(1,.86f,.3f));title.rectTransform.anchorMin=new Vector2(.2f,.82f);title.rectTransform.anchorMax=new Vector2(.8f,.97f);
            var row=UiFactory.Horizontal(panel.transform,"Cards",22);UiFactory.Stretch(row,new Vector2(.06f,.2f),new Vector2(.94f,.8f));for(int i=0;i<3;i++){int index=i;var button=UiFactory.Button(row,"UpgradeCard"+i,"",new Color(.16f,.2f,.3f));button.GetComponent<LayoutElement>().preferredWidth=480;cardButtons[i]=button;cardLabels[i]=button.GetComponentInChildren<Text>();cardLabels[i].fontSize=25;button.onClick.AddListener(()=>Select(index));}
            rerollButton=UiFactory.Button(panel.transform,"Reroll","새로고침 1",new Color(.28f,.34f,.52f));var rect=rerollButton.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.4f,.06f);rect.anchorMax=new Vector2(.6f,.16f);rect.offsetMin=rect.offsetMax=Vector2.zero;rerollButton.onClick.AddListener(Reroll);panel.SetActive(false);
        }
        private void OnUnitDied(CombatUnit unit,DamageInfo blow){if(ended||unit==null||unit.Team!=Team.Enemy||blow.SourceTeam!=Team.Player)return;int reward=Mathf.RoundToInt(ExperienceRewardService.ForEnemy(unit.Data.UnitId)*MetaRuntimeModifierProvider.EndlessExperienceMultiplier);Experience.AddExperience(reward);waves.Statistics.RecordExperience(reward,Experience.Level);}
        private void OnWaveCleared(int wave,int gold){if(ended)return;int reward=ExperienceRewardService.ForWave(wave);Experience.AddExperience(reward);waves.Statistics.RecordExperience(reward,Experience.Level);}
        private void OnLevelGained(int level){if(hero!=null&&hero.IsAlive)hero.Health.Heal(hero.Health.MaxHealth*.1f);TryShow();}
        private void TryShow(){if(ended||panel.activeSelf||Experience.PendingSelections<=0)return;pause.PauseFor(GamePauseReason.LevelUpSelection);Roll(false);panel.SetActive(true);}
        private void Roll(bool excludeShown){current=roller.Roll(RuntimeUpgradeCatalog.All,Inventory,Experience.Level,hero.Data.HeroId,excludeShown?shown:null);for(int i=0;i<cardButtons.Length;i++){bool active=i<current.Count;cardButtons[i].gameObject.SetActive(active);if(!active)continue;UpgradeData d=current[i];int next=Inventory.LevelOf(d.UpgradeId)+1;cardLabels[i].text=$"{d.DisplayName}\n[{d.Rarity}]  {d.Category}\n\n{d.Description}\n\nLv.{next} / {d.MaxLevel}";cardButtons[i].GetComponent<Image>().color=RarityColor(d.Rarity);}rerollButton.interactable=rerolls>0;rerollButton.GetComponentInChildren<Text>().text=$"새로고침 {rerolls}";}
        private void Select(int index){if(processing||index<0||index>=current.Count)return;processing=true;UpgradeData data=current[index];if(Inventory.Select(data,Experience.Level)){Modifiers.Rebuild(Inventory);ApplyToActive();waves.Statistics.RecordUpgradeSelection(data.Rarity,data.Category);Experience.ConsumeSelection();}panel.SetActive(false);processing=false;if(Experience.PendingSelections>0)TryShow();else pause.ResumeReason(GamePauseReason.LevelUpSelection);RefreshHud();}
        private void Reroll(){if(processing||rerolls<=0)return;rerolls--;waves.Statistics.RecordUpgradeReroll();for(int i=0;i<current.Count;i++)shown.Add(current[i].UpgradeId);Roll(true);}
        private void ApplyToActive(){var list=new List<IDamageable>(40);combat.Registry.CollectPlayers(list);for(int i=0;i<list.Count;i++)if(list[i] is IAdvancedCombatant advanced){advanced.RuntimeStats.Clear();Modifiers.Apply(advanced.RuntimeStats,list[i] is HeroController,list[i] is CombatUnit unit?unit.Data.UnitId:null);}}
        private void RefreshHud(){if(Experience==null)return;levelText.text=$"LV.{Experience.Level}";xpText.text=Experience.IsMaxLevel?"MAX":$"XP {Experience.CurrentExperience} / {Experience.RequiredExperience}";xpFill.fillAmount=Experience.IsMaxLevel?1:(float)Experience.CurrentExperience/Mathf.Max(1,Experience.RequiredExperience);}
        private void OnEnded(){ended=true;Experience.ClearPending();panel.SetActive(false);pause.ResumeReason(GamePauseReason.LevelUpSelection);RefreshHud();}
        private void ResetProgression(){ended=false;rerolls=1;shown.Clear();current.Clear();Inventory.Clear();Modifiers.Clear();Experience.Reset();panel.SetActive(false);pause.ResumeReason(GamePauseReason.LevelUpSelection);RefreshHud();}
        private static Color RarityColor(UpgradeRarity r)=>r switch{UpgradeRarity.Common=>new Color(.18f,.24f,.3f),UpgradeRarity.Rare=>new Color(.12f,.32f,.62f),UpgradeRarity.Epic=>new Color(.45f,.18f,.62f),_=>new Color(.65f,.42f,.08f)};
        private void OnDestroy(){if(combat!=null){combat.UnitDied-=OnUnitDied;combat.BattleReset-=ResetProgression;}if(waves!=null){waves.WaveCleared-=OnWaveCleared;waves.StageWon-=OnEnded;waves.StageFailed-=OnEnded;}if(Experience!=null){Experience.Changed-=RefreshHud;Experience.LevelGained-=OnLevelGained;}if(BattleModifierRepository.Current==Modifiers)BattleModifierRepository.Current=null;}
    }
}
