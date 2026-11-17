using System.Collections.Generic;
using UnityEngine;

// 강화 하나의 정의. 새 강화는 이 에셋을 하나 만들어 UpgradeCatalog에 넣으면 끝난다.
[CreateAssetMenu(fileName = "Upgrade", menuName = "REROOT/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Tooltip("저장 파일에 쓰는 고유 키. 출시 후에는 바꾸지 않는다.")]
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField, TextArea] private string _description;
    [SerializeField] private Sprite _icon;

    [Header("단계와 비용")]
    [SerializeField, Min(1)] private int _maxLevel = 10;
    [Tooltip("1단계를 살 때의 비용")]
    [SerializeField, Min(0)] private int _baseCost = 20;
    [Tooltip("단계가 오를 때마다 비용에 곱하는 값")]
    [SerializeField, Min(1f)] private float _costGrowth = 1.5f;

    [Header("효과")]
    [SerializeField] private UpgradeEffect[] _effects;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public int MaxLevel => _maxLevel;
    public IReadOnlyList<UpgradeEffect> Effects => _effects;

    // currentLevel에서 다음 단계로 올리는 비용
    public int GetCost(int currentLevel)
    {
        return Mathf.RoundToInt(_baseCost * Mathf.Pow(_costGrowth, currentLevel));
    }
}
