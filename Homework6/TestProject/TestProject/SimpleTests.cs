using MyNUnit.Core.Attributes;

namespace TestProject;

public class SimpleTests
{
    [Test]
    public void PassingTest()
    {
        Console.WriteLine("This test should pass");
        AssertTrue(true);
    }

    [Test]
    public void FailingTest()
    {
        Console.WriteLine("This test should fail");
        AssertTrue(false);
    }

    [Test(Ignore = true, IgnoreReason = "This test is ignored")]
    public void IgnoredTest()
    {
        Console.WriteLine("This test should be ignored");
    }

    [Test(Expected = typeof(DivideByZeroException))]
    public void ExpectedExceptionTest()
    {
        Console.WriteLine("This test expects an exception");
        var x = 0;
        var result = 10 / x; // Will throw DivideByZeroException
    }

    [Before]
    public void BeforeEachTest()
    {
        Console.WriteLine("Running before each test");
    }

    [After]
    public void AfterEachTest()
    {
        Console.WriteLine("Running after each test");
    }

    [BeforeClass]
    public static void BeforeAllTests()
    {
        Console.WriteLine("Running before all tests (static)");
    }

    [AfterClass]
    public static void AfterAllTests()
    {
        Console.WriteLine("Running after all tests (static)");
    }

    private static void AssertTrue(bool condition)
    {
        if (!condition)
        {
            throw new Exception("Assertion failed: expected true");
        }
    }
}