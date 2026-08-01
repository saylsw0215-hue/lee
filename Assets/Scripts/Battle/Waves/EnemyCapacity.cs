namespace HeroDefense.Battle.Waves
{
    /// <summary>Centralized active-enemy cap used by wave scheduling and tests.</summary>
    public static class EnemyCapacity
    {
        public const int MaximumActive=35;
        public static bool CanSpawn(int active)=>active<MaximumActive;
    }
}
