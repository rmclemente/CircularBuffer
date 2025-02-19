// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using circular.buffer.parallel.benchmark;

var runner = new RabbitMqRunner();
Console.WriteLine("Starting!");

var circularBufferTasks = new List<Task>();
for (int i = 0; i < 10; i++)
    circularBufferTasks.Add(Task.Run(() => runner.CircularBuffer()));

var circularBufferTimestamp = Stopwatch.GetTimestamp();
await Task.WhenAll(circularBufferTasks);
var circularBufferElapsedTime = Stopwatch.GetElapsedTime(circularBufferTimestamp);

Console.WriteLine("CircularBuffer Finished! Elapsed: {0}", circularBufferElapsedTime);

var singletonIChannel = new List<Task>();
for (int i = 0; i < 10; i++)
    singletonIChannel.Add(Task.Run(() => runner.SingletonIModel()));

var lockableSingletonIModelTimestamp = Stopwatch.GetTimestamp();
await Task.WhenAll(singletonIChannel);
var lockableSingletonElapsedTime = Stopwatch.GetElapsedTime(lockableSingletonIModelTimestamp);

Console.WriteLine("LockableSingletonIModel Finished! Elapsed: {0}", lockableSingletonElapsedTime);

// var lockableSingletonTasks = new List<Task>();
// for (int i = 0; i < 10; i++)
//     lockableSingletonTasks.Add(Task.Run(() => runner.LockableSingletonIModel()));

// var lockableSingletonIModelTimestamp = Stopwatch.GetTimestamp();
// await Task.WhenAll(lockableSingletonTasks);
// var lockableSingletonElapsedTime = Stopwatch.GetElapsedTime(lockableSingletonIModelTimestamp);

// Console.WriteLine("LockableSingletonIModel Finished! Elapsed: {0}", lockableSingletonElapsedTime);
Console.WriteLine("Finished!");
Console.ReadKey();


