using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 세션 결과 창 뷰
public class SessionEndPanel : MonoBehaviour, ISessionEndView
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _woodGainedText;
    [SerializeField] private Button _continueButton;

    public Observable<Unit> OnContinue => _continueButton.OnClickAsObservable();

    public void Show(SessionResult result)
    {
        _titleText.text = GetTitle(result.Reason);
        _woodGainedText.text = $"Wood +{result.WoodGained}";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private static string GetTitle(SessionEndReason reason)
    {
        return reason switch
        {
            SessionEndReason.Cleared => "Forest Cleared!",
            SessionEndReason.Left => "Left the Forest",
            _ => "Time's Up",
        };
    }
}
