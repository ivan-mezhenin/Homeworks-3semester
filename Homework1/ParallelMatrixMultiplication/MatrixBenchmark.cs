// <copyright file="MatrixBenchmark.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication;

using System.Diagnostics;

/// <summary>
/// matrix multiplication benchmark.
/// </summary>
public class MatrixBenchmark
{
    /// <summary>
    /// To run benchmark.
    /// </summary>
    public static void RunBenchmark()
    {
        const int runs = 10;
        const int startSize = 10;
        const int endSize = 490;
        const int step = 20;
        var random = new Random(42);

        Console.WriteLine("=== Бенчмарк умножения матриц ===");
        Console.WriteLine($"Измерений на тест: {runs}");
        Console.WriteLine($"Размеры: от {startSize}×{startSize}×{startSize} до {endSize}×{endSize}×{endSize} с шагом {step}");
        Console.WriteLine();

        Console.WriteLine("Размеры (A×B×C) | Среднее ± Стандартное отклонение (мс) | Ускорение");
        Console.WriteLine("                | Последовательная | Параллельная       |");
        Console.WriteLine(new string('-', 90));

        for (var size = startSize; size <= endSize; size += step)
        {
            var matrixA = GenerateRandomMatrix(size, size, random);
            var matrixB = GenerateRandomMatrix(size, size, random);

            var seqTimes = new List<double>();
            var parTimes = new List<double>();

            for (var i = 0; i < runs; i++)
            {
                var sw = Stopwatch.StartNew();
                MatrixTools.MultiplyMatrix(matrixA, matrixB);
                sw.Stop();
                seqTimes.Add(sw.Elapsed.TotalMilliseconds);

                sw.Restart();
                MatrixTools.ParallelMultiplyMatrix(matrixA, matrixB);
                sw.Stop();
                parTimes.Add(sw.Elapsed.TotalMilliseconds);
            }

            var (seqMean, seqStd) = CalculateStatistics(seqTimes);
            var (parMean, parStd) = CalculateStatistics(parTimes);
            var speedup = seqMean / parMean;

            Console.WriteLine($"{size}×{size}×{size,-8} | " +
                            $"{seqMean,6:F1} ± {seqStd,5:F1} | " +
                            $"{parMean,6:F1} ± {parStd,5:F1} | " +
                            $"{speedup:F2}x");
        }
    }

    private static (double Mean, double StdDev) CalculateStatistics(List<double> values)
    {
        var mean = values.Average();
        var sumSquaredDeviations = 0.0;
        foreach (var value in values)
        {
            var deviation = value - mean;
            sumSquaredDeviations += deviation * deviation;
        }

        var variance = sumSquaredDeviations / values.Count;
        var stdDev = Math.Sqrt(variance);

        return (mean, stdDev);
    }

    private static int[,] GenerateRandomMatrix(int rows, int cols, Random random)
    {
        var matrix = new int[rows, cols];
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                matrix[i, j] = random.Next(-100, 101);
            }
        }

        return matrix;
    }
}