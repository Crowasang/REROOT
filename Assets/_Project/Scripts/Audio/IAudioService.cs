// 소리를 내는 창구. 무엇을 언제 틀지는 AudioManager가 정하고, 이 인터페이스는 어떻게 틀지만 맡는다.
public interface IAudioService
{
    void PlaySfx(SoundId id);

    // 이미 같은 곡이 나오고 있으면 처음부터 다시 틀지 않는다.
    void PlayMusic(MusicId id);

    void StopMusic();

    // 일시정지 창처럼 잠깐 음악을 낮출 때 쓴다.
    void SetMusicDucked(bool isDucked);

    // 0~1. 나중에 설정 화면에서 쓴다.
    void SetSfxVolume(float volume);

    void SetMusicVolume(float volume);
}
