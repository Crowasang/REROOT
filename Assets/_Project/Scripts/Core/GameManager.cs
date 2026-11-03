using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [FormerlySerializedAs("_playerSpwaner")]
    [SerializeField] private PlayerSpawner _playerSpawner;
    [SerializeField] private TreeSpawner _treeSpawner;
    [SerializeField] private MapBoundary _mapBoundary;
    [SerializeField] private GameCamera _gameCamera;
    [SerializeField] private TreeData _startTree;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        _mapBoundary.Build();
        _gameCamera.SetBounds(_mapBoundary.CameraRect);

        GameObject player = _playerSpawner.PlayerSpawn();
        _gameCamera.SetTarget(player.transform);

        _treeSpawner.Spawn(_startTree);
    }
}
