using System;
using HeroDefense.Save;
using HeroDefense.Meta;

namespace HeroDefense.Collection
{
    public enum CollectionEvent{Encountered,Used,Defeated,Cleared}
    public static class CollectionService
    {
        public static event Action<string> Discovered;
        public static ContentRecord Get(string id){var save=SaveGameManager.Instance;if(save==null||string.IsNullOrEmpty(id))return null;for(int i=0;i<save.Data.collection.Count;i++)if(save.Data.collection[i].id==id)return save.Data.collection[i];var record=new ContentRecord{id=id};save.Data.collection.Add(record);return record;}
        public static void Record(string id,CollectionEvent action){var record=Get(id);if(record==null)return;bool first=!record.discovered;record.discovered=true;record.count++;if(string.IsNullOrEmpty(record.firstSeenUtc))record.firstSeenUtc=DateTime.UtcNow.ToString("O");if(action==CollectionEvent.Defeated)record.defeated=true;if(first)Discovered?.Invoke(id);AchievementService.Evaluate(SaveGameManager.Instance);SaveGameManager.Instance.NotifyChanged();}
    }
}
