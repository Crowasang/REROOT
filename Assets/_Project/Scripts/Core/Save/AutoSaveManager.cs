using System;
using Cysharp.Threading.Tasks;
using R3;
using VContainer.Unity;

// 언제 저장할지만 정한다(Root 계층): 강화를 산 직후.
// 세션이 끝날 때 저장은 게임 씬의 SessionSaveTrigger가 맡는다. 앱 종료 시 저장은 SaveService가 직접 한다.
public sealed class AutoSaveManager : IStartable, IDisposable
{
    private readonly ISaveService _save;
    private readonly IUpgradeService _upgrades;
    private IDisposable _subscription;

    public AutoSaveManager(ISaveService save, IUpgradeService upgrades)
    {
        _save = save;
        _upgrades = upgrades;
    }

    public void Start()
    {
        // 불러오기 때도 OnLevelChanged가 나오지만, 그건 Start 전에 끝나므로 여기서는 구매만 잡힌다.
        _subscription = _upgrades.OnLevelChanged.Subscribe(_ => _save.SaveAsync().Forget());
    }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
