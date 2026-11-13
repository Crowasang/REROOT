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

    public SceneLoader(ISaveService save)
    {
        _save = save;
    }

    public ReadOnlyReactiveProperty<bool> IsLoading => _isLoading;

    public async UniTask LoadAsync(SceneId scene)
    {
        if (_isLoading.Value)
        {
            return;
        }

        _isLoading.Value = true;
        try
        {
            await _save.SaveAsync();
            await SceneManager.LoadSceneAsync(GetSceneName(scene)).ToUniTask();
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
    private static string GetSceneName(SceneId scene)
    {
        return scene switch
        {
            SceneId.Title => "TitleScene",
            SceneId.Game => "GameScene",
            SceneId.Ending => "EndingScene",
            _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, null),
        };
    }
}
