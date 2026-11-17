using Cysharp.Threading.Tasks;

// 저장 파일 읽기·쓰기
public interface ISaveService
{
    // 모든 ISaveable의 상태를 모아 파일로 쓴다. 이미 저장 중이면 끝난 뒤 한 번 더 저장한다.
    UniTask SaveAsync();

    // 저장 파일을 지우고 모든 ISaveable을 처음 상태로 되돌린다. 설정 화면의 "처음부터" 같은 곳에서 쓴다.
    void ResetAll();
}
