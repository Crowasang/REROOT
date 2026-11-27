using UnityEngine;
using VContainer;
using VContainer.Unity;

// 앱 전체에서 하나만 사는 조립 담당. VContainerSettings에 등록돼 게임이 켜질 때 자동으로 만들어지고,
// 씬이 바뀌어도 유지된다. 모든 씬의 LifetimeScope가 이것을 부모로 삼아 여기 등록된 것을 받아 쓴다.
// 씬을 넘어 유지돼야 하는 것(재화, 강화, 기록, 저장, 씬 전환)만 여기 둔다.
public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private UpgradeCatalog _upgradeCatalog;

    protected override void Configure(IContainerBuilder _builder)
    {
        _builder.RegisterInstance(_upgradeCatalog);

        // 진행 상태(저장 대상)
        _builder.Register<Wallet>(Lifetime.Singleton).AsImplementedInterfaces();
        _builder.Register<PlayerRecords>(Lifetime.Singleton).AsImplementedInterfaces();
        _builder.Register<UpgradeService>(Lifetime.Singleton).AsImplementedInterfaces();
        _builder.Register<StatService>(Lifetime.Singleton).AsImplementedInterfaces();

        // 저장·씬
        _builder.RegisterEntryPoint<SaveService>();
        _builder.RegisterEntryPoint<AutoSaveManager>();
        _builder.Register<SceneLoader>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
