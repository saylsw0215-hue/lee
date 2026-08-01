using HeroDefense.Save;
using UnityEngine;

namespace HeroDefense.Audio
{
    public enum AudioChannel{Master,Music,Sfx,Ui} public interface IHapticService{void Pulse();}
    public sealed class NullHapticService:IHapticService{public void Pulse(){}}
    public sealed class MobileHapticService:IHapticService{private float lastPulse=-10;public void Pulse(){if(SaveGameManager.Instance?.Data.settings.vibration!=true||Time.unscaledTime-lastPulse<.25f)return;lastPulse=Time.unscaledTime;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }}
    public static class Haptics{public static IHapticService Current{get;}=
#if UNITY_IOS || UNITY_ANDROID
        new MobileHapticService();
#else
        new NullHapticService();
#endif
    }
    public static class LocalizationService{public static string Get(string korean,string english)=>SaveGameManager.Instance?.Data.settings.language==LanguageOption.English?english:korean;}
}
