using System.Collections.Generic;
using UnityEngine;
using VContainer;

// 실제로 소리를 내는 씬 오브젝트. 배경음 1채널과 효과음 여러 채널을 돌려 쓴다.
// 언제 무엇을 틀지는 모르고, IAudioService로 요청받은 대로만 튼다.
public class AudioPlayer : MonoBehaviour, IAudioService
{
    private const float DUCKED_MUSIC_RATIO = 0.35f;

    [Tooltip("동시에 낼 수 있는 효과음 수. 모자라면 가장 오래된 채널부터 끊고 쓴다.")]
    [SerializeField, Min(1)] private int _sfxVoices = 12;

    private readonly Dictionary<SoundId, float> _lastPlayTime = new();
    private SoundLibrary _library;
    private AudioSource _musicSource;
    private AudioSource[] _sfxSources;
    private int _nextSfxIndex;
    private MusicId? _currentMusic;
    private float _currentMusicBaseVolume = 1f;
    private float _sfxVolume = 1f;
    private float _musicVolume = 1f;
    private bool _isMusicDucked;

    private void Awake()
    {
        _musicSource = CreateSource("Music");
        _musicSource.loop = true;

        _sfxSources = new AudioSource[_sfxVoices];
        for (int i = 0; i < _sfxVoices; i++)
        {
            _sfxSources[i] = CreateSource($"Sfx_{i:00}");
        }
    }

    [Inject]
    public void Construct(SoundLibrary library)
    {
        _library = library;
    }

    public void PlaySfx(SoundId id)
    {
        if (_library == null || !_library.TryGetSfx(id, out SoundLibrary.SfxEntry entry) || entry.Clips.Count == 0)
        {
            return;
        }

        // 일시정지 중에도 UI 소리는 나야 하므로 게임 시간이 아닌 실제 시간으로 잰다.
        float now = Time.unscaledTime;
        if (_lastPlayTime.TryGetValue(id, out float last) && now - last < entry.MinInterval)
        {
            return;
        }

        _lastPlayTime[id] = now;

        AudioSource source = _sfxSources[_nextSfxIndex];
        _nextSfxIndex = (_nextSfxIndex + 1) % _sfxSources.Length;

        source.pitch = 1f + Random.Range(-entry.PitchVariation, entry.PitchVariation);
        source.PlayOneShot(entry.Clips[Random.Range(0, entry.Clips.Count)], entry.Volume * _sfxVolume);
    }

    public void PlayMusic(MusicId id)
    {
        if (_currentMusic == id && _musicSource.isPlaying)
        {
            return;
        }

        if (_library == null || !_library.TryGetMusic(id, out SoundLibrary.MusicEntry entry) || entry.Clip == null)
        {
            return;
        }

        _currentMusic = id;
        _currentMusicBaseVolume = entry.Volume;
        _musicSource.clip = entry.Clip;
        ApplyMusicVolume();
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _currentMusic = null;
        _musicSource.Stop();
    }

    public void SetMusicDucked(bool isDucked)
    {
        _isMusicDucked = isDucked;
        ApplyMusicVolume();
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
    }

    private AudioSource CreateSource(string sourceName)
    {
        var child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);

        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private void ApplyMusicVolume()
    {
        float duck = _isMusicDucked ? DUCKED_MUSIC_RATIO : 1f;
        _musicSource.volume = _currentMusicBaseVolume * _musicVolume * duck;
    }
}
