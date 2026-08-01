using System;

namespace HeroDefense.Battle
{
    public enum BuildingType { Barracks, ArcheryRange, MagicSanctum }

    /// <summary>UI-independent toggle-selection model ready for future data assets.</summary>
    public sealed class BuildingSelectionModel
    {
        public event Action<BuildingType?> Changed;
        public BuildingType? Selected { get; private set; }
        public void Toggle(BuildingType type)
        {
            Selected = Selected == type ? null : type;
            Changed?.Invoke(Selected);
        }
        public void Clear() { if (!Selected.HasValue) return; Selected = null; Changed?.Invoke(Selected); }
    }
}
