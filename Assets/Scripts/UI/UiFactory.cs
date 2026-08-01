using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HeroDefense.UI
{
    /// <summary>Small runtime UI factory used by the dependency-free prototype scenes.</summary>
    public static class UiFactory
    {
        public static readonly Color Navy = new(0.045f, 0.065f, 0.11f, 0.88f);
        public static readonly Color Gold = new(0.95f, 0.7f, 0.2f, 1f);
        private static Sprite roundedSprite;private static Font cuteFont;
        public static Sprite RoundedSprite=>roundedSprite??=CreateRoundedSprite();
        public static Font CuteFont
        {
            get
            {
                if(cuteFont!=null)return cuteFont;
                cuteFont=Resources.Load<Font>("Fonts/NotoSansKR");
                if(cuteFont!=null)return cuteFont;
                cuteFont=Font.CreateDynamicFontFromOSFont(new[]{"Arial Rounded MT Bold","Apple SD Gothic Neo","Arial"},32);
                return cuteFont!=null?cuteFont:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        public static RectTransform CreateCanvas(string name = "GameCanvas")
        {
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            return canvasObject.GetComponent<RectTransform>();
        }

        public static RectTransform Panel(Transform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); Stretch(rect, min, max);
            var image=go.GetComponent<Image>();image.sprite=RoundedSprite;image.type=Image.Type.Sliced;image.color = color;
            if(color.a>.15f)
            {
                var shadow=go.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,Mathf.Min(.42f,color.a*.42f));shadow.effectDistance=new Vector2(3,-4);
                var outline=go.AddComponent<Outline>();outline.effectColor=new Color(1f,.76f,.35f,Mathf.Min(.22f,color.a*.22f));outline.effectDistance=new Vector2(1,-1);
                if(color.a>.35f&&!name.Contains("Fill")&&!name.Contains("Overlay"))
                {var shineObject=new GameObject("GlassHighlight",typeof(RectTransform),typeof(Image));shineObject.transform.SetParent(go.transform,false);var shine=shineObject.GetComponent<Image>();shine.sprite=RoundedSprite;shine.type=Image.Type.Sliced;shine.color=new Color(1,1,1,.055f);shine.raycastTarget=false;Stretch(shine.rectTransform,new Vector2(.015f,.68f),new Vector2(.985f,.975f));}
            }
            return rect;
        }

        public static Text Label(Transform parent, string name, string value, int size, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>(); text.text = value; text.font = CuteFont;
            text.fontSize = size; text.alignment = alignment; text.color = color; text.resizeTextForBestFit = true;
            var shadow=go.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.55f);shadow.effectDistance=new Vector2(2,-2);
            Stretch(go.GetComponent<RectTransform>(), Vector2.zero, Vector2.one); return text;
        }

        public static Button Button(Transform parent, string name, string caption, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement)); go.transform.SetParent(parent, false);
            var image=go.GetComponent<Image>();image.sprite=RoundedSprite;image.type=Image.Type.Sliced;image.color = color;
            var layout = go.GetComponent<LayoutElement>(); layout.preferredWidth = 310; layout.preferredHeight = 96; layout.minHeight = 72;
            var button = go.GetComponent<Button>(); var colors = button.colors; colors.highlightedColor = Color.Lerp(color, Color.white, .2f); colors.pressedColor = Color.Lerp(color, Color.black, .2f); button.colors = colors;
            var shadow=go.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.52f);shadow.effectDistance=new Vector2(4,-5);
            var outline=go.AddComponent<Outline>();outline.effectColor=Color.Lerp(new Color(1f,.72f,.26f,.62f),color,.35f);outline.effectDistance=new Vector2(2,-2);
            var glossObject=new GameObject("SoftGloss",typeof(RectTransform),typeof(Image));glossObject.transform.SetParent(go.transform,false);var gloss=glossObject.GetComponent<Image>();gloss.sprite=RoundedSprite;gloss.type=Image.Type.Sliced;gloss.color=new Color(1,1,1,.075f);gloss.raycastTarget=false;Stretch(gloss.rectTransform,new Vector2(.025f,.54f),new Vector2(.975f,.94f));
            var accentObject=new GameObject("GoldAccent",typeof(RectTransform),typeof(Image));accentObject.transform.SetParent(go.transform,false);var accent=accentObject.GetComponent<Image>();accent.sprite=RoundedSprite;accent.type=Image.Type.Sliced;accent.color=new Color(1f,.73f,.22f,.72f);accent.raycastTarget=false;Stretch(accent.rectTransform,new Vector2(.018f,.13f),new Vector2(.034f,.87f));
            Label(go.transform, "Label", caption, 34, TextAnchor.MiddleCenter, Color.white);
            return button;
        }

        public static RectTransform Vertical(Transform parent, string name, float spacing = 22)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup)); go.transform.SetParent(parent, false);
            var group = go.GetComponent<VerticalLayoutGroup>(); group.spacing = spacing; group.childAlignment = TextAnchor.MiddleCenter; group.childControlHeight = true; group.childControlWidth = true;
            return go.GetComponent<RectTransform>();
        }

        public static RectTransform Horizontal(Transform parent, string name, float spacing = 20)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup)); go.transform.SetParent(parent, false);
            var group = go.GetComponent<HorizontalLayoutGroup>(); group.spacing = spacing; group.childAlignment = TextAnchor.MiddleCenter; group.childControlHeight = true; group.childControlWidth = true;
            return go.GetComponent<RectTransform>();
        }

        public static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }

        private static Sprite CreateRoundedSprite()
        {
            const int size=48,radius=13;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="RuntimeRoundedPanel"};var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){float dx=Mathf.Max(radius-x-1,x-(size-radius),0),dy=Mathf.Max(radius-y-1,y-(size-radius),0);float distance=Mathf.Sqrt(dx*dx+dy*dy);byte alpha=(byte)Mathf.RoundToInt(Mathf.Clamp01(radius+.5f-distance)*255);pixels[y*size+x]=new Color32(255,255,255,alpha);}
            texture.SetPixels32(pixels);texture.Apply(false,true);return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(radius,radius,radius,radius));
        }
    }
}
