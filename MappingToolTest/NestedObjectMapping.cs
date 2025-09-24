using MappingTool.Mapping;

namespace MappingToolTest;

public class NestedObjectMapping
{
    // テスト用のクラス
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public NestedSource Nested { get; set; } = null!;
    }

    public class NestedSource
    {
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public NestedDestination Nested { get; set; } = null!;
    }

    public class NestedDestination
    {
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    [Fact]
    public void Map_NestedObject_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var source = new Source
        {
            Id = 1,
            Name = "Test",
            Nested = new NestedSource
            {
                Description = "Nested Test",
                CreatedAt = new DateTime(2023, 1, 1)
            }
        };
        var mapper = new MapperFactory<Source, Destination>(allowRecursion: true).CreateMapper();

        // Act
        var destination = mapper.Map(source);

        // Assert
        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Name, destination.Name);
        Assert.NotNull(destination.Nested);
        Assert.Equal(source.Nested.Description, destination.Nested.Description);
        Assert.Equal(source.Nested.CreatedAt, destination.Nested.CreatedAt);
    }
}