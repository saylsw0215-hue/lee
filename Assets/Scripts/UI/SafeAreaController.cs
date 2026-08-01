using UnityEngine;

namespace HeroDefense.UI
{
    /// <summary>Keeps its RectTransform inside the current device safe area.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaController : MonoBehaviour
    {
        private RectTransform target;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;

        private void Awake() { target = GetComponent<RectTransform>(); Apply(); }
        private void OnEnable() => Apply();
        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreen.x != Screen.width || lastScreen.y != Screen.height) Apply();
        }

        private void Apply()
        {
            if (target == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect area = Screen.safeArea;
            target.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            target.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            target.offsetMin = target.offsetMax = Vector2.zero;
            lastSafeArea = area;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
