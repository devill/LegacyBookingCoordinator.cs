using System.Text;
using VerifyXunit;
using Xunit;

namespace LegacyTestingTools.Tests
{
    public class CallLoggerTests
    {
        [Fact]
        public async Task Wrap_ShouldLogAllMethodCalls()
        {
            var storybook = new StringBuilder();
            var logger = new CallLogger(storybook);
            var mockService = new TestService();
            
            var wrappedService = logger.Wrap<ITestService>(mockService, "🧪");
            
            var result = wrappedService.Calculate(5, 10);
            wrappedService.ProcessData("test input");
            
            await Verify(storybook.ToString());
        }
        
        [Fact]
        public async Task Wrap_WithCallLogFormatter_ShouldRespectFormattingRules()
        {
            var storybook = new StringBuilder();
            var logger = new CallLogger(storybook);
            var mockService = new FormattedTestService();
            
            var wrappedService = logger.Wrap<ITestService>(mockService, "📝");
            
            wrappedService.Calculate(5, 10);
            wrappedService.ProcessData("secret");
            
            await Verify(storybook.ToString());
        }
        
        [Fact]
        public async Task Wrap_WithOutParameter_ShouldLogOutValues()
        {
            var storybook = new StringBuilder();
            var logger = new CallLogger(storybook);
            var mockService = new TestService();
            
            var wrappedService = logger.Wrap<ITestService>(mockService, "🔍");
            
            string output;
            var result = wrappedService.TryProcess("input", out output);
            
            await Verify(storybook.ToString());
        }
    }
    
    public interface ITestService
    {
        int Calculate(int a, int b);
        void ProcessData(string input);
        bool TryProcess(string input, out string output);
    }
    
    public class TestService : ITestService
    {
        public int Calculate(int a, int b) => a + b;
        
        public void ProcessData(string input) { }
        
        public bool TryProcess(string input, out string output)
        {
            output = $"processed_{input}";
            return true;
        }
    }
    
    public class FormattedTestService : ITestService
    {
        public int Calculate(int a, int b)
        {
            LegacyTestingTools.CallLogFormatterContext.IgnoreAllArguments();
            LegacyTestingTools.CallLogFormatterContext.AddNote("This calculation ignores all arguments in logs");
            return a + b;
        }
        
        public void ProcessData(string input) 
        { 
            LegacyTestingTools.CallLogFormatterContext.IgnoreCall();
        }
        
        public bool TryProcess(string input, out string output)
        {
            output = $"processed_{input}";
            return true;
        }
    }
}