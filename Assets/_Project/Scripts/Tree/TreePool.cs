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
    public TreePool(Transform root, int defaultCapacity, int maxSize, Action<Trees> onCreated)
    {
        _root = root;
        _defaultCapacity = defaultCapacity;
        _maxSize = maxSize;
        _onCreated = onCreated;
    }

    public Trees Get(Trees prefab)
    {
        return GetPool(prefab).Get();
    }

    public void Release(Trees tree)
    {
        _poolByInstance[tree].Release(tree);
    }

    private ObjectPool<Trees> GetPool(Trees prefab)
    {
        if (_poolByPrefab.TryGetValue(prefab, out ObjectPool<Trees> pool))
        {
            return pool;
        }

        pool = new ObjectPool<Trees>(
            createFunc: () => Create(prefab, pool),
            actionOnGet: tree =>
            {
                tree.ResetState();
                tree.gameObject.SetActive(true);
            },
            actionOnRelease: tree => tree.gameObject.SetActive(false),
            actionOnDestroy: tree =>
            {
                _poolByInstance.Remove(tree);
                Object.Destroy(tree.gameObject);
            },
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize);

        _poolByPrefab.Add(prefab, pool);
        return pool;
    }

    private Trees Create(Trees prefab, ObjectPool<Trees> pool)
    {
        Trees tree = Object.Instantiate(prefab, _root);
        _poolByInstance.Add(tree, pool);
        _onCreated?.Invoke(tree);
        return tree;
    }
}
