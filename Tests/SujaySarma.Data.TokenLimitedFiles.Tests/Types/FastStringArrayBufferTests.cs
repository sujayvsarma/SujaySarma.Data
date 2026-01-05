using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.TokenLimitedFiles.Types;

namespace SujaySarma.Data.TokenLimitedFiles.Tests.Types;

[TestClass]
public sealed class FastStringArrayBufferTests
{

    [TestMethod(DisplayName = "FastStringArrayBuffer: Basic functionality")]
    [TestCategory("Functional")]
    public void BasicFunctionality()
    {
        using FastStringArrayBuffer buffer = new FastStringArrayBuffer();

        Assert.AreEqual(0, buffer.Length, $"buffer.Length: Expected: [0], Actual: [{buffer.Length}]");

        buffer.Append("Hello").Append("World").Append("!").Append("This").Append("is").Append("our").Append("FastStringArrayBuffer");
        Assert.AreEqual(7, buffer.Length, $"buffer.Length: Expected: [7], Actual: [{buffer.Length}]");

        string[] values = buffer.ToStringArray();
        Assert.AreEqual("HelloWorld!ThisisourFastStringArrayBuffer", string.Join("", values));

        buffer.Clear();
        values = buffer.ToStringArray();

        Assert.AreEqual(0, buffer.Length, $"buffer.Length: Expected: [0], Actual: [{buffer.Length}]");        
        Assert.AreEqual(Array.Empty<string>(), values, $"buffer.ToString(): Expected: [], Actual: [{string.Join(',', values)}]");
    }


    [TestMethod(DisplayName = "FastStringArrayBuffer: Insertion throughput (chars/ms)")]
    [TestCategory("Performance")]
    public void InsersionThroughput()
    {
        const int STRINGS = 100_000; // Reduced from 4096 * 1024
        List<string> strings = new List<string>(STRINGS);
        Random random = new Random();
        for (int i = 0; i < STRINGS; i++)
        {
            int l = random.Next(1, 16);
            StringBuilder field = new StringBuilder();
            for (int j = 0; j < l; j++)
            {
                int c = random.Next(0, 25);
                field.Append((char)('A' + c));
            }
            strings.Add(field.ToString());
        }

        using FastStringArrayBuffer buffer = new FastStringArrayBuffer();
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < STRINGS; i++)
        {
            buffer.Append(strings[i]);
        }
        sw.Stop();

