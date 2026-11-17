using System.Collections.Generic;

// 파일로 저장되는 내용. 필드를 바꿀 때는 CURRENT_VERSION을 올리고 SaveService.Migrate에 옛 버전 변환을 추가한다.
public sealed class SaveData
{
    public const int CURRENT_VERSION = 1;

    public int Version { get; set; } = CURRENT_VERSION;
    public int Wood { get; set; }

    // 강화 id → 단계
    public Dictionary<string, int> UpgradeLevels { get; set; } = new();

    // 누적 기록. 엔딩 화면, 업적에 쓴다.
    public int TotalTreesFelled { get; set; }
    public int TotalSessions { get; set; }
}
