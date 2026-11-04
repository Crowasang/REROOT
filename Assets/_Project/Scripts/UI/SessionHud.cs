using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionHud : MonoBehaviour
{
    [SerializeField] private SessionTimer _sessionTimer;
    [SerializeField] private Wallet _wallet;

    [SerializeField] private Image _timeFill;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _woodText;
    [SerializeField] private Button _pauseButton;

    public Observable<Unit> OnPauseClicked => _pauseButton.OnClickAsObservable();

    private void Start()
    {
        // 바는 매 프레임, 글자는 초가 바뀔 때만 갱신한다.
        _sessionTimer.Remaining
            .Subscribe(remaining => _timeFill.fillAmount = Mathf.Clamp01(remaining / _sessionTimer.BaseDuration))
            .AddTo(this);

        _sessionTimer.Remaining
            .Select(Mathf.CeilToInt)
            .DistinctUntilChanged()
            .Subscribe(seconds => _timeText.text = $"{seconds}s")
            .AddTo(this);

        _wallet.Wood
            .Subscribe(wood => _woodText.text = $"Wood {wood}")
            .AddTo(this);
    }
}
