namespace HeroDefense.Battle.Production
{
    /// <summary>Configurable guard used before automatic player-unit production.</summary>
    public sealed class PlayerUnitLimitService
    {
        public int Maximum {get;}
        public PlayerUnitLimitService(int maximum=30){Maximum=maximum;}
        public bool CanProduce(int activePlayers)=>activePlayers<Maximum;
    }
}
