// <copyright file="MatrixToolsTest.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

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
}