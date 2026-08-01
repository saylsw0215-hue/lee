using System.Collections;
using HeroDefense.Battle.Effects;
using UnityEngine;
using UnityEngine.UI;
using HeroDefense.Core;

namespace HeroDefense.Battle.Combat
{
    /// <summary>Owns placeholder visuals and feedback independently of combat decisions.</summary>
    public sealed class UnitVisualController : MonoBehaviour
    {
        private Image body;
        private RectTransform bodyRect;
        private WorldHealthBar healthBar;
        private Color baseColor;
        private Coroutine feedback;
        private Text statusLabel;
        private Vector3 lastPosition;
        private float walkBlend;
        private int facing=1;

        public void Build(UnitData data)
        {
            var bodyObject=new GameObject("AnimatedBody",typeof(RectTransform),typeof(Image));bodyObject.transform.SetParent(transform,false);body=bodyObject.GetComponent<Image>();body.sprite=RuntimeArtworkCatalog.Unit(data.UnitId);body.preserveAspect=body.sprite!=null;body.color=body.sprite!=null?Color.white:data.PlaceholderColor;baseColor=body.color;
            bodyRect = bodyObject.GetComponent<RectTransform>();
            bodyRect.sizeDelta = body.sprite!=null?(data.UnitId.StartsWith("boss_")?new Vector2(155,185):data.UnitId.Contains("elite")?new Vector2(120,145):data.VisualShape==UnitVisualShape.Slime?new Vector2(92,72):new Vector2(100,125)):data.VisualShape switch { UnitVisualShape.Slime => new Vector2(70, 54), UnitVisualShape.Goblin => new Vector2(54, 82), UnitVisualShape.Archer=>new Vector2(52,86),UnitVisualShape.Mage=>new Vector2(60,96),UnitVisualShape.EliteSlime=>new Vector2(100,76),UnitVisualShape.EliteGoblin=>new Vector2(78,112),UnitVisualShape.BossGoblin=>new Vector2(126,170), _ => new Vector2(58, 92) };GetComponent<RectTransform>().sizeDelta=bodyRect.sizeDelta;lastPosition=transform.localPosition;
            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text)); labelObject.transform.SetParent(transform, false);
            var label = labelObject.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.text = data.DisplayName; label.fontSize = 19; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white;
            var labelRect = labelObject.GetComponent<RectTransform>(); labelRect.sizeDelta = new Vector2(110, 30); labelRect.anchoredPosition = new Vector2(0, -58);
            var statusObject=new GameObject("StatusIcons",typeof(RectTransform),typeof(Text));statusObject.transform.SetParent(transform,false);statusLabel=statusObject.GetComponent<Text>();statusLabel.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");statusLabel.fontSize=16;statusLabel.alignment=TextAnchor.MiddleCenter;statusLabel.color=Color.white;var statusRect=statusObject.GetComponent<RectTransform>();statusRect.sizeDelta=new Vector2(150,26);statusRect.anchoredPosition=new Vector2(0,86);
            if(body.sprite==null){if (data.VisualShape == UnitVisualShape.Swordsman) AddWeapon(new Vector2(38, 3), new Vector2(10, 64), new Color(.82f,.88f,.96f));
            if (data.VisualShape == UnitVisualShape.Goblin) AddWeapon(new Vector2(-35, -4), new Vector2(12, 48), new Color(.36f,.2f,.08f));
            if (data.VisualShape == UnitVisualShape.Archer) AddWeapon(new Vector2(35, 2), new Vector2(8, 62), new Color(.62f,.35f,.12f));
            if (data.VisualShape == UnitVisualShape.Mage) AddWeapon(new Vector2(38, -2), new Vector2(10, 76), new Color(.65f,.28f,.9f));
            if (data.VisualShape == UnitVisualShape.EliteGoblin) AddWeapon(new Vector2(-50,-4),new Vector2(18,82),new Color(.85f,.15f,.1f));
            if (data.VisualShape == UnitVisualShape.BossGoblin) AddWeapon(new Vector2(-78,-5),new Vector2(28,125),new Color(.95f,.55f,.08f));}
            if (data.VisualShape==UnitVisualShape.EliteSlime||data.VisualShape==UnitVisualShape.EliteGoblin||data.VisualShape==UnitVisualShape.BossGoblin)
            {var aura=new GameObject("EliteAura",typeof(RectTransform),typeof(Image));aura.transform.SetParent(transform,false);aura.transform.SetAsFirstSibling();var ar=aura.GetComponent<RectTransform>();ar.sizeDelta=bodyRect.sizeDelta*1.35f;aura.GetComponent<Image>().color=data.VisualShape==UnitVisualShape.BossGoblin?new Color(1,.1f,.05f,.25f):new Color(.65f,.2f,.9f,.22f);}
            healthBar = new WorldHealthBar(transform, new Vector2(0, 62));
        }
        private void AddWeapon(Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject("Weapon", typeof(RectTransform), typeof(Image)); go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position; rect.localRotation = Quaternion.Euler(0,0,-18);
            go.GetComponent<Image>().color = color;
        }
        public void SetHealth(float current, float maximum) => healthBar.Set(current, maximum);
        public void SetStatuses(System.Collections.Generic.IReadOnlyList<StatusEffectInstance> values){if(statusLabel==null)return;statusLabel.text=values.Count==0?string.Empty:values[0].Data.DisplayName+(values[0].Stacks>1?$" x{values[0].Stacks}":string.Empty);statusLabel.color=values.Count==0?Color.white:values[0].Data.Color;}
        private void LateUpdate()
        {
            if(bodyRect==null||feedback!=null)return;Vector3 current=transform.localPosition;float dx=current.x-lastPosition.x;float speed=(current-lastPosition).sqrMagnitude/Mathf.Max(.0001f,Time.deltaTime*Time.deltaTime);bool moving=speed>4f;if(Mathf.Abs(dx)>.05f)facing=dx>0?1:-1;walkBlend=Mathf.MoveTowards(walkBlend,moving?1:0,Time.deltaTime*8f);float phase=Time.time*9f;float bob=(moving?Mathf.Abs(Mathf.Sin(phase))*5f:Mathf.Sin(Time.time*2.4f)*1.5f);float tilt=moving?Mathf.Sin(phase)*3.8f:Mathf.Sin(Time.time*1.8f)*.8f;bodyRect.anchoredPosition=new Vector2(0,bob);bodyRect.localRotation=Quaternion.Euler(0,0,-tilt*facing);bodyRect.localScale=new Vector3(facing*(1f+walkBlend*.025f),1f-walkBlend*.025f,1);lastPosition=current;
        }
        public void ResetVisual() { if (feedback != null) StopCoroutine(feedback); feedback=null;transform.localScale = Vector3.one;bodyRect.anchoredPosition=Vector2.zero;bodyRect.localRotation=Quaternion.identity;bodyRect.localScale=Vector3.one;body.color = baseColor;lastPosition=transform.localPosition; }
        public void PlayAttack() { if (feedback != null) StopCoroutine(feedback); feedback = StartCoroutine(Pulse(1.18f, .14f)); }
        public void PlayHit() { if (feedback != null) StopCoroutine(feedback); feedback = StartCoroutine(Flash()); }
        private IEnumerator Pulse(float scale, float duration)
        {
            bodyRect.localScale = new Vector3(facing*scale, .9f, 1f);bodyRect.localRotation=Quaternion.Euler(0,0,-9f*facing); yield return new WaitForSeconds(duration);bodyRect.localScale=new Vector3(facing,1,1);bodyRect.localRotation=Quaternion.identity;feedback=null;
        }
        private IEnumerator Flash()
        {
            body.color = body.sprite!=null?new Color(1f,.48f,.48f):Color.white;bodyRect.localRotation=Quaternion.Euler(0,0,7f*facing);yield return new WaitForSeconds(.09f); body.color = baseColor;bodyRect.localRotation=Quaternion.identity;feedback=null;
        }
    }
}
