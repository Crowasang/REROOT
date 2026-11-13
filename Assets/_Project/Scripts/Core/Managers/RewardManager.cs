using System;
using R3;
using VContainer.Unity;

// 나무를 베었을 때의 보상만 맡는다: 재화 지급, 세션 시간 보너스.
// 나중에 패시브 강화(추가 보상 등)는 여기서 계산을 늘리면 된다.
public sealed class RewardManager : IStartable, IDisposable
{
    private readonly IForest _forest;
    private readonly IWallet _wallet;
    private readonly ISessionTimer _timer;
    private readonly ISessionService _session;
    private readonly SessionSettings _settings;
    private readonly CompositeDisposable _disposables = new();

    public RewardManager(IForest forest, IWallet wallet, ISessionTimer timer, ISessionService session, SessionSettings settings)
    {
        _forest = forest;
        _wallet = wallet;
        _timer = timer;
        _session = session;
        _settings = settings;
    }

    public void Start()
    {
        _forest.OnTreeFelled.Subscribe(HandleTreeFelled).AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }

    private void HandleTreeFelled(Trees tree)
    {
        // 세션이 끝난 뒤에 마저 쓰러진 나무는 종료 화면의 획득량과 맞지 않으므로 세지 않는다.
        if (!_session.IsActive.CurrentValue)
        {
            return;
        }

        _wallet.AddWood(tree.Data.Reward);
        _timer.AddTime(_settings.BonusPerTree);
    }
}
