
using BenchmarkDotNet.Running;
using ConsoleApp1.Benchmarks;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;
using System.Numerics;
using System.Xml.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using ConsoleApp1.Samples;
using Perfolizer;

// BenchmarkRunner.Run<MappingBenchmark>();

Sample1 sample = new Sample1();
Sample1.SampleMethod1();