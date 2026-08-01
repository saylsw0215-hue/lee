using HeroDefense.Battle.Combat;
using UnityEngine;

namespace HeroDefense.Battle.Waves
{
    public static class EndlessSession{public static int CurrentWave{get;internal set;}=1;}
    /// <summary>Deterministically produces endless wave scaling, elite cadence and rotating bosses.</summary>
    public static class EndlessWaveGenerator
    {
        public static float HealthMultiplier(int wave)=>1f+Mathf.Max(0,wave-1)*.08f+Mathf.Max(0,wave-20)*.02f;
        public static float DamageMultiplier(int wave)=>1f+Mathf.Max(0,wave-1)*.05f+Mathf.Max(0,wave-20)*.01f;
        public static bool IsElite(int wave)=>wave>0&&wave%5==0&&wave%10!=0; public static bool IsBoss(int wave)=>wave>0&&wave%10==0;
        public static UnitData BossForWave(int wave)
        {string[] ids={"BossGoblinChieftain","BossOrcWarlord","BossFrostQueen","BossDeathKnight"};return RuntimeUnitCatalog.Get(ids[Mathf.Max(0,wave/10-1)%ids.Length]);}
    }
}
