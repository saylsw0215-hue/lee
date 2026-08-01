using UnityEngine;

namespace HeroDefense.Heroes.Selection
{
    /// <summary>Persistent single-selection session with a safe knight fallback.</summary>
    public sealed class HeroSelectionService:MonoBehaviour
    {
        public static HeroSelectionService Instance{get;private set;}public HeroData SelectedHero{get;private set;}
        private void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);EnsureDefault();}
        public void Select(HeroData data){SelectedHero=data;EnsureDefault();}
        public HeroData GetSelectedOrDefault(){EnsureDefault();return SelectedHero;}
        private void EnsureDefault(){if(SelectedHero==null)SelectedHero=RuntimeHeroCatalog.GetDefault();}
    }
}
