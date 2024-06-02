using BenchmarkDotNet.Running;
using System.Reflection;

// Benchmarks must be run with Release configuration and withot Debugging session
BenchmarkSwitcher
    .FromAssembly(Assembly.GetExecutingAssembly())
    .Run(args);