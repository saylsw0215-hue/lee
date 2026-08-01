using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.Battle
{
    /// <summary>Binds building buttons to an independent selection model.</summary>
    public sealed class BuildingSelectionController
    {
        private readonly BuildingSelectionModel model;
        private readonly Text status;
        private readonly Dictionary<BuildingType, Image> images = new();
        private readonly Color normal = new(.18f, .31f, .43f), selected = new(.85f, .58f, .12f);

        public BuildingSelectionController(Transform row, Transform statusParent, BuildingSelectionModel model)
        {
            this.model = model;
            Add(row, BuildingType.Barracks, "병영"); Add(row, BuildingType.ArcheryRange, "사격장"); Add(row, BuildingType.MagicSanctum, "마법소");
            status = UI.UiFactory.Label(statusParent, "SelectedBuilding", "선택된 건물: 없음", 27, TextAnchor.MiddleCenter, Color.white);
            model.Changed += Refresh;
        }
        private void Add(Transform row, BuildingType type, string caption)
        {
            var button = UI.UiFactory.Button(row, type.ToString(), caption, normal); images[type] = button.GetComponent<Image>();
            button.onClick.AddListener(() => model.Toggle(type));
        }
        private void Refresh(BuildingType? value)
        {
            foreach (var pair in images) pair.Value.color = pair.Key == value ? selected : normal;
            status.text = value.HasValue ? $"선택된 건물: {Display(value.Value)}" : "선택된 건물: 없음";
            Debug.Log(value.HasValue ? $"Selected building: {value.Value}" : "Building selection cleared");
        }
        private static string Display(BuildingType type) => type switch { BuildingType.Barracks => "병영", BuildingType.ArcheryRange => "사격장", _ => "마법소" };
        public void Dispose() => model.Changed -= Refresh;
    }
}
