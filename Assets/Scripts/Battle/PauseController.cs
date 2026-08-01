using System;
using UnityEngine;

namespace HeroDefense.Battle
{
    public enum GamePauseReason { UserPause,LevelUpSelection,ApplicationBackground,Victory,Defeat }
    /// <summary>Controls time scale and publishes pause state.</summary>
    public sealed class PauseController
    {
        public event Action<bool> Changed;
        private readonly System.Collections.Generic.HashSet<GamePauseReason> reasons=new();
        public bool IsPaused => reasons.Count>0;
        public bool HasReason(GamePauseReason reason)=>reasons.Contains(reason);
        public void Pause() => PauseFor(GamePauseReason.UserPause);
        public void PauseFor(GamePauseReason reason){bool was=IsPaused;reasons.Add(reason);Time.timeScale=0f;if(!was)Changed?.Invoke(true);}
        public void ResumeReason(GamePauseReason reason){bool was=IsPaused;reasons.Remove(reason);Time.timeScale=IsPaused?0f:1f;if(was&&!IsPaused)Changed?.Invoke(false);}
        public void SuspendForResult() => PauseFor(GamePauseReason.Victory);
        public void Resume() { bool was=IsPaused;reasons.Clear();Time.timeScale=1f;if(was)Changed?.Invoke(false); }
        public void Toggle() { if (IsPaused) Resume(); else Pause(); }
    }
}
