using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 화면 표시만 맡는 뷰. 어떤 시스템에서 값이 오는지는 모르고, UIManager가 값을 넣어 준다.
public class SessionHud : MonoBehaviour, IHudView
{
    [SerializeField] private Image _timeFill;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _woodText;
    [SerializeField] private Button _pauseButton;

    public Observable<Unit> OnPauseClicked => _pauseButton.OnClickAsObservable();

    public void SetTime(float _ratio, int _seconds)
    {
        _timeFill.fillAmount = Mathf.Clamp01(_ratio);
        _timeText.text = $"{_seconds}s";
    }

    public void SetWood(int _wood)
    {
        _woodText.text = $"Wood {_wood}";
    }
}
