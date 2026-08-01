using HeroDefense.Save;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace HeroDefense.UI
{
    /// <summary>Persistent non-technical notification for recoverable save failures.</summary>
    public sealed class SystemMessageController:MonoBehaviour
    {
        private static event Action<string> Notice;private GameObject root;private Text label;private float remaining;private void Awake(){DontDestroyOnLoad(gameObject);var canvas=UiFactory.CreateCanvas();canvas.GetComponent<Canvas>().sortingOrder=800;DontDestroyOnLoad(canvas.gameObject);root=UiFactory.Panel(canvas,"SystemMessage",new Color(.45f,.08f,.08f,.96f),new Vector2(.25f,.84f),new Vector2(.75f,.96f)).gameObject;label=UiFactory.Label(root.transform,"Text","",25,TextAnchor.MiddleCenter,Color.white);root.SetActive(false);SaveGameManager.SaveFailed+=OnSaveFailed;Notice+=OnNotice;}
        public static void Show(string message)=>Notice?.Invoke(message);private void OnSaveFailed(string error)=>OnNotice("게임 데이터를 저장하지 못했습니다.\n잠시 후 다시 시도합니다.");private void OnNotice(string value){label.text=value;root.SetActive(true);remaining=4;}
        private void Update(){if(!root.activeSelf)return;remaining-=Time.unscaledDeltaTime;if(remaining<=0)root.SetActive(false);}
        private void OnDestroy(){SaveGameManager.SaveFailed-=OnSaveFailed;Notice-=OnNotice;}
    }
}
