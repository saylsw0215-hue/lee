using System.IO;
using HeroDefense.Battle.Combat;
using UnityEditor;
using UnityEngine;

namespace HeroDefense.Editor
{
    /// <summary>Idempotently creates editable Phase 2 unit data and validates combat prerequisites.</summary>
    public static class Phase2Setup
    {
        private const string DataFolder = "Assets/Resources/UnitData";

        [MenuItem("Tools/Hero Defense/Setup Phase 2")]
        public static void Setup()
        {
            Phase1Setup.Setup();
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory("Assets/Prefabs/Combat/Player"); Directory.CreateDirectory("Assets/Prefabs/Combat/Enemy"); Directory.CreateDirectory("Assets/Prefabs/Combat/Effects");
            CreateIfMissing("PlayerSwordsman", "player_swordsman", "검사", Team.Player, 100, 2f, 20, 1.1f, 1f, 8, .42f, 0, new Color(.12f,.42f,.88f), UnitVisualShape.Swordsman);
            CreateIfMissing("EnemySlime", "enemy_slime", "슬라임", Team.Enemy, 60, 1.6f, 8, 1f, 1.2f, 7, .48f, 10, new Color(.15f,.68f,.26f), UnitVisualShape.Slime);
            CreateIfMissing("EnemyGoblin", "enemy_goblin", "고블린", Team.Enemy, 90, 2.2f, 12, 1f, .9f, 8, .43f, 15, new Color(.78f,.38f,.12f), UnitVisualShape.Goblin);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            ValidateAsset("PlayerSwordsman"); ValidateAsset("EnemySlime"); ValidateAsset("EnemyGoblin");
            Debug.Log("Hero Defense Phase 2 setup complete. Existing UnitData assets were preserved.");
        }

        private static void CreateIfMissing(string file, string id, string displayName, Team team, float hp, float speed, float damage, float range, float interval, float detection, float radius, int reward, Color color, UnitVisualShape shape)
        {
            string path = $"{DataFolder}/{file}.asset"; if (AssetDatabase.LoadAssetAtPath<UnitData>(path) != null) return;
            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.Configure(id, displayName, team, hp, speed, damage, range, interval, detection, radius, reward, color, shape);
            AssetDatabase.CreateAsset(data, path);
        }
        private static void ValidateAsset(string file)
        {
            UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>($"{DataFolder}/{file}.asset");
            if (data == null) throw new System.InvalidOperationException($"Missing UnitData {file}.");
            if (!data.Validate(out string reason)) throw new System.InvalidOperationException($"Invalid UnitData {file}: {reason}");
        }
    }
}
