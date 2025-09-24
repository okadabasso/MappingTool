using MappingTool.Mapping;

namespace MappingToolTest;
public class RecordMappingTest
{
    // テスト用のクラス
    public record Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public record DestinationRecord(int Id, string Name);

    [Fact]
    public void Map_Record_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var source = new Source { Id = 1, Name = "Test" };
        var mapper = new MapperFactory<Source, DestinationRecord>().CreateMapper();

        // Act
        var destination = mapper.Map(source);

        // Assert
        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Name, destination.Name);
    }
}