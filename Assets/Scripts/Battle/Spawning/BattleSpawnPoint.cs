using HeroDefense.Battle.Combat;
using UnityEngine;

namespace HeroDefense.Battle
{
    /// <summary>Named team spawn anchor with an Editor-only gizmo.</summary>
    public sealed class BattleSpawnPoint : MonoBehaviour
    {
        public Team Team { get; private set; }
        public void Configure(Team team, Vector2 localPosition) { Team = team; transform.localPosition = localPosition; }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Team == Team.Player ? Color.cyan : Color.red;
            Gizmos.DrawWireSphere(transform.position, 32f);
            Gizmos.DrawLine(transform.position + Vector3.up * 50f, transform.position - Vector3.up * 50f);
        }
#endif
    }
}
