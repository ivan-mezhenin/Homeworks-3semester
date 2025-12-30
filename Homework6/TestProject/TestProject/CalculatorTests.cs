using MyNUnit.Core.Attributes;

namespace TestProject;

public class CalculatorTests
{
    [Test]
    public void Add_TwoNumbers_ReturnsSum()
    {
        var calculator = new Calculator();
        var result = calculator.Add(5, 3);
        
        if (result != 8)
        {
            throw new Exception($"Expected 8, but got {result}");
        }
        
        Console.WriteLine("Add test passed");
    }

    [Test]
    public void Divide_ByZero_ThrowsException()
    {
        var calculator = new Calculator();
        
        try
        {
            calculator.Divide(10, 0);
            throw new Exception("Expected DivideByZeroException");
        }
        catch (DivideByZeroException)
        {
            // Expected exception
            Console.WriteLine("Divide by zero test passed");
        }
    }

    [Test]
    public void Multiply_TwoNumbers_ReturnsProduct()
    {
        var calculator = new Calculator();
        var result = calculator.Multiply(4, 7);
        
        if (result != 28)
        {
            throw new Exception($"Expected 28, but got {result}");
        }
        
        Console.WriteLine("Multiply test passed");
    }
}

// Простой класс для тестирования
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Multiply(int a, int b) => a * b;
    public int Divide(int a, int b) => a / b;
}