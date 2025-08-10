namespace ConsoleApp1.Benchmarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using AutoMapper;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;

[MemoryDiagnoser] // メモリ使用量も測定
[WarmupCount(3)]
[IterationCount(10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class MappingBenchmark
{
    private List<SourceData> source = null!;
    private IMapper autoMapper = null!;
    private SimpleMapper<SourceData, DestinationData> simpleMapper = null!;

    [GlobalSetup]
    public void Setup()
    {
        source = GenerateTestData(1000);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceData, DestinationData>();
        });

        autoMapper = config.CreateMapper();
        simpleMapper = new SimpleMapper<SourceData, DestinationData>();
    }

    [Benchmark(Baseline = true)]
    public int MapWithSimpleMapper()
    {
        var destination = new List<DestinationData>(source.Count);
        foreach (var item in source)
        {
            var dest = new DestinationData();
            simpleMapper.Map(item, dest);
            destination.Add(dest);
        }
        return destination.Count;
    }
    [Benchmark]
    public int MapWithSimpleMapper2()
    {
        var destination = new List<DestinationData>(source.Count);
        foreach (var item in source)
        {
            var dest = new DestinationData();
            simpleMapper.Map2(item, dest);
            destination.Add(dest);
        }
        return destination.Count;
    }
    [Benchmark]
    public int MapWithSimpleMapper3()
    {
        var destination = new List<DestinationData>(source.Count);
        foreach (var item in source)
        {
            var dest = new DestinationData();
            simpleMapper.Map3(item, dest);
            destination.Add(dest);
        }
        return destination.Count;
    }
    [Benchmark]
    public int MapWithSimpleMapper4()
    {
        var destination = new List<DestinationData>(source.Count);
        foreach (var item in source)
        {
            var dest = simpleMapper.Map4(item);
            destination.Add(dest);
        }
        return destination.Count;
    }
    [Benchmark]
    public int MapWithSimpleMapper5()
    {
        var destination = new List<DestinationData>(source.Count);
        foreach (var item in source)
        {
            var dest = simpleMapper.Map5(item);
            destination.Add(dest);
        }
        return destination.Count;
    }
    [Benchmark]
    public int MapWithSimpleMapper5List()
    {
        var destination = simpleMapper.Map5(source).ToList();
        return destination.Count;
    }

    [Benchmark]
    public int MapDefault()
    {
        var destination = new List<DestinationData>(source.Count);
        foreach (var item in source)
        {
            var dest = new DestinationData()
            {
                Id = item.Id,
                // Name = item.Name,
                Created = item.Created,
                IsActive = item.IsActive,
                Score = item.Score,
                Balance = item.Balance,
                Token = item.Token,
                Description = item.Description,
                Age = item.Age,
                Category = item.Category,
                Expiration = item.Expiration,
                IsDeleted = item.IsDeleted,
                TotalCount = item.TotalCount,
                Ratio = item.Ratio,
                Level = item.Level,
                Status = item.Status,
                Initial = item.Initial,
                Email = item.Email,
                Phone = item.Phone,
                Website = item.Website
            };
            destination.Add(dest);
        }
        return destination.Count;
    }
    [Benchmark]
    public int MapWithAutoMapper()
    {
        var destination = autoMapper.Map<List<DestinationData>>(source);
        return destination.Count;
    }
    [Benchmark]
    public int MapWithAutoMapper2()
    {
        var destination = new List<DestinationData>();
        foreach (var item in source)
        {
            var dest = new DestinationData();
            autoMapper.Map(item, dest);
            destination.Add(dest);
        }
        return destination.Count;
    }

    // データ生成（ベンチマーク用）
    private static List<SourceData> GenerateTestData(int count)
    {
        var list = new List<SourceData>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new SourceData
            {
                Id = i,
                Name = $"Name_{i}",
                Created = DateTime.Now.AddDays(-i),
                IsActive = i % 2 == 0,
                Score = i * 1.23,
                Balance = i * 1000.50m,
                Token = Guid.NewGuid(),
                Description = $"Description for {i}",
                Age = 20 + (i % 30),
                Category = $"Category_{i % 5}",
                Expiration = DateTime.Now.AddDays(i),
                IsDeleted = i % 10 == 0,
                TotalCount = i * 1000L,
                Ratio = (float)(i * 0.1),
                Level = (short)(i % 10),
                Status = (byte)(i % 256),
                Initial = (char)('A' + (i % 26)),
                Email = $"user{i}@example.com",
                Phone = $"090-0000-{i:D4}",
                Website = new Uri($"https://example.com/user/{i}")
            });
        }
        return list;
    }
}
