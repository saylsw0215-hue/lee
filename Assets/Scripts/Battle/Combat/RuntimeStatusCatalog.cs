using System.Collections.Generic;
using UnityEngine;

namespace HeroDefense.Battle.Combat
{
    /// <summary>Runtime status definitions that bypass Unity Resources on macOS.</summary>
    public static class RuntimeStatusCatalog
    {
        private static readonly string[] Names={"Burn","Freeze","Invincible","Poison","ShamanPower","Shock","Silence","Slow","Stun","Taunt"};
        private static readonly Dictionary<string,StatusEffectData> Items=new();
        public static StatusEffectData[] GetAll(){var result=new StatusEffectData[Names.Length];for(int i=0;i<Names.Length;i++)result[i]=Get(Names[i]);return result;}
        public static StatusEffectData Get(string name)
        {
            if(Items.TryGetValue(name,out StatusEffectData item))return item;
            item=name switch
            {
                "Burn"=>Create("status_burn","화상",StatusEffectType.DamageOverTime,5,1,4,3,StatusRefreshRule.Stack,true,new Color(1,.25f,.05f)),
                "Freeze"=>Create("status_freeze","빙결",StatusEffectType.CrowdControl,2.5f,1,1,1,StatusRefreshRule.ReplaceIfStronger,true,new Color(.2f,.75f,1)),
                "Invincible"=>Create("status_invincible","무적",StatusEffectType.Buff,2,1,1,1,StatusRefreshRule.RefreshDuration,false,new Color(.3f,.9f,1)),
                "Poison"=>Create("status_poison","독",StatusEffectType.DamageOverTime,5,1,2,5,StatusRefreshRule.Stack,true,new Color(.3f,.8f,.15f)),
                "ShamanPower"=>Create("status_shaman_power","주술 강화",StatusEffectType.Buff,5,1,.15f,1,StatusRefreshRule.RefreshDuration,false,new Color(.8f,.25f,1)),
                "Shock"=>Create("status_shock","감전",StatusEffectType.Debuff,4,1,.15f,1,StatusRefreshRule.RefreshDuration,true,new Color(.7f,.55f,1)),
                "Silence"=>Create("status_silence","침묵",StatusEffectType.CrowdControl,3,1,1,1,StatusRefreshRule.RefreshDuration,true,new Color(.6f,.25f,.75f)),
                "Slow"=>Create("status_slow","둔화",StatusEffectType.Debuff,4,1,.3f,1,StatusRefreshRule.ReplaceIfStronger,true,new Color(.35f,.7f,1)),
                "Stun"=>Create("status_stun","기절",StatusEffectType.CrowdControl,1.5f,1,1,1,StatusRefreshRule.RefreshDuration,true,new Color(1,.85f,.2f)),
                "Taunt"=>Create("status_taunt","도발",StatusEffectType.CrowdControl,3,1,1,1,StatusRefreshRule.RefreshDuration,true,new Color(1,.25f,.2f)),
                _=>null
            };
            if(item!=null)Items[name]=item;else Debug.LogError("Unknown runtime status: "+name);return item;
        }
        private static StatusEffectData Create(string id,string display,StatusEffectType type,float duration,float tick,float potency,int stacks,StatusRefreshRule rule,bool resist,Color color)
        {var data=ScriptableObject.CreateInstance<StatusEffectData>();data.name=id;data.hideFlags=HideFlags.DontSave;data.Configure(id,display,display,type,duration,tick,potency,stacks,rule,resist,true,color);return data;}
    }
}
