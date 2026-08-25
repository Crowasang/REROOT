using System;
using UnityEngine;

public class MoveMent : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    
    private Rigidbody2D _rb2d;
    private PlayerInputAction _playerInput;

    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _playerInput = new PlayerInputAction();
    }

    private void OnEnable()
    {
        _playerInput.Player.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Player.Disable();
    }
    
    
}
