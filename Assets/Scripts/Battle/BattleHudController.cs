using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.Battle
{
    /// <summary>Observes battle state and renders top HUD values.</summary>
    public sealed class BattleHudController
    {
        private readonly BattleSessionState state;
        private readonly Text gold, wave, hp;
        public BattleHudController(Transform row, BattleSessionState state)
        {
            this.state = state;
            gold = Make(row, "Gold"); wave = Make(row, "Wave"); hp = Make(row, "BaseHP");
            state.Changed += Refresh; Refresh();
        }
        private static Text Make(Transform row, string name)
        {
            var label = UI.UiFactory.Label(row, name, "", 31, TextAnchor.MiddleCenter, Color.white);
            label.gameObject.AddComponent<LayoutElement>().preferredWidth = 390; return label;
        }
        private void Refresh() { gold.text = $"Gold  {state.CurrentGold}"; wave.text = $"Wave  {state.CurrentWave}"; hp.text = $"Base HP  {state.CurrentBaseHp} / {state.MaxBaseHp}"; }
        public void Dispose() => state.Changed -= Refresh;
    }
}
