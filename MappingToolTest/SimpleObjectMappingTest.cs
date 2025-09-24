using System;
using Xunit;
using MappingTool.Mapping;
namespace MappingToolTest;


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
    [Fact]
    public void MapToStruct_ShouldMapPropertiesCorrectly()
    {
        var source = new Source
        {
            IntValue = 42,
            ShortValue = 10,
            LongValue = 100000L,
            DoubleValue = 3.14,
            FloatValue = 2.71f,
            DecimalValue = 99.99m,
            BoolValue = true,
            StringValue = "Hello, World!",
            DateTimeValue = DateTime.Now,
            DateTimeOffsetValue = DateTimeOffset.Now,
            GuidValue = Guid.NewGuid(),
            TimeSpanValue = TimeSpan.FromHours(1),
            NullableIntValue = null,
            NullableShortValue = 20,
            NullableLongValue = null,
            NullableDoubleValue = 6.28,
            NullableFloatValue = null,
            NullableDecimalValue = 199.99m,
            NullableBoolValue = null,
            NullableStringValue = null,
            NullableDateTimeValue = DateTime.UtcNow,
            NullableDateTimeOffsetValue = DateTimeOffset.UtcNow,
            NullableGuidValue = Guid.NewGuid(),
            NullableTimeSpanValue = TimeSpan.FromHours(1)
        };
        var mapper = new MapperFactory<Source, Destination>().CreateMapper();
        // Act
        var destination = mapper.Map(source);
        var destination2 = new Destination(source);
        // Assert
        Assert.Equal(source.IntValue, destination.IntValue);
        Assert.Equal(source.ShortValue, destination.ShortValue);
        Assert.Equal(source.LongValue, destination.LongValue);
        Assert.Equal(source.DoubleValue, destination.DoubleValue);
        Assert.Equal(source.FloatValue, destination.FloatValue);
        Assert.Equal(source.DecimalValue, destination.DecimalValue);
        Assert.Equal(source.BoolValue, destination.BoolValue);
        Assert.Equal(source.StringValue, destination.StringValue);
        Assert.Equal(source.DateTimeValue, destination.DateTimeValue);
        Assert.Equal(source.DateTimeOffsetValue, destination.DateTimeOffsetValue);
        Assert.Equal(source.GuidValue, destination.GuidValue);
        Assert.Equal(source.TimeSpanValue, destination.TimeSpanValue);
        Assert.Equal(source.NullableIntValue, destination.NullableIntValue);
        Assert.Equal(source.NullableShortValue, destination.NullableShortValue);
        Assert.Equal(source.NullableLongValue, destination.NullableLongValue);
        Assert.Equal(source.NullableDoubleValue, destination.NullableDoubleValue);
        Assert.Equal(source.NullableFloatValue, destination.NullableFloatValue);
        Assert.Equal(source.NullableDecimalValue, destination.NullableDecimalValue);
        Assert.Equal(source.NullableBoolValue, destination.NullableBoolValue);
        Assert.Equal(source.NullableStringValue, destination.NullableStringValue);
        Assert.Equal(source.NullableDateTimeValue, destination.NullableDateTimeValue);
        Assert.Equal(source.NullableDateTimeOffsetValue, destination.NullableDateTimeOffsetValue);
        Assert.Equal(source.NullableGuidValue, destination.NullableGuidValue);
    }
    public class Source
    {
        public int IntValue { get; set; }
        public short ShortValue { get; set; }
        public long LongValue { get; set; }
        public double DoubleValue { get; set; }
        public float FloatValue { get; set; }
        public decimal DecimalValue { get; set; }
        public bool BoolValue { get; set; }

        public string StringValue { get; set; } = null!;
        public DateTime DateTimeValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }
        public Guid GuidValue { get; set; }
        public TimeSpan TimeSpanValue { get; set; }

        public int? NullableIntValue { get; set; }
        public short? NullableShortValue { get; set; }
        public long? NullableLongValue { get; set; }
        public double? NullableDoubleValue { get; set; }
        public float? NullableFloatValue { get; set; }
        public decimal? NullableDecimalValue { get; set; }
        public bool? NullableBoolValue { get; set; }

        public string? NullableStringValue { get; set; } = null!;
        public DateTime? NullableDateTimeValue { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetValue { get; set; }
        public Guid? NullableGuidValue { get; set; }
        public TimeSpan? NullableTimeSpanValue { get; set; }
    }

    public class Destination
    {
        public int IntValue { get; set; }
        public int ShortValue { get; set; }
        public int LongValue { get; set; }
        public double DoubleValue { get; set; }
        public float FloatValue { get; set; }
        public decimal DecimalValue { get; set; }
        public bool BoolValue { get; set; }

        public string StringValue { get; set; } = null!;
        public DateTime DateTimeValue { get; set; }

        public DateTimeOffset DateTimeOffsetValue { get; set; }
        public Guid GuidValue { get; set; }
        public TimeSpan TimeSpanValue { get; set; }
        public int? NullableIntValue { get; set; }
        public short? NullableShortValue { get; set; }
        public long? NullableLongValue { get; set; }
        public double? NullableDoubleValue { get; set; }
        public float? NullableFloatValue { get; set; }
        public decimal? NullableDecimalValue { get; set; }
        public bool? NullableBoolValue { get; set; }

        public string? NullableStringValue { get; set; } = null!;
        public DateTime? NullableDateTimeValue { get; set; }
        public DateTimeOffset? NullableDateTimeOffsetValue { get; set; }
        public Guid? NullableGuidValue { get; set; }
        public TimeSpan? NullableTimeSpanValue { get; set; }

        public Destination()
        {

        }
        public Destination(Source source)
        {
            var mapper = new MapperFactory<Source, Destination>().CreateMapper();
            mapper.Map(source, this);


        }

    }
    public class SimpleObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class SimpleObjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

}

