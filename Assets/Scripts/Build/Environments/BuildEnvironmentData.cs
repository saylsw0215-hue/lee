using UnityEngine;

namespace HeroDefense.Build
{
    public enum HeroDefenseBuildType{Development,InternalTest,ReleaseCandidate,StoreRelease}
    [CreateAssetMenu(fileName="BuildEnvironment",menuName="Hero Defense/Build/Build Environment")]
    public sealed class BuildEnvironmentData:ScriptableObject
    {
        [SerializeField]private HeroDefenseBuildType buildType=HeroDefenseBuildType.Development;[SerializeField]private string bundleIdentifier="com.independent.herodefense",productName="Hero Defense",version="1.0.0";[SerializeField]private int buildNumber=1;[SerializeField]private bool enableDebugMenu=true,enableVerboseLogging=true,enableCheats=true;
        public HeroDefenseBuildType BuildType=>buildType;public string BundleIdentifier=>bundleIdentifier;public string ProductName=>productName;public string Version=>version;public int BuildNumber=>buildNumber;public bool EnableDebugMenu=>enableDebugMenu&&buildType==HeroDefenseBuildType.Development;public bool EnableVerboseLogging=>enableVerboseLogging&&buildType<=HeroDefenseBuildType.InternalTest;public bool EnableCheats=>enableCheats&&buildType==HeroDefenseBuildType.Development;
        public void Configure(HeroDefenseBuildType type,string identifier,string product,string value,int number){buildType=type;bundleIdentifier=identifier;productName=product;version=value;buildNumber=Mathf.Max(1,number);enableDebugMenu=type==HeroDefenseBuildType.Development;enableVerboseLogging=type<=HeroDefenseBuildType.InternalTest;enableCheats=type==HeroDefenseBuildType.Development;}
        public bool Validate(out string error){if(string.IsNullOrWhiteSpace(bundleIdentifier)||bundleIdentifier.Split('.').Length<3){error="Bundle Identifier is invalid.";return false;}if(string.IsNullOrWhiteSpace(productName)){error="Product Name is empty.";return false;}if(!GameVersionService.IsValidVersion(version)){error="Version must use Major.Minor.Patch.";return false;}if(buildNumber<=0){error="Build Number must be positive.";return false;}error="";return true;}
    }
    public static class BuildEnvironmentService
    {private static BuildEnvironmentData cached;public static BuildEnvironmentData Current=>cached??=Resources.Load<BuildEnvironmentData>("Build/BuildEnvironment");public static HeroDefenseBuildType BuildType=>Current!=null?Current.BuildType:HeroDefenseBuildType.Development;public static bool DebugUiEnabled=>Debug.isDebugBuild&&(Current==null||Current.EnableDebugMenu);public static bool CheatsEnabled=>Debug.isDebugBuild&&(Current==null||Current.EnableCheats);public static void ClearCache()=>cached=null;}
    public static class GameVersionService
    {public static string Version=>BuildEnvironmentService.Current!=null?BuildEnvironmentService.Current.Version:Application.version;public static int BuildNumber=>BuildEnvironmentService.Current!=null?BuildEnvironmentService.Current.BuildNumber:1;public static string DisplayVersion=>$"v{Version} (Build {BuildNumber})";public static bool IsValidVersion(string value){if(string.IsNullOrWhiteSpace(value))return false;string[] parts=value.Split('.');if(parts.Length!=3)return false;for(int i=0;i<parts.Length;i++)if(!int.TryParse(parts[i],out int number)||number<0)return false;return true;}public static string ReleaseArtifactName(string platform)=>$"HeroDefense-{platform}-{Version}-{BuildNumber}";}
}
