
using BenchmarkDotNet.Running;
using ConsoleApp1.Benchmarks;
using ConsoleApp1.Data;
using ConsoleApp1.Shared;
using System.Numerics;
using System.Xml.Linq;

var summary = BenchmarkRunner.Run<MappingBenchmark>();

//var i = 1;

//var source = new SourceData()
//{

//    Id = i,
//    Name = $"Name_{i}",
//    Created = DateTime.Now.AddDays(-i),
//    IsActive = i % 2 == 0,
//    Score = i * 1.23,
//    Balance = i * 1000.50m,
//    Token = Guid.NewGuid(),
//    Description = $"Description for {i}",
//    Age = 20 + (i % 30),
//    Category = $"Category_{i % 5}",
//    Expiration = DateTime.Now.AddDays(i),
//    IsDeleted = i % 10 == 0,
//    TotalCount = i * 1000L,
//    Ratio = (float)(i * 0.1),
//    Level = (short)(i % 10),
//    Status = (byte)(i % 256),
//    Initial = (char)('A' + (i % 26)),
//    Email = $"user{i}@example.com",
//    Phone = $"090-0000-{i:D4}",
//    Website = new Uri($"https://example.com/user/{i}")
//};
//var destination = new DestinationData();

//var mapper = new SimpleMapper<SourceData, DestinationData>();
//mapper.Map3(source, destination);

//Console.WriteLine($"Id: {destination.Id} email: {destination.Email}");

