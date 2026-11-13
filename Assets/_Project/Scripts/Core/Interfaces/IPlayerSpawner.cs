using R3;

// 플레이어를 만들고 시작 위치로 돌려놓는다.
public interface IPlayerSpawner
{
    // 아직 만들지 않았으면 null
    ReadOnlyReactiveProperty<IPlayerControl> Player { get; }

    void Spawn();

    void MoveToSpawnPoint();
}
