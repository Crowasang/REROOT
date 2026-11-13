using System.Collections.Generic;
using UnityEngine;

// 게임에 있는 모든 강화의 목록. 강화 화면은 이 순서대로 보여준다.
[CreateAssetMenu(fileName = "UpgradeCatalog", menuName = "REROOT/Upgrade Catalog")]
public class UpgradeCatalog : ScriptableObject
{
    [SerializeField] private UpgradeData[] _upgrades;

    public IReadOnlyList<UpgradeData> Upgrades => _upgrades;

    public UpgradeData Find(string id)
    {
        foreach (UpgradeData upgrade in _upgrades)
        {
            if (upgrade != null && upgrade.Id == id)
            {
                return upgrade;
            }
        }

        return null;
    }
}
