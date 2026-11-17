using UnityEngine;

[CreateAssetMenu(fileName = "TreeData", menuName = "REROOT/Tree Data")]
public class TreeData : ScriptableObject
{
    [SerializeField] private int _stage = 1;
    [SerializeField] private string _treeName;
    [SerializeField] private int _maxHp = 10;
    [SerializeField] private int _reward = 10;
    [SerializeField] private Trees _prefab;

    public int Stage => _stage;
    public string TreeName => _treeName;
    public int MaxHp => _maxHp;
    public int Reward => _reward;
    public Trees Prefab => _prefab;
}
