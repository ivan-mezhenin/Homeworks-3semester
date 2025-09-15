// <copyright file="MatrixToolsTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace ParallelMatrixMultiplication.Tests;

/// <summary>
/// test for matrix tools.
/// </summary>
public class MatrixToolsTest
{
    /// <summary>
    /// test for correct reading matrix from file.
    /// </summary>
    [Test]
    public void MatrixTools_ReadMatrixFromFile()
    {
        var testFile = Path.Combine(AppContext.BaseDirectory, "TestFiles", "ReadMatrixFromFile.txt");
        var inputMatrix = MatrixTools.ReadMatrixFromFile(testFile);
        var expectedMatrix = new int[,]
        {
            { 1, 2, 34, 4, 5, 7 },
            { 23, 43, 43, 5, 5, 54 },
            { 1, 2, 4, 5, 6, 7 },
        };

        Assert.That(inputMatrix, Is.EqualTo(expectedMatrix));
    }

    /// <summary>
    /// test for throwing file not found exception while reading non-existing file.
    /// </summary>
    [Test]
    public void MatrixTools_WriteMatrixToFile_ShouldThrowFileNotFoundException()
    => Assert.Throws<FileNotFoundException>(() => MatrixTools.ReadMatrixFromFile("NonExistentFile.txt"));

    /// <summary>
    /// test for throwing format exception while reading incorrect matrix from file.
    /// </summary>
    [Test]
    public void MatrixTools_WriteMatrixToFile_IncorrectMatrix_ShouldThrowFormatException()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "IncorrectFormatMatrix.txt");
        Assert.Throws<FormatException>(() => MatrixTools.ReadMatrixFromFile(filePath));
    }

    /// <summary>
    /// test for parallel multiply small matrices.
    /// </summary>
    [Test]
    public void MatrixTools_ParallelMultiplyMatrix_SmallMatrices()
    {
        var matrix1File = Path.Combine(AppContext.BaseDirectory, "TestFiles", "Matrix1.txt");
        var matrix2File = Path.Combine(AppContext.BaseDirectory, "TestFiles", "Matrix2.txt");

        var matrix1 = MatrixTools.ReadMatrixFromFile(matrix1File);
        var matrix2 = MatrixTools.ReadMatrixFromFile(matrix2File);
        var expected = new int[,]
        {
            { 541, 347, 837, 1849 },
            { 985, 174, 531, 1247 },
            { 2981, 137, 277, 714 },
        };

        Assert.That(MatrixTools.ParallelMultiplyMatrix(matrix1, matrix2), Is.EqualTo(expected));
    }

    /// <summary>
    /// test for throwing format exception while multiplication matrices with incorrect sizes.
    /// </summary>
    [Test]
    public void MatrixTools_ParallelMultiplyMatrix_IncorrectSize_ShouldThrowFormatException()
    {
        var matrix1 = new int[,]
        {
            { 1, 2, 3, 4 },
            { 5, 6, 7, 8 },
        };

        var matrix2 = new int[,]
        {
            { 5, 8, 9 },
            { 4, 14, 2 },
            { 2, 3, 56 },
        };

        Assert.Throws<FormatException>(() => MatrixTools.ParallelMultiplyMatrix(matrix1, matrix2));
    }
}