// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using ParallelMatrixMultiplication;

// Write dotnet run -- Matrix1File Matrix2File FileWithResult
if (string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]) || args.Length < 2)
{
    Console.WriteLine("Error when passing arguments");
    return -1;
}

var matrix1Path = args[0];
var matrix2Path = args[1];

try
{
    var matrix1 = MatrixTools.ReadMatrixFromFile(matrix1Path);
    var matrix2 = MatrixTools.ReadMatrixFromFile(matrix2Path);

    var matrix = MatrixTools.MultiplyMatrix(matrix1, matrix2);

    MatrixTools.WriteMatrixInFile("result.txt", matrix);
}
catch (Exception ex) when (ex is FileNotFoundException
                               or FormatException
                               or ArgumentException)
{
    Console.WriteLine(ex);
    return -1;
}

return 0;
