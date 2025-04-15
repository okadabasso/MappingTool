public record DestinationRecord(int Id, string Name)
{
    // 主コンストラクターを使用してプロパティを初期化
    public DestinationRecord() : this(0, string.Empty) { }
    public DestinationRecord(int id) : this(id, string.Empty) { }

}