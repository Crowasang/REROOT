using System;
using System.Collections.Generic;
using UnityEngine;

// 소리 종류(SoundId, MusicId)와 실제 소리 파일·볼륨을 잇는 표. 소리를 바꿀 때는 이 에셋만 고친다.
[CreateAssetMenu(fileName = "SoundLibrary", menuName = "REROOT/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class SfxEntry
    {
        [SerializeField] private SoundId _id;
        [Tooltip("여러 개를 넣으면 틀 때마다 무작위로 하나를 고른다. 반복되는 소리가 덜 단조롭다.")]
        [SerializeField] private AudioClip[] _clips;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [Tooltip("틀 때마다 음높이를 이 범위 안에서 무작위로 흔든다. 0이면 그대로.")]
        [SerializeField, Range(0f, 0.3f)] private float _pitchVariation = 0.05f;
        [Tooltip("같은 소리가 이 시간(초) 안에 또 오면 무시한다. 여러 나무를 한 번에 칠 때 소리가 겹쳐 커지는 것을 막는다.")]
        [SerializeField] private float _minInterval = 0.03f;

        public SoundId Id => _id;
        public IReadOnlyList<AudioClip> Clips => _clips;
        public float Volume => _volume;
        public float PitchVariation => _pitchVariation;
        public float MinInterval => _minInterval;
    }

    [Serializable]
    public class MusicEntry
    {
        [SerializeField] private MusicId _id;
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 0.5f;

        public MusicId Id => _id;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
    }

    [SerializeField] private SfxEntry[] _sfx;
    [SerializeField] private MusicEntry[] _music;

    public bool TryGetSfx(SoundId id, out SfxEntry entry)
    {
        foreach (SfxEntry candidate in _sfx)
        {
            if (candidate.Id == id)
            {
                entry = candidate;
                return true;
            }
        }

        entry = null;
        return false;
    }

    public bool TryGetMusic(MusicId id, out MusicEntry entry)
    {
        foreach (MusicEntry candidate in _music)
        {
            if (candidate.Id == id)
            {
                entry = candidate;
                return true;
            }
        }

        entry = null;
        return false;
    }
}
