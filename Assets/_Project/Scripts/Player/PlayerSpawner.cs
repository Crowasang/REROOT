using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

// 플레이어를 만들고 시작 위치로 돌려놓는다. 컨테이너로 만들어서 플레이어 컴포넌트에 PlayerStats 등이 주입된다.
public class PlayerSpawner : MonoBehaviour, IPlayerSpawner
{
    [SerializeField] private PlayerController _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    private readonly ReactiveProperty<IPlayerControl> _player = new(null);
    private IObjectResolver _resolver;
    private Rigidbody _playerBody;

    public ReadOnlyReactiveProperty<IPlayerControl> Player => _player;

    private void OnDestroy()
    {
        _player.Dispose();
    }

    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        _resolver = resolver;
    }

    public void Spawn()
    {
        PlayerController player = _resolver.Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
        _playerBody = player.GetComponent<Rigidbody>();
        _player.Value = player;
    }

    public void MoveToSpawnPoint()
    {
        IPlayerControl player = _player.CurrentValue;
        if (player == null)
        {
            return;
        }

        player.Transform.position = _spawnPoint.position;
        if (_playerBody != null)
        {
            _playerBody.position = _spawnPoint.position;
        }
    }
}
