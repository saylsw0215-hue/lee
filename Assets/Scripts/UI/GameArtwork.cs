using UnityEngine;
using UnityEngine.UI;

namespace HeroDefense.UI
{
    /// <summary>Creates a full-screen, aspect-filled image from the user-provided medieval artwork.</summary>
    public static class GameArtwork
    {
        private static readonly System.Collections.Generic.Dictionary<string,Sprite> Sprites=new();

        public static RectTransform AddMainMenuBackground(Transform parent,string name="KingdomBackground")=>AddBackground(parent,"Backgrounds/MainKingdom",name);
        public static RectTransform AddStageBackground(Transform parent,string stageId,string name="StageBackground")
        {
            string path=stageId switch{"stage_02_red_canyon"=>"Backgrounds/StageGoldenDesert","stage_03_frozen_fortress"=>"Backgrounds/StageDeepForest","stage_04_dead_sanctuary"=>"Backgrounds/StageDarkFortress",_=>"Backgrounds/StageGrassland"};
            return AddBackground(parent,path,name);
        }
        public static Sprite GetBuildingSprite(string buildingId)=>LoadSprite("BuildingArt/"+buildingId);
        public static void AddBattleWorldDecorations(RectTransform world)
        {
            AddProp(world,"castle_gate",new Vector2(-690,-18),new Vector2(245,245),.9f);
            AddProp(world,"wall",new Vector2(-510,12),new Vector2(205,150),.82f);
            AddProp(world,"watchtower",new Vector2(675,28),new Vector2(185,225),.88f);
            AddProp(world,"tree_oak",new Vector2(-350,155),new Vector2(150,175),.72f);
            AddProp(world,"tree_pine",new Vector2(360,155),new Vector2(125,165),.66f);
            AddProp(world,"fence",new Vector2(-210,-188),new Vector2(190,105),.72f);
            AddProp(world,"signpost",new Vector2(500,-150),new Vector2(85,120),.9f);
            AddProp(world,"lantern",new Vector2(-420,-125),new Vector2(75,130),.92f);
            AddProp(world,"barrel",new Vector2(-565,-165),new Vector2(74,90),.94f);
            AddProp(world,"chest",new Vector2(585,-175),new Vector2(100,82),.96f);
            AddProp(world,"flowers",new Vector2(155,-205),new Vector2(125,70),.85f);
            AddProp(world,"well",new Vector2(20,130),new Vector2(115,125),.72f);
        }
        private static void AddProp(RectTransform parent,string id,Vector2 position,Vector2 size,float alpha)
        {
            Sprite sprite=Core.RuntimeArtworkCatalog.Load("WorldProps/"+id);if(sprite==null)return;
            var go=new GameObject("WorldProp_"+id,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
            var image=go.GetComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;image.color=new Color(1,1,1,alpha);
        }
        private static RectTransform AddBackground(Transform parent,string path,string name)
        {
            Texture2D texture=Resources.Load<Texture2D>(path);
            if(texture==null)return UiFactory.Panel(parent,name,new Color(.08f,.14f,.13f),Vector2.zero,Vector2.one);
            Sprite sprite=LoadSprite(path);
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(AspectRatioFitter));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;
            var image=go.GetComponent<Image>();image.sprite=sprite;image.color=Color.white;image.raycastTarget=false;
            var fitter=go.GetComponent<AspectRatioFitter>();fitter.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fitter.aspectRatio=(float)texture.width/texture.height;
            return rect;
        }
        private static Sprite LoadSprite(string path)
        {
            if(Sprites.TryGetValue(path,out Sprite sprite)&&sprite!=null)return sprite;
            Texture2D texture=Resources.Load<Texture2D>(path);if(texture==null)return null;
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);Sprites[path]=sprite;return sprite;
        }

        public static void AddReadabilityOverlay(Transform parent,float opacity)
        {
            RectTransform overlay=UiFactory.Panel(parent,"BackgroundReadabilityOverlay",new Color(.015f,.025f,.035f,Mathf.Clamp01(opacity)),Vector2.zero,Vector2.one);
            overlay.GetComponent<Image>().raycastTarget=false;
        }
    }
}
