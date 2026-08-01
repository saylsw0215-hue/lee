using System.Collections.Generic;

namespace HeroDefense.Save
{
    public interface ISaveMigration{int FromVersion{get;}int ToVersion{get;}bool Apply(GameSaveData data);}
    public sealed class SaveMigrationV0ToV1:ISaveMigration{public int FromVersion=>0;public int ToVersion=>1;public bool Apply(GameSaveData data){if(data==null)return false;data.profile??=new();data.currencies??=new();data.settings??=new();data.saveVersion=1;return true;}}
    public sealed class SaveMigrationService
    {
        private readonly List<ISaveMigration> migrations=new(){new SaveMigrationV0ToV1()};public SaveMigrationService(IEnumerable<ISaveMigration> custom=null){if(custom!=null){migrations.Clear();migrations.AddRange(custom);}migrations.Sort((a,b)=>a.FromVersion.CompareTo(b.FromVersion));}
        public bool TryMigrate(GameSaveData data){if(data==null||data.saveVersion>GameSaveData.CurrentVersion)return false;while(data.saveVersion<GameSaveData.CurrentVersion){ISaveMigration match=null;for(int i=0;i<migrations.Count;i++)if(migrations[i].FromVersion==data.saveVersion){match=migrations[i];break;}if(match==null||match.ToVersion<=match.FromVersion||!match.Apply(data))return false;data.saveVersion=match.ToVersion;}return true;}
    }
}
