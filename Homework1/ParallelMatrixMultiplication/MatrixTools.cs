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
            throw new ArgumentException("File is empty");
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
}