using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private TreeSpawner _treeSpawner;
    [SerializeField] private MapBoundary _mapBoundary;
    [SerializeField] private GameCamera _gameCamera;
    [SerializeField] private TreeData _startTree;

    [Header("세션")]
    [SerializeField] private SessionTimer _sessionTimer;
    [SerializeField] private Wallet _wallet;
    [SerializeField] private SessionHud _sessionHud;
    [SerializeField] private PausePanel _pausePanel;
    [SerializeField] private SessionEndPanel _sessionEndPanel;

    [Header("입력")]
    [Tooltip("일시정지 창을 열고 닫는 키. 플레이어 입력이 꺼진 일시정지 중에도 받아야 하므로 PlayerInput과 따로 둔다.")]
    [SerializeField] private InputAction _pauseAction = new("Pause", InputActionType.Button, "<Keyboard>/escape");

    private GameObject _player;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private int _woodAtSessionStart;
    private bool _isSessionActive;
    private bool _isPaused;

    private void Start()
    {
        InitializeGame();
        StartSession();
    }

    private void OnEnable()
    {
        _pauseAction.performed += HandlePausePressed;
        _pauseAction.Enable();
    }

    private void OnDisable()
    {
        _pauseAction.performed -= HandlePausePressed;
        _pauseAction.Disable();
    }

    private void OnDestroy()
    {
        // 일시정지 중에 씬이 내려가도 시간이 멈춘 채로 남지 않게 한다.
        Time.timeScale = 1f;
    }

    private void InitializeGame()
    {
        _mapBoundary.Build();
        _gameCamera.SetBounds(_mapBoundary.CameraRect);

        _player = _playerSpawner.SpawnPlayer();
        _playerInput = _player.GetComponent<PlayerInput>();
        _playerMovement = _player.GetComponent<PlayerMovement>();

        _treeSpawner.OnTreeFelled.Subscribe(HandleTreeFelled).AddTo(this);
        _sessionTimer.OnEnded.Subscribe(_ => EndSession(SessionEndReason.TimeUp)).AddTo(this);

        _sessionHud.OnPauseClicked.Subscribe(_ => Pause()).AddTo(this);
        _pausePanel.OnResume.Subscribe(_ => Resume()).AddTo(this);
        _pausePanel.OnLeave.Subscribe(_ => EndSession(SessionEndReason.Left)).AddTo(this);

        _sessionEndPanel.OnContinue.Subscribe(_ => StartSession()).AddTo(this);
    }

    // 숲에 들어갈 때마다 나무를 새로 배치하고 시간을 처음부터 잰다.
    private void StartSession()
    {
        _sessionEndPanel.Hide();
        _pausePanel.Hide();

        _playerSpawner.MoveToSpawnPoint(_player);
        _gameCamera.SetTarget(_player.transform);
        _treeSpawner.Spawn(_startTree);

        _woodAtSessionStart = _wallet.Wood.CurrentValue;
        _isSessionActive = true;
        _playerInput.ActivateInput();
        _sessionTimer.Begin();
    }

    // 시간이 다 되거나, 숲을 다 베거나, 일시정지에서 나가기를 누르면 끝난다.
    private void EndSession(SessionEndReason reason)
    {
        if (!_isSessionActive)
        {
            return;
        }

        _isSessionActive = false;
        SetPaused(false);
        _pausePanel.Hide();

        _sessionTimer.Stop();
        _playerInput.DeactivateInput();
        _playerMovement.Stop();

        _sessionEndPanel.Show(reason, _wallet.Wood.CurrentValue - _woodAtSessionStart);
    }

    private void Pause()
    {
        if (!_isSessionActive || _isPaused)
        {
            return;
        }

        SetPaused(true);
        _playerInput.DeactivateInput();
        _playerMovement.Stop();
        _pausePanel.Show();
    }

    private void Resume()
    {
        if (!_isSessionActive || !_isPaused)
        {
            return;
        }

        _pausePanel.Hide();
        SetPaused(false);
        _playerInput.ActivateInput();
    }

    // ESC는 열려 있으면 닫고, 닫혀 있으면 연다.
    private void HandlePausePressed(InputAction.CallbackContext context)
    {
        if (_isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    // 타이머, 공격 딜레이, 나무 흔들림이 모두 게임 시간을 쓰므로 timeScale 하나로 함께 멈춘다.
    private void SetPaused(bool paused)
    {
        _isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    private void HandleTreeFelled(Trees tree)
    {
        // 세션이 끝난 뒤에 마저 쓰러진 나무는 종료 패널의 획득량과 맞지 않으므로 세지 않는다.
        if (!_isSessionActive)
        {
            return;
        }

        _wallet.AddWood(tree.Data.Reward);
        _sessionTimer.AddTreeBonus();

        if (_treeSpawner.RemainingCount == 0)
        {
            EndSession(SessionEndReason.Cleared);
        }
    }
}
