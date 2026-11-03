using R3;
using UnityEngine;

// 플레이어가 가진 재화. 강화 구매, 저장이 여기서 값을 읽고 쓴다.
public class Wallet : MonoBehaviour
{
    private readonly ReactiveProperty<int> _wood = new(0);

    public ReadOnlyReactiveProperty<int> Wood => _wood;

    private void OnDestroy()
    {
        _wood.Dispose();
    }

    public void AddWood(int amount)
    {
        _wood.Value += amount;
    }
}
