using R3;
using UnityEngine;
using UnityEngine.UI;

// 일시정지 창 뷰. 버튼 클릭을 알리기만 하고, 무엇을 할지는 UIManager가 정한다.
public class PausePanel : MonoBehaviour, IPauseView
{
    [SerializeField] private Button _leaveButton;
    [SerializeField] private Button _resumeButton;

    public Observable<Unit> OnLeave => _leaveButton.OnClickAsObservable();
    public Observable<Unit> OnResume => _resumeButton.OnClickAsObservable();

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
