using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using MappingTool.Mapping;
namespace MappingToolTest.Benchmarks
{
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

    [MemoryDiagnoser]
    [WarmupCount(3)]
    [IterationCount(10)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]

    public class MappingBenchmark
    {
        private readonly SimpleMapper<Source, Destination> _simpleMapper = new();
        private readonly IMapper _autoMapper;
        private readonly List<Source> _sourceList;

        public MappingBenchmark()
        {
            // AutoMapper の設定
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });
            _autoMapper = config.CreateMapper();

            // テストデータの準備
            _sourceList = new List<Source>();
            for (int i = 0; i < 1000; i++)
            {
                _sourceList.Add(new Source { Id = i, Name = $"Name{i}" });
            }
        }

        [Benchmark]
        public int SimpleMapper_Single()
        {
            foreach (var source in _sourceList)
            {
                var destination = _simpleMapper.Map(source);
            }
            return _sourceList.Count;
        }

        [Benchmark]
        public int AutoMapper_Single()
        {
            foreach (var source in _sourceList)
            {
                var destination = _autoMapper.Map<Destination>(source);
            }
            return _sourceList.Count;
        }

        [Benchmark]
        public int SimpleMapper_Bulk()
        {
            var destinations = _simpleMapper.Map(_sourceList);
            return destinations.Count();
        }

        [Benchmark]
        public int AutoMapper_Bulk()
        {
            var destinations = _autoMapper.Map<List<Destination>>(_sourceList);
            return destinations.Count;
        }
    }

}