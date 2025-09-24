using System;
using Xunit;
using MappingTool.Mapping; // MappingTool.Mapping.SimpleMapper を利用

namespace MappingToolTest
{
    public class SimpleObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SimpleObjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SimpleObjectMappingTest
    {
        [Fact]
        public void MapToDto_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var source = new SimpleObject { Id = 1, Name = "Test" };

            var mapper = new MapperFactory<SimpleObject, SimpleObjectDto>().CreateMapper();
            // Act
            var dto = mapper.Map(source);

            // Assert
            Assert.Equal(source.Id, dto.Id);
            Assert.Equal(source.Name, dto.Name);
        }
    }
}
