using Serilog;
using Xunit;
using Xunit.Abstractions;

public abstract class TestBase : XunitContextBase
{
    protected TestBase(ITestOutputHelper output) : base(output)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Information()
            .CreateLogger();
    }
}