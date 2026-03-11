using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("SFX Clips")]
    [SerializeField] private SFXClip[] sfxClips;

    private const string KEY_SFX_VOL = "audio_sfx_volume";
    private const string KEY_MUSIC_VOL = "audio_music_volume";
    private const string KEY_MUTED = "audio_muted";

    public bool IsMuted { get; private set; }

    [Serializable]
    public class SFXClip
    {
        public string name;
        public AudioClip clip;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPreferences();
    }

    // ───────────────────────── Play SFX ─────────────────────────

    public void Play(string clipName)
    {
        if (IsMuted) return;
        if (sfxSource == null || sfxClips == null) return;

        foreach (var entry in sfxClips)
        {
            if (entry.clip != null && string.Equals(entry.name, clipName, StringComparison.OrdinalIgnoreCase))
            {
                sfxSource.PlayOneShot(entry.clip);
                return;
            }
        }

        Debug.LogWarning($"[AudioManager] SFX clip not found: {clipName}");
    }

    // ───────────────────────── Music ─────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        if (!IsMuted) musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    // ───────────────────────── Volume ─────────────────────────

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = volume;
        PlayerPrefs.SetFloat(KEY_SFX_VOL, volume);
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = volume;
        PlayerPrefs.SetFloat(KEY_MUSIC_VOL, volume);
    }

    public float GetSFXVolume()
    {
        return sfxSource != null ? sfxSource.volume : 1f;
    }

    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : 1f;
    }

    // ───────────────────────── Mute ─────────────────────────

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        ApplyMute();
        PlayerPrefs.SetInt(KEY_MUTED, IsMuted ? 1 : 0);
    }

    private void ApplyMute()
    {
        if (sfxSource != null) sfxSource.mute = IsMuted;
        if (musicSource != null) musicSource.mute = IsMuted;
    }

    // ───────────────────────── Persistence ─────────────────────────

    private void LoadPreferences()
    {
        float sfxVol = PlayerPrefs.GetFloat(KEY_SFX_VOL, 1f);
        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 0.5f);
        IsMuted = PlayerPrefs.GetInt(KEY_MUTED, 0) == 1;

        if (sfxSource != null) sfxSource.volume = sfxVol;
        if (musicSource != null) musicSource.volume = musicVol;
        ApplyMute();
    }
}
