using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private Transform _spawnPoint;

    public GameObject SpawnPlayer()
    {
        return Instantiate(_player, _spawnPoint.position, Quaternion.identity);
    }

    public void MoveToSpawnPoint(GameObject player)
    {
        player.transform.position = _spawnPoint.position;
        if (player.TryGetComponent(out Rigidbody2D rb))
        {
            rb.position = _spawnPoint.position;
        }
    }
}
