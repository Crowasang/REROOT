using System;
using UnityEngine;

public abstract class Tree : MonoBehaviour
{
    [SerializeField] protected int _maxHp = 10;
    
    protected int _currentHp;

    protected virtual void Awake()
    {
        _currentHp = _maxHp;
    }

    protected void TakeDamage(int damage)
    {
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            _currentHp = 0;
            OnDestroyed();
        }
    }

    protected abstract void OnDestroyed();
}
