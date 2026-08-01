using System.Collections.Generic;
using HeroDefense.Battle.Combat;

namespace HeroDefense.Heroes.Skills
{
    /// <summary>Pure friendly-fire and duplicate-hit guard for one area-skill execution.</summary>
    public static class HeroSkillRules
    {
        public static bool TryAccept(IDamageable source,IDamageable target,HashSet<IDamageable> hit)
        {return source!=null&&target!=null&&target.IsAlive&&source.Team!=target.Team&&hit.Add(target);}
    }
}
