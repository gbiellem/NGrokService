using Serilog;
using Xunit;
using Xunit.Abstractions;

public class SampleTest : TestBase
{
    [Fact]
    public void HelloWorld()
    {
        Log.Information("Hello World");
    }

    public SampleTest(ITestOutputHelper output) : base(output)
    {
    }
}