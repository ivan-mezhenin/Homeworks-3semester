// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using ParallelMatrixMultiplication;

var matrix1Path = args[0];
var matrix2Path = args[1];

var matrix1 = MatrixTools.ReadMatrixFromFile(matrix1Path);
var matrix2 = MatrixTools.ReadMatrixFromFile(matrix2Path);

var matrix = MatrixTools.MultiplyMatrix(matrix1, matrix2);

for (int i = 0; i < matrix.GetLength(0); i++)
{
    for (int j = 0; j < matrix.GetLength(1); j++)
    {
        Console.Write($"{matrix[i, j],4}");
    }

    Console.WriteLine();
}

