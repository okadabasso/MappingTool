namespace ConsoleApp2.Commands;

using ConsoleAppFramework;
using MappingTool.Mapping;

[ConsoleAppFramework.RegisterCommands("simple-mapping")]

public class SimpleMappingSampleCommand
{
    [Command("method1")]
    public void Method1()
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
        var destination = mapper.Map(source);
        Console.WriteLine("Mapping completed successfully.");
        Console.WriteLine($"Destination IntValue: {destination.IntValue}");
        Console.WriteLine($"Destination ShortValue: {destination.ShortValue}");
        Console.WriteLine($"Destination LongValue: {destination.LongValue}");
        Console.WriteLine($"Destination DoubleValue: {destination.DoubleValue}");
        Console.WriteLine($"Destination FloatValue: {destination.FloatValue}");
        Console.WriteLine($"Destination DecimalValue: {destination.DecimalValue}");
        Console.WriteLine($"Destination BoolValue: {destination.BoolValue}");
        Console.WriteLine($"Destination StringValue: {destination.StringValue}");
        Console.WriteLine($"Destination DateTimeValue: {destination.DateTimeValue}");
        Console.WriteLine($"Destination NullableIntValue: {destination.NullableIntValue}");
        Console.WriteLine($"Destination NullableShortValue: {destination.NullableShortValue}");
        Console.WriteLine($"Destination NullableLongValue: {destination.NullableLongValue}");
        Console.WriteLine($"Destination NullableDoubleValue: {destination.NullableDoubleValue}");
        Console.WriteLine($"Destination NullableFloatValue: {destination.NullableFloatValue}");
        Console.WriteLine($"Destination NullableDecimalValue: {destination.NullableDecimalValue}");
        Console.WriteLine($"Destination NullableBoolValue: {destination.NullableBoolValue}");
        Console.WriteLine($"Destination NullableStringValue: {destination.NullableStringValue}");
        Console.WriteLine($"Destination NullableDateTimeValue: {destination.NullableDateTimeValue}");

        Console.WriteLine($"Destination DateTimeOffsetValue: {destination.DateTimeOffsetValue}");
        Console.WriteLine($"Destination GuidValue: {destination.GuidValue}");
        Console.WriteLine($"Destination TimeSpanValue: {destination.TimeSpanValue}");
        Console.WriteLine($"Destination NullableDateTimeOffsetValue: {destination.NullableDateTimeOffsetValue}");
        Console.WriteLine($"Destination NullableGuidValue: {destination.NullableGuidValue}");
        Console.WriteLine($"Destination NullableTimeSpanValue: {destination.NullableTimeSpanValue}");
    }
    [Command("method2")]
    public void Method2()
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

        var destination = new Destination(source);
        Console.WriteLine("Mapping completed successfully.");
        Console.WriteLine($"Destination IntValue: {destination.IntValue}");
        Console.WriteLine($"Destination ShortValue: {destination.ShortValue}");
        Console.WriteLine($"Destination LongValue: {destination.LongValue}");
        Console.WriteLine($"Destination DoubleValue: {destination.DoubleValue}");
        Console.WriteLine($"Destination FloatValue: {destination.FloatValue}");
        Console.WriteLine($"Destination DecimalValue: {destination.DecimalValue}");
        Console.WriteLine($"Destination BoolValue: {destination.BoolValue}");
        Console.WriteLine($"Destination StringValue: {destination.StringValue}");
        Console.WriteLine($"Destination DateTimeValue: {destination.DateTimeValue}");
        Console.WriteLine($"Destination NullableIntValue: {destination.NullableIntValue}");
        Console.WriteLine($"Destination NullableShortValue: {destination.NullableShortValue}");
        Console.WriteLine($"Destination NullableLongValue: {destination.NullableLongValue}");
        Console.WriteLine($"Destination NullableDoubleValue: {destination.NullableDoubleValue}");
        Console.WriteLine($"Destination NullableFloatValue: {destination.NullableFloatValue}");
        Console.WriteLine($"Destination NullableDecimalValue: {destination.NullableDecimalValue}");
        Console.WriteLine($"Destination NullableBoolValue: {destination.NullableBoolValue}");
        Console.WriteLine($"Destination NullableStringValue: {destination.NullableStringValue}");
        Console.WriteLine($"Destination NullableDateTimeValue: {destination.NullableDateTimeValue}");

        Console.WriteLine($"Destination DateTimeOffsetValue: {destination.DateTimeOffsetValue}");
        Console.WriteLine($"Destination GuidValue: {destination.GuidValue}");
        Console.WriteLine($"Destination TimeSpanValue: {destination.TimeSpanValue}");
        Console.WriteLine($"Destination NullableDateTimeOffsetValue: {destination.NullableDateTimeOffsetValue}");
        Console.WriteLine($"Destination NullableGuidValue: {destination.NullableGuidValue}");
        Console.WriteLine($"Destination NullableTimeSpanValue: {destination.NullableTimeSpanValue}");
    }
    [Command("method3")]
    public void Method3()
    {
        var source = new SourceWithLEnumerable();

        var mapper = new MapperFactory<SourceWithLEnumerable, DestinationWithCollections>().CreateMapper();
        var destination = mapper.Map(source);
        Console.WriteLine("Mapping completed successfully.");
        Console.WriteLine($"Destination IntValues: {string.Join(", ", destination.IntValues)}");
        Console.WriteLine($"Destination StringValues: {string.Join(", ", destination.StringValues)}");
        Console.WriteLine($"Destination IntArray: {string.Join(", ", destination.IntArray)}");
        Console.WriteLine($"Destination StringArray: {string.Join(", ", destination.StringArray)}");
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

    class SourceWithLEnumerable
    {
        public List<int> IntValues { get; set; } = new List<int> { 1, 2, 3 };
        public List<string> StringValues { get; set; } = new List<string> { "A", "B", "C" };
        public int[] IntArray { get; set; } = new int[] { 4, 5, 6 };
        public string[] StringArray { get; set; } = new string[] { "D", "E", "F" };
    }
    class DestinationWithCollections
    {
        public List<int> IntValues { get; set; } = new List<int>();
        public List<string> StringValues { get; set; } = new List<string>();
        public int[] IntArray { get; set; } = Array.Empty<int>();
        public string[] StringArray { get; set; } = Array.Empty<string>();
    }
}