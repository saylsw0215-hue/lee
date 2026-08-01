using System.Collections.Generic;
using HeroDefense.Battle.Combat;
using HeroDefense.Core;
using HeroDefense.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.Battle.Economy
{
    public enum GoldMineOwner{Neutral,Player,Enemy}

    /// <summary>Contested center objective. Nearby combatants capture it; player ownership generates periodic gold.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class GoldMineController:MonoBehaviour
    {
        private const float CaptureRadius=175f,CaptureSeconds=5f,IncomeInterval=5f;private const int IncomeGold=25;
        private readonly List<IDamageable> players=new(40),enemies=new(40);
        private BattleSessionState session;private PauseController pause;private CombatRegistry registry;private System.Func<bool> stageEnded;
        private Image frame,progress;private Text label;private float capture,incomeTimer,scanTimer;private int playerNearby,enemyNearby;
        public GoldMineOwner Owner{get;private set;}=GoldMineOwner.Neutral;public float CaptureValue=>capture;

        public void Initialize(RectTransform world,BattleSessionState state,PauseController pauseController,CombatRegistry combatRegistry,System.Func<bool> ended)
        {
            session=state;pause=pauseController;registry=combatRegistry;stageEnded=ended;transform.SetParent(world,false);var rect=GetComponent<RectTransform>();rect.sizeDelta=new Vector2(170,145);rect.anchoredPosition=new Vector2(80,-155);
            frame=gameObject.AddComponent<Image>();frame.sprite=UiFactory.RoundedSprite;frame.type=Image.Type.Sliced;frame.color=new Color(.34f,.28f,.17f,.92f);
            var artworkObject=new GameObject("MineArtwork",typeof(RectTransform),typeof(Image));artworkObject.transform.SetParent(transform,false);var artwork=artworkObject.GetComponent<Image>();artwork.sprite=RuntimeArtworkCatalog.Load("BuildingArt/building_siege_workshop");artwork.preserveAspect=true;artwork.raycastTarget=false;UiFactory.Stretch(artwork.rectTransform,new Vector2(.12f,.24f),new Vector2(.88f,.94f));
            label=UiFactory.Label(transform,"MineLabel","⛏ 중립 금광",18,TextAnchor.MiddleCenter,new Color(1f,.86f,.32f));label.raycastTarget=false;UiFactory.Stretch(label.rectTransform,new Vector2(.03f,.03f),new Vector2(.97f,.25f));
            var bar=UiFactory.Panel(transform,"CaptureBar",new Color(.06f,.07f,.08f,.94f),new Vector2(.08f,-.08f),new Vector2(.92f,.01f));progress=UiFactory.Panel(bar,"Fill",new Color(1f,.72f,.12f),Vector2.zero,Vector2.one).GetComponent<Image>();progress.type=Image.Type.Filled;progress.fillMethod=Image.FillMethod.Horizontal;progress.fillOrigin=0;RefreshVisual();
        }
        private void Update()
        {
            if(session==null||pause==null||pause.IsPaused||stageEnded?.Invoke()==true)return;float dt=Time.deltaTime;scanTimer-=dt;if(scanTimer<=0){scanTimer=.2f;Scan();}
            int difference=playerNearby-enemyNearby;if(difference!=0)capture=Mathf.Clamp(capture+Mathf.Sign(difference)*Mathf.Min(2,Mathf.Abs(difference))*dt/CaptureSeconds,-1,1);
            else if(playerNearby==0&&enemyNearby==0&&Owner==GoldMineOwner.Neutral)capture=Mathf.MoveTowards(capture,0,dt*.08f);
            GoldMineOwner next=capture>=.99f?GoldMineOwner.Player:capture<=-.99f?GoldMineOwner.Enemy:Owner;if(Owner==GoldMineOwner.Player&&capture<=0||Owner==GoldMineOwner.Enemy&&capture>=0)next=GoldMineOwner.Neutral;if(next!=Owner){Owner=next;incomeTimer=0;RefreshVisual();}
            if(Owner==GoldMineOwner.Player){incomeTimer+=dt;if(incomeTimer>=IncomeInterval){incomeTimer-=IncomeInterval;session.AddGold(IncomeGold);RefreshVisual();}}
            progress.fillAmount=Mathf.Abs(capture);progress.fillOrigin=capture<0?1:0;progress.color=capture<0?new Color(.9f,.22f,.18f):new Color(1f,.75f,.12f);
        }
        private void Scan()
        {
            registry.CollectPlayers(players);registry.CollectEnemies(enemies);playerNearby=CountNear(players);enemyNearby=CountNear(enemies);RefreshVisual();
        }
        private int CountNear(List<IDamageable> values){int count=0;Vector3 center=transform.localPosition;float squared=CaptureRadius*CaptureRadius;for(int i=0;i<values.Count;i++)if(values[i] is CombatUnit||values[i] is HeroDefense.Heroes.HeroController)if((values[i].TargetTransform.localPosition-center).sqrMagnitude<=squared)count++;return count;}
        private void RefreshVisual()
        {
            if(frame==null)return;frame.color=Owner switch{GoldMineOwner.Player=>new Color(.14f,.48f,.3f,.94f),GoldMineOwner.Enemy=>new Color(.58f,.16f,.15f,.94f),_=>new Color(.34f,.28f,.17f,.92f)};
            string owner=Owner==GoldMineOwner.Player?"아군 금광 +25":Owner==GoldMineOwner.Enemy?"몬스터 점령":"중립 금광";label.text=$"⛏ {owner}\n아군 {playerNearby}  적 {enemyNearby}";
        }
        public void ResetMine(){capture=incomeTimer=scanTimer=0;playerNearby=enemyNearby=0;Owner=GoldMineOwner.Neutral;RefreshVisual();if(progress!=null)progress.fillAmount=0;}
#if UNITY_EDITOR
        private void OnDrawGizmosSelected(){Gizmos.color=Color.yellow;Gizmos.DrawWireSphere(transform.position,CaptureRadius);}
#endif
    }
}
