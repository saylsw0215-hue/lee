using HeroDefense.Battle;
using HeroDefense.Core;
using NUnit.Framework;
using UnityEngine;

namespace HeroDefense.Tests
{
    public sealed class Phase1Tests
    {
        [TearDown] public void TearDown() => Time.timeScale = 1f;

        [Test] public void BattleState_HasExpectedDefaults()
        { var s = new BattleSessionState(); Assert.That(s.CurrentGold, Is.EqualTo(500)); Assert.That(s.CurrentWave, Is.Zero); Assert.That(s.CurrentBaseHp, Is.EqualTo(100)); Assert.That(s.MaxBaseHp, Is.EqualTo(100)); }

        [Test] public void GoldChange_RaisesEvent()
        { var s = new BattleSessionState(); bool raised = false; s.Changed += () => raised = true; s.AddGold(100); Assert.That(raised, Is.True); Assert.That(s.CurrentGold, Is.EqualTo(600)); }

        [Test] public void WaveChange_RaisesEvent()
        { var s = new BattleSessionState(); bool raised = false; s.Changed += () => raised = true; s.AdvanceWave(); Assert.That(raised, Is.True); Assert.That(s.CurrentWave, Is.EqualTo(1)); }

        [Test] public void BaseHp_IsClamped()
        { var s = new BattleSessionState(); s.SetBaseHp(-50); Assert.That(s.CurrentBaseHp, Is.Zero); s.SetBaseHp(999); Assert.That(s.CurrentBaseHp, Is.EqualTo(s.MaxBaseHp)); }

        [Test] public void Pause_ControlsTimeScale()
        { var p = new PauseController(); p.Pause(); Assert.That(Time.timeScale, Is.Zero); p.Resume(); Assert.That(Time.timeScale, Is.EqualTo(1f)); }

        [Test] public void BuildingSelection_Toggles()
        { var m = new BuildingSelectionModel(); m.Toggle(BuildingType.Barracks); Assert.That(m.Selected, Is.EqualTo(BuildingType.Barracks)); m.Toggle(BuildingType.Barracks); Assert.That(m.Selected, Is.Null); }

        [Test] public void SceneNames_AreCanonical()
        { Assert.That(SceneNames.Boot, Is.EqualTo("Boot")); Assert.That(SceneNames.MainMenu, Is.EqualTo("MainMenu")); Assert.That(SceneNames.Battle, Is.EqualTo("Battle")); }
    }
}
