using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [FormerlySerializedAs("_playerSpwaner")]
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private TreeSpawner _treeSpawner;
    [SerializeField] private MapBoundary _mapBoundary;
    [SerializeField] private GameCamera _gameCamera;
    [SerializeField] private TreeData _startTree;

    [Header("세션")]
    [SerializeField] private SessionTimer _sessionTimer;
    [SerializeField] private Wallet _wallet;
    [SerializeField] private SessionEndPanel _sessionEndPanel;

    private GameObject _player;
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private int _woodAtSessionStart;

    private void Start()
    {
        InitializeGame();
        StartSession();
    }

    private void InitializeGame()
    {
        _mapBoundary.Build();
        _gameCamera.SetBounds(_mapBoundary.CameraRect);

        _player = _playerSpawner.PlayerSpawn();
        _playerInput = _player.GetComponent<PlayerInput>();
        _playerMovement = _player.GetComponent<PlayerMovement>();

        _treeSpawner.OnTreeFelled.Subscribe(HandleTreeFelled).AddTo(this);
        _sessionTimer.OnEnded.Subscribe(_ => EndSession()).AddTo(this);
        _sessionEndPanel.OnContinue.Subscribe(_ => StartSession()).AddTo(this);
    }

    // 숲에 들어갈 때마다 나무를 새로 배치하고 시간을 처음부터 잰다.
    private void StartSession()
    {
        _sessionEndPanel.Hide();

        _playerSpawner.MoveToSpawnPoint(_player);
        _gameCamera.SetTarget(_player.transform);
        _treeSpawner.Spawn(_startTree);

        _woodAtSessionStart = _wallet.Wood.CurrentValue;
        _playerInput.ActivateInput();
        _sessionTimer.Begin();
    }

    private void EndSession()
    {
        _playerInput.DeactivateInput();
        _playerMovement.Stop();

        _sessionEndPanel.Show(_wallet.Wood.CurrentValue - _woodAtSessionStart);
    }

    private void HandleTreeFelled(Trees tree)
    {
        _wallet.AddWood(tree.Data.Reward);
        _sessionTimer.AddTreeBonus();
    }
}
