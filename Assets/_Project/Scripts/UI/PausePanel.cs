using R3;
using UnityEngine;
using UnityEngine.UI;

// 일시정지 버튼을 누르면 뜨는 패널. 숲을 나가 강화하러 갈지, 계속 벨지 묻는다.
public class PausePanel : MonoBehaviour
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
