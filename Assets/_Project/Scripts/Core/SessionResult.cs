// 끝난 세션의 결과. 종료 화면과 저장이 쓴다.
public readonly struct SessionResult
{
    public SessionResult(SessionEndReason reason, int woodGained)
    {
        Reason = reason;
        WoodGained = woodGained;
    }

    public SessionEndReason Reason { get; }
    public int WoodGained { get; }
}
