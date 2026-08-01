using System;
using HeroDefense.Battle.Combat;
using HeroDefense.Battle.Effects;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Meta;
using HeroDefense.Save;

namespace HeroDefense.Battle
{
    /// <summary>Damageable player base whose health is the source for the HUD base value.</summary>
    public sealed class PlayerBase : MonoBehaviour, IDamageable
    {
        public event Action Defeated;
        public Team Team => Team.Player;
        public bool IsAlive => health.IsAlive;
        public Transform TargetTransform => transform;
        public float CollisionRadius => 58f;
        private HealthComponent health;
        private BattleSessionState state;
        private WorldHealthBar healthBar;
        private bool defeatRaised;
        private float startingShield;

        public void Build(BattleSessionState session)
        {
            state = session; health = gameObject.AddComponent<HealthComponent>();
            var image = gameObject.AddComponent<Image>(); image.color = new Color(.1f,.38f,.82f);
            var rect = GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(125, 190);
            var label = UI.UiFactory.Label(transform,"Label","아군 본진",24,TextAnchor.MiddleCenter,Color.white); label.rectTransform.anchoredPosition = new Vector2(0,-115); label.rectTransform.sizeDelta = new Vector2(160,40);
            healthBar = new WorldHealthBar(transform, new Vector2(0,112), 130);
            health.HealthChanged += OnHealthChanged; health.Died += OnDied; ResetBase();
        }
        public void ResetBase() { defeatRaised = false; health.Initialize(state.MaxBaseHp);startingShield=state.MaxBaseHp*(SaveGameManager.Instance==null?0:new MetaUpgradeService(SaveGameManager.Instance).Effect("meta_base_shield")); }
        public void TakeDamage(DamageInfo damageInfo) { if (damageInfo.SourceTeam == Team.Player)return;float remaining=damageInfo.Amount;if(startingShield>0){float absorbed=Mathf.Min(startingShield,remaining);startingShield-=absorbed;remaining-=absorbed;}if(remaining>0)health.TakeDamage(new DamageInfo(remaining,damageInfo.SourceTeam,damageInfo.Source,damageInfo.DamageType,damageInfo.CanCritical,damageInfo.CanDodge,damageInfo.IsSkill,damageInfo.IsUltimate,damageInfo.IsDamageOverTime,damageInfo.SkillId)); }
        private void OnHealthChanged(float current, float max) { healthBar.Set(current,max); state.SetBaseHp(Mathf.RoundToInt(current)); }
        private void OnDied(DamageInfo info) { if (defeatRaised) return; defeatRaised = true; Defeated?.Invoke(); }
        private void OnDestroy()
        {
            if (health == null) return; health.HealthChanged -= OnHealthChanged; health.Died -= OnDied;
        }
    }
}
