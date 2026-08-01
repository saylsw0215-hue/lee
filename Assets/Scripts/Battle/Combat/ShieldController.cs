using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    public sealed class ShieldController
    {
        private sealed class Shield{public string Source;public float Amount,Remaining;}
        private readonly List<Shield> active=new(4); public event Action<float> Changed; public float Total{get{float sum=0;for(int i=0;i<active.Count;i++)sum+=active[i].Amount;return sum;}}
        public void Add(string source,float amount,float duration){if(amount<=0||duration<=0)return;active.Add(new Shield{Source=source,Amount=amount,Remaining=duration});Changed?.Invoke(Total);}
        public float Absorb(float damage){float remaining=Mathf.Max(0,damage),before=remaining;for(int i=0;i<active.Count&&remaining>0;){Shield shield=active[i];float used=Mathf.Min(shield.Amount,remaining);shield.Amount-=used;remaining-=used;if(shield.Amount<=0)active.RemoveAt(i);else i++;}float absorbed=before-remaining;if(absorbed>0)Changed?.Invoke(Total);return absorbed;}
        public void Tick(float dt){if(dt<=0)return;bool changed=false;for(int i=active.Count-1;i>=0;i--){active[i].Remaining-=dt;if(active[i].Remaining<=0){active.RemoveAt(i);changed=true;}}if(changed)Changed?.Invoke(Total);}
        public void RemoveSource(string source){bool changed=false;for(int i=active.Count-1;i>=0;i--)if(active[i].Source==source){active.RemoveAt(i);changed=true;}if(changed)Changed?.Invoke(Total);}public void Clear(){if(active.Count==0)return;active.Clear();Changed?.Invoke(0);}
    }
}
