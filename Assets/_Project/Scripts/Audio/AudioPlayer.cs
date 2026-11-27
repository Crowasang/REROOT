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
        for (int _i = 0; _i < _sfxVoices; _i++)
        {
            _sfxSources[_i] = CreateSource($"Sfx_{_i:00}");
        }
    }

    [Inject]
    public void Construct(SoundLibrary _library)
    {
        this._library = _library;
    }

    public void PlaySfx(SoundId _id)
    {
        if (_library == null || !_library.TryGetSfx(_id, out SoundLibrary.SfxEntry _entry) || _entry.Clips.Count == 0)
        {
            return;
        }

        // 일시정지 중에도 UI 소리는 나야 하므로 게임 시간이 아닌 실제 시간으로 잰다.
        float _now = Time.unscaledTime;
        if (_lastPlayTime.TryGetValue(_id, out float _last) && _now - _last < _entry.MinInterval)
        {
            return;
        }

        _lastPlayTime[_id] = _now;

        AudioSource _source = _sfxSources[_nextSfxIndex];
        _nextSfxIndex = (_nextSfxIndex + 1) % _sfxSources.Length;

        _source.pitch = 1f + Random.Range(-_entry.PitchVariation, _entry.PitchVariation);
        _source.PlayOneShot(_entry.Clips[Random.Range(0, _entry.Clips.Count)], _entry.Volume * _sfxVolume);
    }

    public void PlayMusic(MusicId _id)
    {
        if (_currentMusic == _id && _musicSource.isPlaying)
        {
            return;
        }

        if (_library == null || !_library.TryGetMusic(_id, out SoundLibrary.MusicEntry _entry) || _entry.Clip == null)
        {
            return;
        }

        _currentMusic = _id;
        _currentMusicBaseVolume = _entry.Volume;
        _musicSource.clip = _entry.Clip;
        ApplyMusicVolume();
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _currentMusic = null;
        _musicSource.Stop();
    }

    public void SetMusicDucked(bool _isDucked)
    {
        _isMusicDucked = _isDucked;
        ApplyMusicVolume();
    }

    public void SetSfxVolume(float _volume)
    {
        _sfxVolume = Mathf.Clamp01(_volume);
    }

    public void SetMusicVolume(float _volume)
    {
        _musicVolume = Mathf.Clamp01(_volume);
        ApplyMusicVolume();
    }

    private AudioSource CreateSource(string _sourceName)
    {
        var _child = new GameObject(_sourceName);
        _child.transform.SetParent(transform, false);

        AudioSource _source = _child.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;
        return _source;
    }

    private void ApplyMusicVolume()
    {
        float _duck = _isMusicDucked ? DUCKED_MUSIC_RATIO : 1f;
        _musicSource.volume = _currentMusicBaseVolume * _musicVolume * _duck;
    }
}
