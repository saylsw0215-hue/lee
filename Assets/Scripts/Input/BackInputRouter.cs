using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeroDefense.Input
{
    /// <summary>Owns Escape/Android-back input and exposes a single event.</summary>
    public sealed class BackInputRouter : MonoBehaviour
    {
        public event Action BackPressed;
        private InputAction back;

        private void Awake()
        {
            back = new InputAction("Back", binding: "<Keyboard>/escape");
            back.AddBinding("<AndroidGamepad>/buttonEast");
            back.performed += OnBack;
        }
        private void OnEnable() => back?.Enable();
        private void OnDisable() => back?.Disable();
        private void OnDestroy() { if (back != null) { back.performed -= OnBack; back.Dispose(); } }
        private void OnBack(InputAction.CallbackContext context) => BackPressed?.Invoke();
    }
}
