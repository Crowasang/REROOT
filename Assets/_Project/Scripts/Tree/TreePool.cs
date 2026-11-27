using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

// 나무 오브젝트 풀. 나무 종류(프리팹)마다 풀을 따로 두고, 꺼낸 인스턴스가 어느 풀 소속인지 기억한다.
public sealed class TreePool
{
    private readonly Transform _root;
    private readonly int _defaultCapacity;
    private readonly int _maxSize;
    private readonly Action<Trees> _onCreated;

    private readonly Dictionary<Trees, ObjectPool<Trees>> _poolByPrefab = new();
    private readonly Dictionary<Trees, ObjectPool<Trees>> _poolByInstance = new();

    // onCreated: 인스턴스를 처음 만들 때 한 번만 호출된다. 재사용되는 인스턴스에 구독을 걸 때 쓴다.
    public TreePool(Transform _root, int _defaultCapacity, int _maxSize, Action<Trees> _onCreated)
    {
        this._root = _root;
        this._defaultCapacity = _defaultCapacity;
        this._maxSize = _maxSize;
        this._onCreated = _onCreated;
    }

    public Trees Get(Trees _prefab)
    {
        return GetPool(_prefab).Get();
    }

    public void Release(Trees _tree)
    {
        _poolByInstance[_tree].Release(_tree);
    }

    private ObjectPool<Trees> GetPool(Trees _prefab)
    {
        if (_poolByPrefab.TryGetValue(_prefab, out ObjectPool<Trees> _pool))
        {
            return _pool;
        }

        _pool = new ObjectPool<Trees>(
            createFunc: () => Create(_prefab, _pool),
            actionOnGet: _tree =>
            {
                _tree.ResetState();
                _tree.gameObject.SetActive(true);
            },
            actionOnRelease: _tree => _tree.gameObject.SetActive(false),
            actionOnDestroy: _tree =>
            {
                _poolByInstance.Remove(_tree);
                Object.Destroy(_tree.gameObject);
            },
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize);

        _poolByPrefab.Add(_prefab, _pool);
        return _pool;
    }

    private Trees Create(Trees _prefab, ObjectPool<Trees> _pool)
    {
        Trees _tree = Object.Instantiate(_prefab, _root);
        _poolByInstance.Add(_tree, _pool);
        _onCreated?.Invoke(_tree);
        return _tree;
    }
}
