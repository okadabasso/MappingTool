namespace MappingToolTest;

using System;
using MappingTool.Mapping;
public class EnumerableMappingTest
{
    [Fact]
    public void Map_List_ShouldMapElementsCorrectly()
    {
        // Arrange
        var sourceList = new List<Source>
        {
            new Source {
                Id = 1,
                Name = "Test1",
                IntArray = new int[] { 1, 2, 3 },
                StringList = new List<string> { "A", "B" } },
            new Source {
                Id = 2,
                Name = "Test2",
                IntArray = new int[] { 4, 5, 6 },
                 StringList = new List<string> { "C", "D" } }
        };
        var mapper = new MapperFactory<Source, Destination>().CreateMapper();

        // Act
        var destinationList = mapper.Map(sourceList).ToList();

        // Assert
        Assert.Equal(sourceList.Count, destinationList.Count);
        for (int i = 0; i < sourceList.Count; i++)
        {
            Assert.Equal(sourceList[i].Id, destinationList[i].Id);
            Assert.Equal(sourceList[i].Name, destinationList[i].Name);
            Assert.Equal(sourceList[i].IntArray, destinationList[i].IntArray);
            Assert.Equal(sourceList[i].StringList, destinationList[i].StringList);
        }
    }

    [Fact]
    public void Map_Array_ShouldMapElementsCorrectly()
    {
        // Arrange
        var sourceArray = new[]
        {
            new Source {
                Id = 1,
                Name = "Test1",
                IntArray = new int[] { 1, 2, 3 },
                StringList = new List<string> { "A", "B" } },
            new Source {
                Id = 2,
                Name = "Test2",
                IntArray = new int[] { 4, 5, 6 },
                StringList = new List<string> { "C", "D" } }
        };
        var mapper = new MapperFactory<Source, Destination>(allowRecursion: true).CreateMapper();

        // Act
        var destinationArray = mapper.Map(sourceArray).ToArray();

        // Assert
        Assert.Equal(sourceArray.Length, destinationArray.Length);
        for (int i = 0; i < sourceArray.Length; i++)
        {
            Console.WriteLine(destinationArray[i].IntArray);
            Assert.Equal(sourceArray[i].Id, destinationArray[i].Id);
            Assert.Equal(sourceArray[i].Name, destinationArray[i].Name);
            Assert.Equal(sourceArray[i].IntArray, destinationArray[i].IntArray);
            Assert.Equal(sourceArray[i].StringList, destinationArray[i].StringList);
        }
    }

    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int[] IntArray { get; set; } = Array.Empty<int>();
        public List<string> StringList { get; set; } = new List<string>();
    }

    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int[] IntArray { get; set; } = Array.Empty<int>();
        public List<string> StringList { get; set; } = new List<string>();

    }
}