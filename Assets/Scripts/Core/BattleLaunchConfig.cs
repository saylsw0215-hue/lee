using HeroDefense.Battle.Stages;

namespace HeroDefense.Core
{
    public enum GameDifficulty { Easy, Normal, Hard }
    public enum GameMode { Stage, Endless }

    /// <summary>Session-only launch choice preserved across Battle replay without mutating content assets.</summary>
    public static class BattleLaunchConfig
    {
        public static string SelectedStageId { get; private set; } = "stage_01_grassland";
        public static GameDifficulty Difficulty { get; private set; } = GameDifficulty.Normal;
        public static GameMode Mode { get; private set; } = GameMode.Stage;
        public static StageData SelectedStage => RuntimeStageCatalog.GetById(SelectedStageId) ?? RuntimeStageCatalog.GetStageOne();
        public static void Configure(string stageId, GameDifficulty difficulty, GameMode mode)
        { SelectedStageId=string.IsNullOrWhiteSpace(stageId)?"stage_01_grassland":stageId;Difficulty=difficulty;Mode=mode; }
        public static void Reset()=>Configure("stage_01_grassland",GameDifficulty.Normal,GameMode.Stage);
    }

    public readonly struct DifficultyModifiers
    {
        public readonly float EnemyHealth,EnemyDamage,EnemySpeed,Gold,Experience,BaseHealth,StartingGold;
        public DifficultyModifiers(float hp,float damage,float speed,float gold,float xp,float baseHp,float startGold)
        {EnemyHealth=hp;EnemyDamage=damage;EnemySpeed=speed;Gold=gold;Experience=xp;BaseHealth=baseHp;StartingGold=startGold;}
        public static DifficultyModifiers For(GameDifficulty value)=>value switch
        {
            GameDifficulty.Easy=>new(.8f,.8f,.95f,1.1f,1.1f,1.2f,1.2f),
            GameDifficulty.Hard=>new(1.45f,1.3f,1.08f,1.05f,1.15f,.9f,.9f),
            _=>new(1,1,1,1,1,1,1)
        };
    }
}
