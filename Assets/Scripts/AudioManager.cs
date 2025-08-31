using UnityEngine;
using Hellmade.Sound;

public class AudioManager : MonoBehaviour
{
    public AudioClipStorage[] audioClips;

    // Make it static so it can be accessed from anywhere
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Don't destroy this object when loading new scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        PlayMusic(0, 0.6f, true); // Play the ambient music track at start
    }

    public void PlayMusic(int index, float volume = 1.0f, bool loop = true)
    {
        if (index < 0 || index >= audioClips.Length) return;

        AudioClip clip = audioClips[index].audioClip;
        if (clip == null) return;

        int audioID = EazySoundManager.PlayMusic(clip, volume, loop, false);
        audioClips[index].audio = EazySoundManager.GetAudio(audioID);
    }

    public void PlaySound(int index, float volume = 1.0f)
    {
        if (index < 0 || index >= audioClips.Length) return;
        
        AudioClip clip = audioClips[index].audioClip;
        if (clip == null) return;

        int audioID = EazySoundManager.PlaySound(clip, volume);
        audioClips[index].audio = EazySoundManager.GetAudio(audioID);
    }

    public void PlaySfxLoop(int index, float volume = 1.0f)
    {
        if (index < 0 || index >= audioClips.Length) return;
        
        AudioClip clip = audioClips[index].audioClip;
        if (clip == null) return;

        // Use PlaySound with volume parameter and then manually set it to loop
        int audioID = EazySoundManager.PlaySound(clip, volume, true, null); // true for loop, null for no 3D transform
        audioClips[index].audio = EazySoundManager.GetAudio(audioID);
    }

    public void PlaySfxLoop3D(int index, float volume, Transform sourceTransform)
    {
        if (index < 0 || index >= audioClips.Length) return;
        
        AudioClip clip = audioClips[index].audioClip;
        if (clip == null) return;

        // Use PlaySound with 3D transform for spatial audio
        int audioID = EazySoundManager.PlaySound(clip, volume, true, sourceTransform); // true for loop, sourceTransform for 3D
        audioClips[index].audio = EazySoundManager.GetAudio(audioID);
    }

    public void PlaySound3D(int index, float volume, Transform sourceTransform)
    {
        if (index < 0 || index >= audioClips.Length) return;
        
        AudioClip clip = audioClips[index].audioClip;
        if (clip == null) return;

        // Use PlaySound with 3D transform for spatial audio (no loop)
        int audioID = EazySoundManager.PlaySound(clip, volume, false, sourceTransform); // false for no loop
        audioClips[index].audio = EazySoundManager.GetAudio(audioID);
    }

    public void PauseAudio(int index)
    {
        if (index < 0 || index >= audioClips.Length) return;
        if (audioClips[index].audio != null)
        {
            audioClips[index].audio.Pause();
        }
    }

    public void StopAudio(int index)
    {
        if (index < 0 || index >= audioClips.Length) return;
        if (audioClips[index].audio != null)
        {
            audioClips[index].audio.Stop();
        }
    }

    public void StopAllMusic()
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audio != null && audioClips[i].audio.Type == Hellmade.Sound.Audio.AudioType.Music)
            {
                audioClips[i].audio.Stop();
            }
        }
    }

    public void StopAllSounds()
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audio != null && audioClips[i].audio.Type == Hellmade.Sound.Audio.AudioType.Sound)
            {
                audioClips[i].audio.Stop();
            }
        }
    }

    public void StopAllAudio()
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audio != null)
            {
                audioClips[i].audio.Stop();
            }
        }
    }

    public bool IsAudioPlaying(int index)
    {
        if (index < 0 || index >= audioClips.Length) return false;
        if (audioClips[index].audio != null)
        {
            return audioClips[index].audio.IsPlaying;
        }
        return false;
    }

    public bool IsMusicPlaying(int index)
    {
        if (index < 0 || index >= audioClips.Length) return false;
        if (audioClips[index].audio != null && audioClips[index].audio.Type == Hellmade.Sound.Audio.AudioType.Music)
        {
            return audioClips[index].audio.IsPlaying;
        }
        return false;
    }

    public void KillAudioSfx(int index)
    {
        if (index < 0 || index >= audioClips.Length) return;
        if (audioClips[index].audio != null && audioClips[index].audio.Type == Hellmade.Sound.Audio.AudioType.Sound)
        {
            audioClips[index].audio.Stop();
            audioClips[index].audio = null; // Force clear the reference
        }
    }

    public void KillAllAudioSfx()
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audio != null && audioClips[i].audio.Type == Hellmade.Sound.Audio.AudioType.Sound)
            {
                audioClips[i].audio.Stop();
                audioClips[i].audio = null; // Force clear the reference
            }
        }
    }

    public void ForceKillAudio(int index)
    {
        if (index < 0 || index >= audioClips.Length) return;
        if (audioClips[index].audio != null)
        {
            audioClips[index].audio.Stop();
            audioClips[index].audio = null; // Force clear the reference
        }
    }
}

[System.Serializable]
public struct AudioClipStorage
{
    public AudioClip audioClip;
    public Audio audio;
}