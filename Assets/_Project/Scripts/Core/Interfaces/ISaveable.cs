// 세이브 대상. 이 인터페이스를 구현하고 컨테이너에 등록하면 SaveService가 불러오기·저장 때 알아서 부른다.
// 새 시스템이 저장할 게 생기면 SaveData에 필드를 추가하고 이것만 구현하면 된다.
public interface ISaveable
{
    // 게임 시작 시 한 번. 저장 파일이 없으면 기본값이 들어 있는 SaveData가 온다.
    void Load(SaveData data);

    // 저장 직전. 자기 상태를 data에 써 넣는다.
    void Write(SaveData data);
}
