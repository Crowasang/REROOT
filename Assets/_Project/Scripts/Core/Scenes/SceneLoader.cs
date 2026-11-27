using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;

// 씬을 비동기로 불러온다(Root 계층). 씬을 떠나기 전에 저장해서 진행이 사라지지 않게 한다.
// 페이드나 로딩 화면은 IsLoading을 구독하는 쪽에서 붙이면 된다.
public sealed class SceneLoader : ISceneLoader, IDisposable
{
    private readonly ISaveService _save;
    private readonly ReactiveProperty<bool> _isLoading = new(false);

    public SceneLoader(ISaveService _save)
    {
        this._save = _save;
    }

    public ReadOnlyReactiveProperty<bool> IsLoading => _isLoading;

    public async UniTask LoadAsync(SceneId _scene)
    {
        if (_isLoading.Value)
        {
            return;
        }

        _isLoading.Value = true;
        try
        {
            await _save.SaveAsync();
            await SceneManager.LoadSceneAsync(GetSceneName(_scene)).ToUniTask();
        }
        finally
        {
            _isLoading.Value = false;
        }
    }

    public void Dispose()
    {
        _isLoading.Dispose();
    }

    // Build Settings에 들어간 씬 이름과 맞춘다.
    private static string GetSceneName(SceneId _scene)
    {
        return _scene switch
        {
            SceneId.Title => "TitleScene",
            SceneId.Game => "GameScene",
            SceneId.Ending => "EndingScene",
            _ => throw new ArgumentOutOfRangeException(nameof(_scene), _scene, null),
        };
    }
}
