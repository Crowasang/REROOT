using System;
using UnityEngine;

// 강화 한 단계마다 어떤 수치를 얼마나 바꾸는지. 강화 하나가 여러 효과를 가질 수 있다.
[Serializable]
public struct UpgradeEffect
{
    [SerializeField] private StatType _stat;
    [SerializeField] private StatModifierType _modifierType;
    [Tooltip("한 단계당 값. Flat이면 더하는 양, Percent면 비율(0.1 = +10%).")]
    [SerializeField] private float _valuePerLevel;

    public StatType Stat => _stat;
    public StatModifierType ModifierType => _modifierType;
    public float ValuePerLevel => _valuePerLevel;
}
