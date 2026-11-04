using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Pool;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _spawnArea;
    [SerializeField] private Transform _treeRoot;

    [Header("배치")]
    [Tooltip("나무 콜라이더 사이에 항상 남겨 둘 길의 폭. 0이면 빽빽해서 베어야 길이 열리고, 플레이어 폭보다 크면 사이로 지나갈 수 있다.")]
    [SerializeField] private float _pathWidth = 0f;
    [Tooltip("격자 위치에서 무작위로 흔드는 거리. 흔들어도 길 폭은 줄어들지 않도록 간격에 반영된다.")]
    [SerializeField] private float _positionJitter = 0f;

    [Header("비워 둘 곳")]
    [Tooltip("이 지점 주변에는 나무를 놓지 않는다. 플레이어 스폰 지점을 넣는다.")]
    [SerializeField] private Transform _keepClearPoint;
    [SerializeField] private float _keepClearRadius = 1.5f;

    [Header("풀")]
    [SerializeField] private int _poolDefaultCapacity = 1100;
    [SerializeField] private int _poolMaxSize = 1500;

    private readonly List<Trees> _spawnedTrees = new();
    private readonly Subject<Trees> _onTreeFelled = new();

    // 나무 종류(프리팹)마다 풀을 따로 둔다.
    private readonly Dictionary<Trees, ObjectPool<Trees>> _poolByPrefab = new();
    private readonly Dictionary<Trees, ObjectPool<Trees>> _poolByInstance = new();

    private Transform TreeRoot => _treeRoot != null ? _treeRoot : transform;

    public IReadOnlyList<Trees> SpawnedTrees => _spawnedTrees;

    // 스폰한 나무 중 하나가 다 쓰러졌을 때 발행한다. 보상, 세션 시간 등이 구독한다.
    public Observable<Trees> OnTreeFelled => _onTreeFelled;

    private void OnDestroy()
    {
        _onTreeFelled.Dispose();
    }

    public void Spawn(TreeData data)
    {
        Clear();

        ObjectPool<Trees> pool = GetPool(data.Prefab);
        Bounds bounds = _spawnArea.bounds;

        // 이웃한 두 나무가 서로 반대쪽으로 최대한 흔들려도 길 폭이 남도록 간격을 정한다.
        float spacing = GetTreeRadius(data.Prefab) * 2f + _pathWidth + _positionJitter * 2f;

        // 영역 안에 들어가는 만큼 격자를 만들고, 남는 여백은 양쪽에 똑같이 나눈다.
        int columns = Mathf.FloorToInt(bounds.size.x / spacing) + 1;
        int rows = Mathf.FloorToInt(bounds.size.y / spacing) + 1;
        Vector2 origin = new(
            bounds.center.x - (columns - 1) * spacing * 0.5f,
            bounds.center.y - (rows - 1) * spacing * 0.5f);

        float clearSqr = _keepClearRadius * _keepClearRadius;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector2 position = origin + new Vector2(x, y) * spacing;

                if (_keepClearPoint != null && ((Vector2)_keepClearPoint.position - position).sqrMagnitude < clearSqr)
                {
                    continue;
                }

                position += Random.insideUnitCircle * _positionJitter;

                Trees tree = pool.Get();
                tree.transform.SetPositionAndRotation(position, Quaternion.identity);
                _spawnedTrees.Add(tree);
            }
        }
    }

    public void Clear()
    {
        foreach (Trees tree in _spawnedTrees)
        {
            if (tree != null)
            {
                _poolByInstance[tree].Release(tree);
            }
        }

        _spawnedTrees.Clear();
    }

    // 나무가 길을 막는 범위. 나무 종류마다 크기가 다르므로 프리팹의 콜라이더에서 읽는다.
    private static float GetTreeRadius(Trees prefab)
    {
        if (prefab.TryGetComponent(out CircleCollider2D circle))
        {
            Vector3 scale = prefab.transform.localScale;
            return circle.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
        }

        Debug.LogWarning($"[TreeSpawner] {prefab.name}에 CircleCollider2D가 없어 반지름을 0.5로 가정합니다.", prefab);
        return 0.5f;
    }

    private ObjectPool<Trees> GetPool(Trees prefab)
    {
        if (_poolByPrefab.TryGetValue(prefab, out ObjectPool<Trees> pool))
        {
            return pool;
        }

        pool = new ObjectPool<Trees>(
            createFunc: () => CreateTree(prefab, pool),
            actionOnGet: tree =>
            {
                tree.ResetState();
                tree.gameObject.SetActive(true);
            },
            actionOnRelease: tree => tree.gameObject.SetActive(false),
            actionOnDestroy: tree =>
            {
                _poolByInstance.Remove(tree);
                Destroy(tree.gameObject);
            },
            defaultCapacity: _poolDefaultCapacity,
            maxSize: _poolMaxSize);

        _poolByPrefab.Add(prefab, pool);
        return pool;
    }

    private Trees CreateTree(Trees prefab, ObjectPool<Trees> pool)
    {
        Trees tree = Instantiate(prefab, TreeRoot);
        _poolByInstance.Add(tree, pool);

        // 인스턴스는 재사용되므로 생성할 때 한 번만 구독한다.
        tree.OnFelled.Subscribe(HandleTreeFelled).AddTo(tree);
        return tree;
    }

    private void HandleTreeFelled(Trees tree)
    {
        _spawnedTrees.Remove(tree);
        _onTreeFelled.OnNext(tree);
        _poolByInstance[tree].Release(tree);
    }
}
