// <copyright file="MatrixTools.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace ParallelMatrixMultiplication;

/// <summary>
/// tools to work with matrix.
/// </summary>
public static class MatrixTools
{
    /// <summary>
    /// to read matrix from file.
    /// </summary>
    /// <param name="filePath">path to the input file.</param>
    /// <returns>matrix from input file.</returns>
    public static int[,] ReadMatrixFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found");
        }

        var lines = File.ReadAllLines(filePath)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        if (lines.Length == 0)
        {
            throw new FormatException("File is empty");
        }

        var rowsLength = lines.Length;
        var columnsLength = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        for (var i = 1; i < rowsLength; i++)
        {
            var currentColumnLength = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (currentColumnLength != columnsLength)
            {
                throw new FormatException($"row {i} has the wrong number of elements");
            }
        }

        var matrix = new int[rowsLength, columnsLength];

        for (var i = 0; i < rowsLength; i++)
        {
            var elements = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (var j = 0; j < columnsLength; j++)
            {
                if (!int.TryParse(elements[j], out var element))
                {
                    throw new FormatException($"element [{i}, {j}] has wrong format");
                }

                matrix[i, j] = element;
            }
        }

        return matrix;
    }

    /// <summary>
    /// to multiply matrix.
    /// </summary>
    /// <param name="matrix1">first matrix.</param>
    /// <param name="matrix2">second matrix.</param>
    /// <returns>result matrix of multiplication.</returns>
    public static int[,] MultiplyMatrix(int[,] matrix1, int[,] matrix2)
    {
        var rows1 = matrix1.GetLength(0);
        var cols1 = matrix1.GetLength(1);
        var rows2 = matrix2.GetLength(0);
        var cols2 = matrix2.GetLength(1);

        if (cols1 != rows2)
        {
            throw new FormatException("It is impossible to multiply matrices of such sizes");
        }

        var resultMatrix = new int[rows1, cols2];

        for (var row = 0; row < rows1; row++)
        {
            for (var col = 0; col < cols2; col++)
            {
                var sum = 0;

                for (var k = 0; k < cols1; k++)
                {
                    sum += matrix1[row, k] * matrix2[k, col];
                }

                resultMatrix[row, col] = sum;
            }
        }

        return resultMatrix;
    }

    /// <summary>
    /// to parallel multiply matrix.
    /// </summary>
    /// <param name="matrix1">first matrix.</param>
    /// <param name="matrix2">second matrix.</param>
    /// <returns>result matrix of multiplication.</returns>
    public static int[,] ParallelMultiplyMatrix(int[,] matrix1, int[,] matrix2)
    {
        var rows1 = matrix1.GetLength(0);
        var cols1 = matrix1.GetLength(1);
        var rows2 = matrix2.GetLength(0);
        var cols2 = matrix2.GetLength(1);

        if (cols1 != rows2)
        {
            throw new FormatException("It is impossible to multiply matrices of such sizes");
        }

        var resultMatrix = new int[rows1, cols2];
        var threads = new Thread[rows1];

        for (var row = 0; row < rows1; row++)
        {
            var localRow = row;

            threads[row] = new Thread(() =>
            {
                for (var col = 0; col < cols2; col++)
                {
                    var sum = 0;

                    for (var k = 0; k < cols1; k++)
                    {
                        sum += matrix1[localRow, k] * matrix2[k, col];
                    }

                    resultMatrix[localRow, col] = sum;
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return resultMatrix;
    }

    /// <summary>
    /// to write matrix in file.
    /// </summary>
    /// <param name="filePath">file to write matrix.</param>
    /// <param name="matrix">input matrix.</param>
    public static void WriteMatrixToFile(string filePath, int[,] matrix)
    {
        using var writer = new StreamWriter(filePath);
        for (var row = 0; row < matrix.GetLength(0); row++)
        {
            for (var col = 0; col < matrix.GetLength(1); col++)
            {
                writer.Write($"{matrix[row, col],4}");
            }

            writer.Write("\n");
        }
    }
}