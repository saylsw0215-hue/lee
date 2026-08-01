using UnityEngine;

namespace HeroDefense.Battle.Production
{
    /// <summary>Deterministic production timer that can remain complete while unit capacity is full.</summary>
    public sealed class ProductionTimer
    {
        public float Elapsed {get;private set;}
        public float Progress(float interval)=>interval<=0f?1f:Mathf.Clamp01(Elapsed/interval);
        public bool Tick(float deltaTime,float interval){Elapsed=Mathf.Min(Mathf.Max(.01f,interval),Elapsed+Mathf.Max(0f,deltaTime));return Elapsed>=interval;}
        public void Consume()=>Elapsed=0f;
        public void PreserveProgress(float oldInterval,float newInterval){float ratio=Progress(oldInterval);Elapsed=Mathf.Clamp01(ratio)*Mathf.Max(.01f,newInterval);}
        public void Reset()=>Elapsed=0f;
    }
}
