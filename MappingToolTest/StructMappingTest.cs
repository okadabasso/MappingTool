using MappingTool.Mapping;

namespace MappingToolTest;

public class StructMappingTest
{
    // テスト用のクラス
    public struct Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public int[] IntArray { get; set; } = null!;
        public List<string> StringList { get; set; } = null!;

        public Source(int id, string name)
        {
            Id = id;
            Name = name;
            IntArray = null!;
            StringList = null!;
        }
    }

    public struct DestinationStruct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int[] IntArray { get; set; } = null!;
        public List<string> StringList { get; set; } = null!;

        public DestinationStruct()
        {
            Id = 0;
            Name = string.Empty;
        }
    }
    public struct DestinationStructWithDefaultConstructor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int[] IntArray { get; set; } = null!;
        public List<string> StringList { get; set; } = null!;

        public DestinationStructWithDefaultConstructor()
        {

        }
    }
    public struct DestinationStructWithConstructor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int[] IntArray { get; set; } = null!;
        public List<string> StringList { get; set; } = null!;

        public DestinationStructWithConstructor(int id, string name, int[] intArray, List<string> stringList)
        {
            Id = id;
            Name = name;
            IntArray = intArray;
            StringList = stringList;
        }
    }

    [Fact]
    public void Map_Struct_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var source = new Source
        {
            Id = 1,
            Name = "Test",
            IntArray = new int[] { 1, 2, 3 },
            StringList = new List<string> { "A", "B", "C" }
        };
        var mapper1 = new MapperFactory<Source, DestinationStruct>().CreateMapper();
        var mapper2 = new MapperFactory<Source, DestinationStructWithDefaultConstructor>().CreateMapper();
        var mapper3 = new MapperFactory<Source, DestinationStructWithConstructor>().CreateMapper();

        // Act
        var destination1 = mapper1.Map(source);
        var destination2 = mapper2.Map(source);
        var destination3 = mapper3.Map(source);

        // Assert
        Assert.Equal(source.Id, destination1.Id);
        Assert.Equal(source.Name, destination1.Name);
        Assert.Equal(source.IntArray, destination1.IntArray);
        Assert.Equal(source.StringList, destination1.StringList);

        Assert.Equal(source.Id, destination2.Id);
        Assert.Equal(source.Name, destination2.Name);
        Assert.Equal(source.IntArray, destination2.IntArray);
        Assert.Equal(source.StringList, destination2.StringList);

        Assert.Equal(source.Id, destination3.Id);
        Assert.Equal(source.Name, destination3.Name);
        Assert.Equal(source.IntArray, destination3.IntArray);
        Assert.Equal(source.StringList, destination3.StringList);

    }
}