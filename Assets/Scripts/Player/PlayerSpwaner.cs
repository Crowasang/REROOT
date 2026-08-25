using UnityEngine;

public class PlayerSpwaner : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private Transform _spawnPoint;

    public GameObject PlayerSpawn()
    {
        return Instantiate(_player, _spawnPoint.position, Quaternion.identity);
    }
}
