using System.Collections;
using System.Collections.Generic;
using HeroDefense.Core;
using HeroDefense.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeroDefense.Audio
{
    [CreateAssetMenu(fileName="AudioCue",menuName="Hero Defense/Audio/Audio Cue")]
    public sealed class AudioCueData:ScriptableObject{[SerializeField]private string cueId;[SerializeField]private AudioClip clip;[SerializeField]private AudioChannel channel=AudioChannel.Sfx;[SerializeField,Range(0,1)]private float volume=1;[SerializeField,Min(1)]private int concurrency=4;public string CueId=>cueId;public AudioClip Clip=>clip;public AudioChannel Channel=>channel;public float Volume=>volume;public int Concurrency=>concurrency;}
    /// <summary>Persistent music crossfade and bounded SFX source pool; empty cues safely remain silent.</summary>
    public sealed class AudioManager:MonoBehaviour
    {
        public static AudioManager Instance{get;private set;}private AudioSource musicA,musicB;private readonly List<AudioSource> pool=new(12);private readonly Dictionary<string,int> playing=new();private bool useA=true;
        private void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);musicA=Create("MusicA",true);musicB=Create("MusicB",true);for(int i=0;i<12;i++)pool.Add(Create("Sfx_"+i,false));SceneManager.sceneLoaded+=OnSceneLoaded;ApplySettings();}
        private AudioSource Create(string value,bool loop){var source=new GameObject(value,typeof(AudioSource)).GetComponent<AudioSource>();source.transform.SetParent(transform,false);source.playOnAwake=false;source.loop=loop;return source;}
        public void ApplySettings(){var settings=SaveGameManager.Instance?.Data.settings;if(settings==null)return;AudioListener.volume=settings.masterVolume;musicA.volume=musicB.volume=settings.musicVolume;}
        public void PlayMusic(AudioClip clip,float fade=.5f){if(clip==null)return;AudioSource current=useA?musicA:musicB,next=useA?musicB:musicA;if(current.clip==clip&&current.isPlaying)return;useA=!useA;StopCoroutine(nameof(FadeMusic));StartCoroutine(FadeMusic(current,next,clip,fade));}
        private IEnumerator FadeMusic(AudioSource from,AudioSource to,AudioClip clip,float duration){to.clip=clip;to.volume=0;to.Play();float target=SaveGameManager.Instance?.Data.settings.musicVolume??.8f;float elapsed=0;while(elapsed<duration){elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/Mathf.Max(.01f,duration));from.volume=target*(1-t);to.volume=target*t;yield return null;}from.Stop();from.clip=null;to.volume=target;}
        public bool Play(AudioCueData cue){if(cue==null||cue.Clip==null)return false;if(playing.TryGetValue(cue.CueId,out int count)&&count>=cue.Concurrency)return false;AudioSource source=null;for(int i=0;i<pool.Count;i++)if(!pool[i].isPlaying){source=pool[i];break;}if(source==null)return false;source.clip=cue.Clip;source.volume=cue.Volume*ChannelVolume(cue.Channel);source.Play();playing[cue.CueId]=count+1;StartCoroutine(Release(cue.CueId,source,cue.Clip.length));return true;}
        private IEnumerator Release(string id,AudioSource source,float duration){yield return new WaitForSecondsRealtime(duration);source.Stop();source.clip=null;if(playing.TryGetValue(id,out int count)){if(count<=1)playing.Remove(id);else playing[id]=count-1;}}
        private float ChannelVolume(AudioChannel channel){var s=SaveGameManager.Instance?.Data.settings;if(s==null)return 1;return channel==AudioChannel.Music?s.musicVolume:channel==AudioChannel.Master?s.masterVolume:s.sfxVolume;}
        private void OnSceneLoaded(Scene scene,LoadSceneMode mode){ApplySettings();}
        private void OnDestroy(){SceneManager.sceneLoaded-=OnSceneLoaded;if(Instance==this)Instance=null;}
    }
}
