using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    /// <summary>Tracks only active entities and provides allocation-free nearest-target queries.</summary>
    public sealed class CombatRegistry
    {
        private readonly List<IDamageable> players = new(32);
        private readonly List<IDamageable> enemies = new(40);
        public PlayerBase PlayerBase { get; private set; }
        public int PlayerCount {get{int count=0;for(int i=0;i<players.Count;i++)if(players[i] is CombatUnit)count++;return count;}}
        public int EnemyCount => enemies.Count;

        public void SetPlayerBase(PlayerBase playerBase) => PlayerBase = playerBase;
        public void Register(IDamageable unit)
        {
            var list = unit.Team == Team.Player ? players : enemies;
            if (!list.Contains(unit)) list.Add(unit);
        }
        public void Unregister(IDamageable unit) => (unit.Team == Team.Player ? players : enemies).Remove(unit);

        public IDamageable FindTarget(CombatUnit seeker)
        {
            var candidates = seeker.Team == Team.Player ? enemies : players;
            IDamageable nearest = null; float best = seeker.Data.DetectionRange * CombatUnit.PixelsPerUnit;
            float bestSquared = best * best;
            Vector3 origin = seeker.TargetTransform.localPosition;
            for (int i = 0; i < candidates.Count; i++)
            {
                IDamageable candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || !candidate.TargetTransform.gameObject.activeInHierarchy) continue;
                float squared = (candidate.TargetTransform.localPosition - origin).sqrMagnitude;
                if (squared < bestSquared) { bestSquared = squared; nearest = candidate; }
            }
            if (nearest != null) return nearest;
            return seeker.Team == Team.Enemy && PlayerBase != null && PlayerBase.IsAlive ? PlayerBase : null;
        }

        public IDamageable FindNearestEnemy(IDamageable seeker,float detectionRange)
        {
            IDamageable nearest=null;float bestSquared=detectionRange*detectionRange*CombatUnit.PixelsPerUnit*CombatUnit.PixelsPerUnit;Vector3 origin=seeker.TargetTransform.localPosition;
            for(int i=0;i<enemies.Count;i++){IDamageable candidate=enemies[i];if(candidate==null||!candidate.IsAlive||!candidate.TargetTransform.gameObject.activeInHierarchy)continue;float squared=(candidate.TargetTransform.localPosition-origin).sqrMagnitude;if(squared<bestSquared){bestSquared=squared;nearest=candidate;}}
            return nearest;
        }
        public void CollectEnemies(List<IDamageable> output){output.Clear();for(int i=0;i<enemies.Count;i++)if(enemies[i]!=null&&enemies[i].IsAlive)output.Add(enemies[i]);}
        public void CollectPlayers(List<IDamageable> output){output.Clear();for(int i=0;i<players.Count;i++)if(players[i]!=null&&players[i].IsAlive)output.Add(players[i]);}

        public static bool CanAttack(IDamageable source, IDamageable target) =>
            source != null && target != null && source.IsAlive && target.IsAlive && source.Team != target.Team;
    }
}
