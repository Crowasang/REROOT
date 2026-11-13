using System;
using R3;
using UnityEngine;
using VContainer.Unity;

// 나무를 베었을 때의 보상만 맡는다: 재화 지급, 세션 시간 보너스.
// 보상량은 IStatService를 거치므로 "획득량 +20%" 같은 강화는 데이터만 추가하면 반영된다.
public sealed class RewardManager : IStartable, IDisposable
{
    private readonly IForest _forest;
    private readonly IWallet _wallet;
    private readonly ISessionTimer _timer;
    private readonly ISessionService _session;
    private readonly IStatService _stats;
    private readonly SessionSettings _settings;
    private readonly CompositeDisposable _disposables = new();

    public RewardManager(IForest forest, IWallet wallet, ISessionTimer timer, ISessionService session, IStatService stats, SessionSettings settings)
    {
        _forest = forest;
        _wallet = wallet;
        _timer = timer;
        _session = session;
        _stats = stats;
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

        _wallet.AddWood(Mathf.RoundToInt(_stats.Evaluate(StatType.WoodGain, tree.Data.Reward)));
        _timer.AddTime(_stats.Evaluate(StatType.TreeBonusTime, _settings.BonusPerTree));
    }
}
