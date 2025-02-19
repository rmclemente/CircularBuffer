// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using circular.buffer.benchmark;

var summary = BenchmarkRunner.Run<RabbitMqRunner>();
