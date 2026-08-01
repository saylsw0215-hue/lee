using System.Collections.Generic;
using HeroDefense.Meta;
using HeroDefense.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.Achievements
{
    /// <summary>Persistent, unscaled notification queue for newly completed achievements.</summary>
    public sealed class AchievementNotificationController:MonoBehaviour
    {
        private readonly Queue<string> queue=new();private GameObject root;private Text label;private float remaining;
        private void Awake(){DontDestroyOnLoad(gameObject);var canvas=UiFactory.CreateCanvas();canvas.GetComponent<Canvas>().sortingOrder=700;DontDestroyOnLoad(canvas.gameObject);root=UiFactory.Panel(canvas,"AchievementToast",new Color(.12f,.08f,.02f,.96f),new Vector2(.32f,.82f),new Vector2(.68f,.96f)).gameObject;label=UiFactory.Label(root.transform,"Text","",29,TextAnchor.MiddleCenter,new Color(1,.86f,.3f));root.SetActive(false);AchievementService.Completed+=Enqueue;}
        private void Enqueue(string name){queue.Enqueue(name);if(!root.activeSelf)ShowNext();}
        private void ShowNext(){if(queue.Count==0){root.SetActive(false);return;}label.text="업적 달성!\n"+queue.Dequeue();root.SetActive(true);remaining=2.5f;}
        private void Update(){if(!root.activeSelf)return;remaining-=Time.unscaledDeltaTime;if(remaining<=0)ShowNext();}
        private void OnDestroy(){AchievementService.Completed-=Enqueue;}
    }
}
