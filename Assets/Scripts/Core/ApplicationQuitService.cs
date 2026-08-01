using UnityEngine;

namespace HeroDefense.Core
{
    /// <summary>Centralizes platform-safe application quit behavior.</summary>
    public static class ApplicationQuitService
    {
        public static void Quit()
        {
            HeroDefense.Save.SaveGameManager.Instance?.SaveNow(HeroDefense.Save.SaveReason.ApplicationQuit);
#if UNITY_EDITOR
            Debug.Log("Application quit requested (ignored in Unity Editor).");
#else
            Application.Quit();
#endif
        }
    }
}
