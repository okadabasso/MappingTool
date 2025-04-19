using MappingTool.Mapping;
using Xunit;

namespace MappingToolTest
{
    public class SimpleMapperTests
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
            public string Name { get; set; }
        }
        public struct DestinationStructWithDefaultConstructor
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public DestinationStructWithDefaultConstructor()
            {
                
            }
        }
        public struct DestinationStructWithConstructor
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public DestinationStructWithConstructor(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
        public record DestinationRecord(int Id, string Name);

        [Fact]
        public void Map_Class_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var source = new Source { Id = 1, Name = "Test" };
            var mapper = MapperFactory<Source, Destination>.CreateMapper();

            // Act
            var destination = mapper.Map(source);

            // Assert
            Assert.Equal(source.Id, destination.Id);
            Assert.Equal(source.Name, destination.Name);
        }

        [Fact]
        public void Map_Struct_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var source = new Source { Id = 2, Name = "StructTest" };
            
            Assert.Throws<InvalidOperationException>(() => {
            var mapper = MapperFactory<Source, DestinationStruct>.CreateMapper();
            });
        }
        [Fact]
        public void Map_Struct_WIthNoConstructor_ShouldThrowException()
        {
            // Arrange
            var source = new Source { Id = 2, Name = "StructTest" };
            var mapper = MapperFactory<Source, DestinationStructWithDefaultConstructor>.CreateMapper();

            // Act
            var destination = mapper.Map(source);

            // Assert
            Assert.Equal(source.Id, destination.Id);
            Assert.Equal(source.Name, destination.Name);
        }
        [Fact]
        public void Map_Struct_MapPropertiesCorrectly_WithConstructor()
        {
            // Arrange
            var source = new Source { Id = 2, Name = "StructTest" };
            var mapper = MapperFactory<Source, DestinationStructWithDefaultConstructor>.CreateMapper();

            // Act
            var destination = mapper.Map(source);

            // Assert
            Assert.Equal(source.Id, destination.Id);
            Assert.Equal(source.Name, destination.Name);
        }

        [Fact]
        public void Map_Record_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var source = new Source { Id = 3, Name = "RecordTest" };
            var mapper = MapperFactory<Source, DestinationRecord>.CreateMapper();

            // Act
            var destination = mapper.Map(source);

            // Assert
            Assert.Equal(source.Id, destination.Id);
            Assert.Equal(source.Name, destination.Name);
        }

        [Fact]
        public void Map_Enumerable_ShouldMapAllItems()
        {
            // Arrange
            var sources = new List<Source>
            {
                new Source { Id = 1, Name = "Item1" },
                new Source { Id = 2, Name = "Item2" }
            };
            var mapper = MapperFactory<Source, Destination>.CreateMapper();

            // Act
            var destinations = mapper.Map(sources);

            // Assert
            Assert.Equal(sources.Count, destinations.Count());
            Assert.Equal(sources[0].Id, destinations.ElementAt(0).Id);
            Assert.Equal(sources[0].Name, destinations.ElementAt(0).Name);
            Assert.Equal(sources[1].Id, destinations.ElementAt(1).Id);
            Assert.Equal(sources[1].Name, destinations.ElementAt(1).Name);
        }

        [Fact]
        public void Map_WithNullSource_ShouldThrowException()
        {
            // Arrange
            var mapper = MapperFactory<Source, Destination>.CreateMapper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mapper.Map((Source)null!));
        }
    }
}