        Console.WriteLine($"Throughput: Writing {STRINGS:N0} chars to buffer: {((sw.ElapsedMilliseconds > 0) ? buffer.Length/sw.ElapsedMilliseconds : 0.0):F2} chars/ms");
    }

    [TestMethod(DisplayName = "FastStringArrayBuffer: Memory usage (bytes/char)")]
    [TestCategory("Performance")]
    public void MemoryUsage()
    {
        const int STRINGS = 100_000; // Reduced from 4096 * 1024
        Random random = new Random();

        // Pre-generate test data to exclude string generation from memory measurement
        List<string> testStrings = new List<string>(STRINGS);
        for (int i = 0; i < STRINGS; i++)
        {
            int l = random.Next(1, 16);
            char[] chars = new char[l];
            for (int j = 0; j < l; j++)
            {
                chars[j] = (char)('A' + random.Next(0, 25));
            }
            testStrings.Add(new string(chars));
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        using FastStringArrayBuffer buffer = new FastStringArrayBuffer();
        long initial = GC.GetTotalMemory(false);
        
        for (int i = 0; i < STRINGS; i++)
        {
            buffer.Append(testStrings[i]);
        }
        
        long final = GC.GetTotalMemory(false);
        long memoryUsed = final - initial;

        Console.WriteLine($"Memory pressure: {STRINGS:N0} strings: {memoryUsed/(double)STRINGS:F2} bytes/string");
        Console.WriteLine($"Buffer internal length: {buffer.Length:N0} strings");
    }


    [TestMethod(DisplayName = "FastStringArrayBuffer: Memory usage - Repeated Clear/Fill, constant data size")]
    [TestCategory("Performance")]
    public void MemoryUsageWithRepeatedReuseWithConstantDataSize()
    {
        const int CHARS = 4096;
        const int ITERATIONS = 100_000;
        Random random = new Random();
        List<long> memoryUsage = new List<long>();

        long overhead = GC.GetTotalMemory(true);

        using FastStringArrayBuffer buffer = new FastStringArrayBuffer();
        long initial = GC.GetTotalMemory(false) - overhead;

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            buffer.Clear();

            StringBuilder field = new StringBuilder();
            for (int j = 0; j < CHARS; j++)
            {
                int c = random.Next(0, 25);
                field.Append(('A' + c));
            }
            buffer.Append(field.ToString());

            long final = GC.GetTotalMemory(false);
            long memoryUsed = final - initial;
            memoryUsage.Add(memoryUsed);
        }

        Console.WriteLine($"Iterations: === {ITERATIONS:N0} ===");
        Console.WriteLine($"Maximum memory: {memoryUsage.Max() / 1024.0:F2} bytes");
        Console.WriteLine($"Minimum memory: {memoryUsage.Min() / 1024.0:F2} bytes");
        Console.WriteLine($"Average memory: {memoryUsage.Average() / 1024.0:F2} bytes");

    }

    [TestMethod(DisplayName = "FastStringArrayBuffer: Memory usage - Repeated Clear/Fill, varying data size")]
    [TestCategory("Performance")]
    public void MemoryUsageWithRepeatedReuseWithVaryingDataSize()
    {
        const int MAX_CHARS = 4096;
        const int ITERATIONS = 100_000;
        Random random = new Random();
        Dictionary<int, double> bytesPerChar = new Dictionary<int, double>();

        long overhead = GC.GetTotalMemory(true);

        using FastStringArrayBuffer buffer = new FastStringArrayBuffer();
        long initial = GC.GetTotalMemory(false) - overhead;

        for (int iteration = 0; iteration < ITERATIONS; iteration++)
        {
            buffer.Clear();

            int maxCharsForIteration = 0, tries = 0;
            do
            {
                maxCharsForIteration = random.Next(0, MAX_CHARS);

            } while ((tries++ < 10) && bytesPerChar.ContainsKey(maxCharsForIteration));

            if (bytesPerChar.ContainsKey(maxCharsForIteration))
            {
                Console.WriteLine($"Terminating at iteration {iteration} due to lack of new chars/iteration generation.");
                break;
            }

            if (maxCharsForIteration != 0)
            {
                int l = random.Next(1, 16);
                StringBuilder field = new StringBuilder();
                for (int j = 0; j < l; j++)
                {
                    int c = random.Next(0, 25);
                    field.Append(('A' + c));
                }
                buffer.Append(field.ToString());

                long final = GC.GetTotalMemory(false);
                long memoryUsed = final - initial;

                bytesPerChar.Add(maxCharsForIteration, memoryUsed);
            }
        }

        Console.WriteLine($"Iterations: === {ITERATIONS:N0} ===");

        double maxMem = bytesPerChar.Max(b => b.Value);
        int maxMemChars = bytesPerChar.Where(b => (b.Value == maxMem)).Select(b => b.Key).First();
        
        int maxChars = bytesPerChar.Max(b => b.Key);
        double maxCharsMem = bytesPerChar.Where(b => (b.Key == maxChars)).Select(b => b.Value).First();

        double minMem = bytesPerChar.Min(b => b.Value);
        int minMemChars = bytesPerChar.Where(b => (b.Value == minMem)).Select(b => b.Key).First();

        int minChars = bytesPerChar.Min(b => b.Key);
        double minCharsMem = bytesPerChar.Where(b => (b.Key == minChars)).Select(b => b.Value).First();

        Console.WriteLine($"Minimum chars written: {minChars:N0} chars @ {minCharsMem/1024.0:F2} bytes");
        Console.WriteLine($"Maximum chars written: {maxChars:N0} chars @ {maxCharsMem/1024.0:F2} bytes");
        Console.WriteLine($"Minimum memory used: {minMem/1024.0:F2} bytes @ {minMemChars:N0} chars");
        Console.WriteLine($"Maximum memory used: {maxMem/1024.0:F2} bytes @ {maxMemChars:N0} chars");

    }

}
