using System.Collections;
using System.Collections.Generic;
using HeroDefense.Core;
using HeroDefense.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroDefense.Audio
{
    public enum GameAudioEvent{MainMenuMusic,BattleMusic,BossMusic,ButtonClick,BuildingPlaced,BuildingUpgraded,BuildingSold,AllyProduced,BasicAttack,AttackHit,CriticalHit,HeroSkill,HeroUltimate,BossAppeared,BaseDamaged,Victory,Defeat}
    [CreateAssetMenu(fileName="AudioCue",menuName="Hero Defense/Audio/Audio Cue")]
    public sealed class AudioCueData:ScriptableObject{[SerializeField]private string cueId;[SerializeField]private AudioClip clip;[SerializeField]private AudioChannel channel=AudioChannel.Sfx;[SerializeField,Range(0,1)]private float volume=1;[SerializeField,Min(1)]private int concurrency=4;public string CueId=>cueId;public AudioClip Clip=>clip;public AudioChannel Channel=>channel;public float Volume=>volume;public int Concurrency=>concurrency;}
    /// <summary>Persistent music crossfade and bounded SFX source pool; empty cues safely remain silent.</summary>
    public sealed class AudioManager:MonoBehaviour
    {
        private readonly struct RuntimeCue{public readonly AudioClip Clip;public readonly AudioChannel Channel;public readonly float Volume;public readonly int Concurrency;public RuntimeCue(AudioClip clip,AudioChannel channel,float volume,int concurrency){Clip=clip;Channel=channel;Volume=Mathf.Clamp01(volume);Concurrency=Mathf.Max(1,concurrency);}}
        public static AudioManager Instance{get;private set;}private AudioSource musicA,musicB;private readonly List<AudioSource> pool=new(12);private readonly Dictionary<AudioSource,(AudioChannel channel,float volume)> sourceSettings=new();private readonly Dictionary<string,int> playing=new();private readonly Dictionary<GameAudioEvent,RuntimeCue> runtimeCues=new();private readonly Dictionary<GameAudioEvent,float> lastPlayed=new();private bool useA=true,userGestureReceived;private GameAudioEvent? pendingMusic;
        public AudioClip CurrentMusicClip=>(useA?musicA:musicB)?.clip;
        private void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);musicA=Create("MusicA",true);musicB=Create("MusicB",true);for(int i=0;i<12;i++)pool.Add(Create("Sfx_"+i,false));SceneManager.sceneLoaded+=OnSceneLoaded;ApplySettings();}
        private AudioSource Create(string value,bool loop){var source=new GameObject(value,typeof(AudioSource)).GetComponent<AudioSource>();source.transform.SetParent(transform,false);source.playOnAwake=false;source.loop=loop;return source;}
        public void ApplySettings(){var settings=SaveGameManager.Instance?.Data.settings;if(settings==null)return;AudioListener.volume=settings.masterVolume;musicA.volume=musicB.volume=settings.musicVolume;for(int i=0;i<pool.Count;i++)if(sourceSettings.TryGetValue(pool[i],out var value))pool[i].volume=value.volume*ChannelVolume(value.channel);}
        public void NotifyUserGesture(){if(userGestureReceived)return;userGestureReceived=true;if(pendingMusic.HasValue){GameAudioEvent value=pendingMusic.Value;pendingMusic=null;PlayEvent(value);}}
        public bool PlayEvent(GameAudioEvent value)
        {
            RuntimeCue cue;if(!runtimeCues.TryGetValue(value,out cue)){AudioCueData asset=Resources.Load<AudioCueData>("Audio/"+value);if(asset==null||asset.Clip==null){if(IsMusic(value))pendingMusic=value;return false;}cue=new RuntimeCue(asset.Clip,asset.Channel,asset.Volume,asset.Concurrency);}
            if(IsMusic(value)){if(!userGestureReceived){pendingMusic=value;return false;}PlayMusic(cue.Clip);return true;}
            if(!userGestureReceived)NotifyUserGesture();float now=Time.unscaledTime;if(lastPlayed.TryGetValue(value,out float previous)&&now-previous<MinimumInterval(value))return false;lastPlayed[value]=now;return Play(cue.Clip,value.ToString(),cue.Channel,cue.Volume,cue.Concurrency);
        }
        public void ConfigureEventForTests(GameAudioEvent value,AudioClip clip,AudioChannel channel=AudioChannel.Sfx,float volume=1,int concurrency=4)=>runtimeCues[value]=new RuntimeCue(clip,channel,volume,concurrency);
        public void PlayMusic(AudioClip clip,float fade=.5f){if(clip==null)return;AudioSource current=useA?musicA:musicB,next=useA?musicB:musicA;if(current.clip==clip)return;useA=!useA;StopCoroutine(nameof(FadeMusic));StartCoroutine(FadeMusic(current,next,clip,fade));}
        public void StopMusic(){StopCoroutine(nameof(FadeMusic));musicA.Stop();musicB.Stop();musicA.clip=musicB.clip=null;pendingMusic=null;}
        private IEnumerator FadeMusic(AudioSource from,AudioSource to,AudioClip clip,float duration){to.clip=clip;to.volume=0;to.Play();float target=SaveGameManager.Instance?.Data.settings.musicVolume??.8f;float elapsed=0;while(elapsed<duration){elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/Mathf.Max(.01f,duration));from.volume=target*(1-t);to.volume=target*t;yield return null;}from.Stop();from.clip=null;to.volume=target;}
        public bool Play(AudioCueData cue)=>cue!=null&&Play(cue.Clip,cue.CueId,cue.Channel,cue.Volume,cue.Concurrency);
        private bool Play(AudioClip clip,string id,AudioChannel channel,float volume,int concurrency){if(clip==null)return false;if(playing.TryGetValue(id,out int count)&&count>=concurrency)return false;AudioSource source=null;for(int i=0;i<pool.Count;i++)if(pool[i].clip==null||!pool[i].isPlaying){source=pool[i];break;}if(source==null)return false;source.clip=clip;sourceSettings[source]=(channel,volume);source.volume=volume*ChannelVolume(channel);source.Play();playing[id]=count+1;StartCoroutine(Release(id,source,clip.length));return true;}
        private IEnumerator Release(string id,AudioSource source,float duration){yield return new WaitForSecondsRealtime(duration);source.Stop();source.clip=null;if(playing.TryGetValue(id,out int count)){if(count<=1)playing.Remove(id);else playing[id]=count-1;}}
        private float ChannelVolume(AudioChannel channel){var s=SaveGameManager.Instance?.Data.settings;if(s==null)return 1;return channel==AudioChannel.Music?s.musicVolume:channel==AudioChannel.Master?s.masterVolume:s.sfxVolume;}
        private static bool IsMusic(GameAudioEvent value)=>value==GameAudioEvent.MainMenuMusic||value==GameAudioEvent.BattleMusic||value==GameAudioEvent.BossMusic;
        private static float MinimumInterval(GameAudioEvent value)=>(value==GameAudioEvent.AttackHit||value==GameAudioEvent.BasicAttack)?.06f:value==GameAudioEvent.CriticalHit?.1f:0f;
        private void OnSceneLoaded(Scene scene,LoadSceneMode mode){ApplySettings();if(scene.name==SceneNames.MainMenu)PlayEvent(GameAudioEvent.MainMenuMusic);else if(scene.name==SceneNames.Battle)PlayEvent(GameAudioEvent.BattleMusic);}
        private void OnApplicationPause(bool paused){AudioListener.pause=paused;if(!paused)ApplySettings();}
        private void OnApplicationFocus(bool focused){if(!focused)AudioListener.pause=true;else{AudioListener.pause=false;ApplySettings();}}
        private void OnDestroy(){SceneManager.sceneLoaded-=OnSceneLoaded;if(Instance==this)Instance=null;}
    }
}
