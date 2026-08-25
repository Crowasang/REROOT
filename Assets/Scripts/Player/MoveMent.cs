using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveMent : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    
    private Rigidbody2D _rb2d;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }
    
    private void FixedUpdate()
    {
        Vector2 movement = _moveInput * (_moveSpeed * Time.fixedDeltaTime);
        
        _rb2d.MovePosition(_rb2d.position + movement);
    }
}
