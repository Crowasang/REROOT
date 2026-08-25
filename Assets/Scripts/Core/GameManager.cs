using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerSpwaner _playerSpwaner;

    private void Start()
    {
        InitailzeGame();
    }

    private void InitailzeGame()
    {
        _playerSpwaner.PlayerSpawn();
    }
}
