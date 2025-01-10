﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Borealis.Benchmarks;

[Config(typeof(Config))]
public class SampleBenchmark {
    [Benchmark(Baseline = true)]
    public int Add() {
        return 1 + 2;
    }

    [Benchmark]
    public int Substract() {
        return 1 - 2;
    }

    [Benchmark]
    public int Multiply() {
        return 1 * 2;
    }

    [Benchmark]
    public int Divide() {
        return 1 / 2;
    }

    private class Config : ManualConfig {
        public Config() {
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }
}
