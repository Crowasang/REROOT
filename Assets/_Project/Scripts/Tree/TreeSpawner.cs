using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Pool;

public class TreeSpawner : MonoBehaviour
{
    [Tooltip("나무를 깔 영역. 바닥(XZ) 크기만 쓴다.")]
    [SerializeField] private BoxCollider _spawnArea;
    [SerializeField] private Transform _treeRoot;

    [Header("배치")]
    [Tooltip("나무 밑동 사이의 격자 간격. 3/4 시점이라 콜라이더는 밑동만 덮으므로 간격을 따로 정한다. " +
             "간격 - 밑동 지름이 플레이어 폭보다 작아야 베지 않고는 지나갈 수 없다.")]
    [SerializeField] private float _spacing = 1f;
    [Tooltip("격자 위치에서 무작위로 흔드는 거리. 0이면 고르게 놓인다.")]
    [SerializeField] private float _positionJitter = 0f;
    [Tooltip("나무마다 Y축으로 무작위 회전. 같은 모델이 반복돼 보이는 것을 줄인다.")]
    [SerializeField] private bool _randomYaw = true;

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

    // 아직 다 쓰러지지 않은 나무 수. 쓰러진 나무는 OnTreeFelled 발행 전에 빠진다.
    public int RemainingCount => _spawnedTrees.Count;

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

        // 바닥(XZ) 위에 격자를 만들고, 남는 여백은 양쪽에 똑같이 나눈다.
        int columns = Mathf.FloorToInt(bounds.size.x / _spacing) + 1;
        int rows = Mathf.FloorToInt(bounds.size.z / _spacing) + 1;
        Vector3 origin = new(
            bounds.center.x - (columns - 1) * _spacing * 0.5f,
            0f,
            bounds.center.z - (rows - 1) * _spacing * 0.5f);

        float clearSqr = _keepClearRadius * _keepClearRadius;

        for (int z = 0; z < rows; z++)
        {
            for (int x = 0; x < columns; x++)
            {
                Vector3 position = origin + new Vector3(x, 0f, z) * _spacing;

                if (IsInKeepClearArea(position, clearSqr))
                {
                    continue;
                }

                Vector2 jitter = Random.insideUnitCircle * _positionJitter;
                position += new Vector3(jitter.x, 0f, jitter.y);

                // 같은 모델이 똑같은 방향으로 늘어서 보이지 않도록 제자리에서 돌린다.
                Quaternion rotation = _randomYaw ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;

                Trees tree = pool.Get();
                tree.transform.SetPositionAndRotation(position, rotation);
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

    // 높이는 무시하고 바닥 위 거리로만 잰다.
    private bool IsInKeepClearArea(Vector3 position, float clearSqr)
    {
        if (_keepClearPoint == null)
        {
            return false;
        }

        Vector3 offset = _keepClearPoint.position - position;
        offset.y = 0f;
        return offset.sqrMagnitude < clearSqr;
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
