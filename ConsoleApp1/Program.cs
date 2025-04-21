
using BenchmarkDotNet.Running;
using ConsoleApp1.Benchmarks;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;
using System.Numerics;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;

// BenchmarkRunner.Run<MappingBenchmark>();

var source = new SourceData { Id = 1, Name = "Test" };


Expression<Func<SourceData, T>> GetProperty<SourceData, T>(Expression<Func<SourceData, T>> expression)
{
    return expression ;  ;
}