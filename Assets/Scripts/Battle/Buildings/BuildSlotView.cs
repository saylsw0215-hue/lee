using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.Battle.Buildings
{
    /// <summary>Shared-canvas visual and input surface for one build slot.</summary>
    [RequireComponent(typeof(RectTransform),typeof(Image),typeof(Button))]
    public sealed class BuildSlotView:MonoBehaviour
    {
        private static readonly Color Empty=new(.12f,.34f,.3f,.48f),Highlighted=new(.95f,.68f,.14f,.72f),DropTarget=new(1f,.86f,.28f,.94f),Occupied=new(.08f,.11f,.16f,.72f);
        public BuildSlotState State{get;private set;} public RectTransform RectTransform{get;private set;}
        private Image image; private Text label;
        public void Initialize(string id,System.Action<BuildSlotView> clicked)
        {
            State=new BuildSlotState(id);RectTransform=GetComponent<RectTransform>();image=GetComponent<Image>();image.sprite=UI.UiFactory.RoundedSprite;image.type=Image.Type.Sliced;
            label=UI.UiFactory.Label(transform,"SlotLabel",id,18,TextAnchor.MiddleCenter,new Color(.8f,1f,.9f));label.raycastTarget=false;
            GetComponent<Button>().onClick.AddListener(()=>clicked(this));Refresh(false);
        }
        public void Refresh(bool buildSelected,bool isDropTarget=false)
        {
            image.color=State.IsOccupied?Occupied:isDropTarget?DropTarget:buildSelected?Highlighted:Empty;
            label.text=State.IsOccupied?string.Empty:isDropTarget?"여기에 놓기":$"✦ 건설 {State.SlotId} ✦";
        }
    }
}
