using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroDefense.Core
{
    /// <summary>Serializes scene transitions and reports load failures.</summary>
    public sealed class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Load(string sceneName)
        {
            if (!IsLoading) StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;
            Time.timeScale = 1f;
            AsyncOperation operation = null;
            try { operation = SceneManager.LoadSceneAsync(sceneName); }
            catch (Exception exception) { Debug.LogError($"Could not start loading '{sceneName}': {exception}"); }
            if (operation == null) { IsLoading = false; yield break; }
            while (!operation.isDone) yield return null;
            IsLoading = false;
        }

        private void OnDestroy()
        {
            if(Instance==this)Instance=null;
        }
    }
}
