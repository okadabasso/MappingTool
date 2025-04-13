using MappingTool.Mapping;
using Xunit;

namespace MappingToolTest;

public class UnitTest1
{
    // テスト用のクラス
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public struct DestinationStruct
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public DestinationStruct()
        {
        }
        public DestinationStruct(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public record DestinationRecord(int Id, string Name);

    [Fact]
    public void MapObject_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var source = new Source { Id = 1, Name = "Test" };
        var destination = new Destination();
        var mapper = new SimpleMapper<Source, Destination>();

        // Act
        mapper.Map(source, destination);

        // Assert
        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Name, destination.Name);
    }

    [Fact]
    public void MapObject_ShouldReturnMappedInstance()
    {
        // Arrange
        var source = new Source { Id = 2, Name = "Example" };
        var mapper = new SimpleMapper<Source, Destination>();

        // Act
        var destination = mapper.Map(source);

        // Assert
        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Name, destination.Name);
    }

    [Fact]
    public void MapStruct_ShouldMapStructCorrectly()
    {
        // Arrange
        var source = new Source { Id = 3, Name = "StructTest" };
        var mapper = new SimpleMapper<Source, DestinationStruct>();

        // Act
        var destination = mapper.MapStruct(source);

        // Assert
        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Name, destination.Name);
    }

    // [Fact]
    // public void MapRecord_ShouldMapRecordCorrectly()
    // {
    //     // Arrange
    //     var source = new Source { Id = 4, Name = "RecordTest" };
    //     var mapper = new SimpleMapper<Source, DestinationRecord>();

    //     // Act
    //     var destination = mapper.MapRecord(source);

    //     // Assert
    //     Assert.Equal(source.Id, destination.Id);
    //     Assert.Equal(source.Name, destination.Name);
    // }

    [Fact]
    public void MapEnumerable_ShouldMapAllItems()
    {
        // Arrange
        var sources = new List<Source>
        {
            new Source { Id = 1, Name = "Item1" },
            new Source { Id = 2, Name = "Item2" }
        };
        var mapper = new SimpleMapper<Source, Destination>();

        // Act
        var destinations = mapper.Map(sources);

        // Assert
        Assert.Equal(sources.Count, destinations.Count());
        Assert.Equal(sources[0].Id, destinations.ElementAt(0).Id);
        Assert.Equal(sources[0].Name, destinations.ElementAt(0).Name);
        Assert.Equal(sources[1].Id, destinations.ElementAt(1).Id);
        Assert.Equal(sources[1].Name, destinations.ElementAt(1).Name);
    }
}