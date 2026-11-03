using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 세션이 끝나면 뜨는 패널. 강화 패널이 생기면 이 자리를 대신하거나 여기서 넘어간다.
public class SessionEndPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _woodGainedText;
    [SerializeField] private Button _continueButton;

    public Observable<Unit> OnContinue => _continueButton.OnClickAsObservable();

    public void Show(int woodGained)
    {
        _woodGainedText.text = $"Wood +{woodGained}";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
