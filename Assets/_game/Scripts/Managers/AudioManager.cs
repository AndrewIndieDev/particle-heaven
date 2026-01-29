using AndrewDowsett.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public enum EMixerGroup
{
    Master,
    Music,
    SFX,

    COUNT
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager Instance;
    private void Awake() => Instance = this;

    [Header("Mixer Groups")]
    public AudioMixer mixer;
    public AudioMixerGroup masterGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    private AudioSource currentMusic;

    public static AudioSource PlaySound(AudioClip clip, EMixerGroup mixerGroup, float pitch = 1.0f, float volume = 1.0f, bool doLoop = false, float fadeTime = 0f)
    {
        if (Instance == null)
            return null;

        if (clip != null)
        {
            GameObject sound = new("Sound_" + clip);
            sound.transform.SetParent(Instance.transform);
            AudioSource source = sound.AddComponent<AudioSource>();
            source.clip = clip;
            source.pitch = pitch;
            source.volume = volume;
            source.loop = doLoop;
            source.outputAudioMixerGroup = Instance.GetMixerGroup(mixerGroup);
            source.Play();
            if (!doLoop)
            {
                sound.AddComponent<DestroyAudioWhenFinished>();
            }
            if (mixerGroup == EMixerGroup.Music)
            {
                if (Instance.currentMusic != null)
                    Instance.SwitchMusic(source, volume);
                else
                {
                    Instance.currentMusic = source;
                    _ = Instance.FadeAudioTo(source, fadeTime, volume);
                }
            }
            return source;
        }
        else
        {
            if (mixerGroup == EMixerGroup.Music)
            {
                if (Instance.currentMusic != null)
                    Instance.SwitchMusic(null);
            }
            return null;
        }
    }

    public AudioMixerGroup GetMixerGroup(EMixerGroup mixerGroup)
    {
        switch (mixerGroup)
        { 
            case EMixerGroup.Master:
                return masterGroup;
            case EMixerGroup.Music:
                return musicGroup;
            case EMixerGroup.SFX:
                return sfxGroup;
        }
        return null;
    }

    private async void SwitchMusic(AudioSource newMusic, float volume = 0.5f)
    {
        Debug.Log("fading out current music");
        await FadeAudioTo(currentMusic, 0.5f, 0f);
        Debug.Log("destroying current music");
        if (currentMusic != null && currentMusic.gameObject != null)
            Destroy(currentMusic.gameObject);
        currentMusic = newMusic;
        Debug.Log("fading in new music");
        await  FadeAudioTo(currentMusic, 0.5f, volume);
        Debug.Log("finshed");
    }

    public async UniTask FadeAudioTo(AudioSource source, float duration, float toVolume)
    {
        if (source != null)
        {
            float i = 0f;
            float fromVolume = source.volume;
            while (i < duration && source != null)
            {
                i += Time.deltaTime;
                source.volume = Mathf.Abs(i.Remap(0f, duration, fromVolume, toVolume));
                await UniTask.Yield();
            }
        }
    }

    public void SetVolume(EMixerGroup mixerGroup, float volumePercentage)
    {
        float volume = volumePercentage.Remap(0f, 1f, -40f, 0f);
        if (volumePercentage == 0f)
            volume = -80f;
        AudioMixerGroup group = GetMixerGroup(mixerGroup);
        group.audioMixer.SetFloat(mixerGroup.ToString(), volume);
    }

    public void MuteAll()
    {
        for (int i = 0; i < (int)EMixerGroup.COUNT; i++)
        {
            switch ((EMixerGroup)i)
            {
                case EMixerGroup.Master:
                    //SetVolume(EMixerGroup.Master, 0);
                    break;
                case EMixerGroup.Music:
                    //SetVolume(EMixerGroup.Music, 0);
                    break;
                case EMixerGroup.SFX:
                    //SetVolume(EMixerGroup.SFX, 0);
                    break;
            }
        }
    }

    public void UnMuteAll()
    {
        for (int i = 0; i < (int)EMixerGroup.COUNT; i++)
        {
            switch ((EMixerGroup)i)
            {
                case EMixerGroup.Master:
                    //SetVolume(EMixerGroup.Master, Settings.Instance.MasterVolume);
                    break;
                case EMixerGroup.Music:
                    //SetVolume(EMixerGroup.Music, Settings.Instance.MusicVolume);
                    break;
                case EMixerGroup.SFX:
                    //SetVolume(EMixerGroup.SFX, Settings.Instance.SfxVolume);
                    break;
            }
        }
    }
}