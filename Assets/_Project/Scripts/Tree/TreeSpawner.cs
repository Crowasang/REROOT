using System.Collections.Generic;
using R3;
using UnityEngine;

// 숲(IForest)의 구현. 자리 계산은 ForestLayout, 오브젝트 재사용은 TreePool에 맡기고
// 이 클래스는 세션마다 숲을 깔고 치우며 쓰러진 나무를 알리는 일만 한다.
public class TreeSpawner : MonoBehaviour, IForest
{
    [Tooltip("나무를 깔 영역. 바닥(XZ) 크기만 쓴다.")]
    [SerializeField] private BoxCollider _spawnArea;
    [SerializeField] private Transform _treeRoot;

    [Header("배치")]
    [Tooltip("나무 밑동 사이의 격자 간격. 간격 - 밑동 지름이 플레이어 폭보다 작아야 베지 않고는 지나갈 수 없다.")]
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
    private readonly List<Pose> _poses = new();
    private readonly Subject<Trees> _onTreeHit = new();
    private readonly Subject<Trees> _onTreeFelled = new();
    private readonly Subject<Unit> _onCleared = new();
    private TreePool _pool;

    public int RemainingCount => _spawnedTrees.Count;
    public Observable<Trees> OnTreeHit => _onTreeHit;
    public Observable<Trees> OnTreeFelled => _onTreeFelled;
    public Observable<Unit> OnCleared => _onCleared;

    private void Awake()
    {
        Transform root = _treeRoot != null ? _treeRoot : transform;

        // 인스턴스는 재사용되므로 처음 만들 때 한 번만 구독한다.
        _pool = new TreePool(root, _poolDefaultCapacity, _poolMaxSize, tree =>
        {
            tree.OnHit.Subscribe(_onTreeHit.OnNext).AddTo(tree);
            tree.OnFelled.Subscribe(HandleTreeFelled).AddTo(tree);
        });
    }

    private void OnDestroy()
    {
        _onTreeHit.Dispose();
        _onTreeFelled.Dispose();
        _onCleared.Dispose();
    }

    public void Spawn(TreeData data)
    {
        Clear();

        Vector3? clearCenter = _keepClearPoint != null ? _keepClearPoint.position : null;
        ForestLayout.FillGrid(_poses, _spawnArea.bounds, _spacing, _positionJitter, _randomYaw, clearCenter, _keepClearRadius);

        foreach (Pose pose in _poses)
        {
            Trees tree = _pool.Get(data.Prefab);
            tree.transform.SetPositionAndRotation(pose.position, pose.rotation);
            _spawnedTrees.Add(tree);
        }
    }

    private void Clear()
    {
        foreach (Trees tree in _spawnedTrees)
        {
            if (tree != null)
            {
                _pool.Release(tree);
            }
        }

        _spawnedTrees.Clear();
    }

    private void HandleTreeFelled(Trees tree)
    {
        _spawnedTrees.Remove(tree);
        _onTreeFelled.OnNext(tree);
        _pool.Release(tree);

        // 쓰러짐 구독자(보상 등)가 모두 처리된 뒤에 알려야 마지막 나무 보상이 빠지지 않는다.
        if (_spawnedTrees.Count == 0)
        {
            _onCleared.OnNext(Unit.Default);
        }
    }
}
