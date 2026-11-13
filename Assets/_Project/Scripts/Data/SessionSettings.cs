using UnityEngine;

// 한 판(세션)의 규칙 수치
[CreateAssetMenu(fileName = "Session", menuName = "REROOT/Session Settings")]
public class SessionSettings : ScriptableObject
{
    [SerializeField] private float _baseDuration = 30f;
    [Tooltip("나무 한 그루를 벨 때마다 늘어나는 시간(초)")]
    [SerializeField] private float _bonusPerTree = 1f;
    [Tooltip("세션마다 깔 나무 종류")]
    [SerializeField] private TreeData _startTree;

    public float BaseDuration => _baseDuration;
    public float BonusPerTree => _bonusPerTree;
    public TreeData StartTree => _startTree;
}
