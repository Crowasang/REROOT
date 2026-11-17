using R3;
using Cysharp.Threading.Tasks;

// 씬 전환. 페이드·로딩 화면은 구현 쪽에서 붙이고, 부르는 쪽은 어떤 씬으로 갈지만 안다.
public interface ISceneLoader
{
    ReadOnlyReactiveProperty<bool> IsLoading { get; }

    UniTask LoadAsync(SceneId scene);
}
