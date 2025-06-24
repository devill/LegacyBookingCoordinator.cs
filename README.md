
# Code Kata: *Legacy Flight Booking System Testing*

# 🎯 Objective:

Introduce testability into an entangled legacy system responsible for managing flight bookings, pricing, and external integrations.

## 💼 Business Context:

Your company maintains a **legacy monolithic flight booking system**, originally written in a hurry for a client with ever-changing airline partnership rules. The original developers are long gone, and now you’re tasked with adding **unit tests** and eventually decoupling and refactoring the system.

**Unfortunately:**

* Classes instantiate each other *directly* with `new`.
* Side effects (logging, emailing, pricing calls) happen all over the place.
* There is **no clean dependency injection**, no container, no interfaces.
* Changes require fear-driven development, unless something changes…

### What the legacy code does

The booking system coordinates:

1. **FlightAvailabilityService**: Queries seat availability.
2. **PricingEngine**: Applies dynamic pricing rules based on time, demand, and airline quirks.
3. **PartnerNotifier**: Notifies airlines about confirmed bookings with airline-specific formatting.
4. **AuditLogger**: Writes booking activity logs to disk.
5. **BookingRepository**: Saves booking data to a proprietary database (only available in production).
6. **BookingCoordinator**: The main orchestrator that coordinates all the services.

# 🏆 Challenges

## 🥉 Inject dependencies 

### 🔧 Task

Use the `ObjectFactory` to write a test for `BookingCoordinator.BookFlight()` that:
* Uses stubs instead of the untestable classes
* Checks that it returns the booking reference produced by the `BookingRepository`
* All *without extensive changes to the production code*.

This is **impossible** without changing the code. With `ObjectFactory`, you can refactor the `new` calls to use `factory.Create<T>()` and inject test doubles that record behavior.

### 🏭 Concept: ObjectFactory

Did you ever run into a codebase so awkward and full of hard to override dependencies that even the thought of writing a test is daunting?

The `ObjectFactory` is meant to solve this problem. It is a simple class that acts as a drop-in replacement for the `new` keyword, allowing you to control object creation in tests.

Instead of:
```csharp
var logger = new AuditLogger(logDirectory, verboseMode);
```

Use:
```csharp
var logger = factory.Create<AuditLogger>(logDirectory, verboseMode);
```

Or for classes with interfaces:
```csharp
var logger = factory.Create<IAuditLogger, AuditLogger>(logDirectory, verboseMode);
```

In tests, you can override what gets created:
```csharp
// For concrete types
factory.SetAlways<AuditLogger>(new FakeAuditLogger());     
// For interface types
factory.SetAlways<IAuditLogger>(new FakeAuditLogger());
// Return this fake once, then normal creation
factory.SetOne<PricingEngine>(new FakePricingEngine());    
```

#### Constructor arguments

If you want to test constructor arguments make sure your test double implements `IConstructorCalledWith`. If it does the `ConstructorCalledWith` method will get called with the constructor arguments upon object creation.

#### Singleton instance

You can either inject an instance of the factory (harder, but better long term) or use the Singleton instance:
```
ObjectFactory.Instance().Create<YourClass>(constructor, arguments);
```

Or use the shorthand:
```
Create<YourClass>(constructor, arguments);
```

⚠️ **Important**: All tests that rely on the singleton instance of `ObjectFactory` should call `ObjectFactory.Instance().ClearAll()` to make sure tests remain independent.

⚠️ **Word of caution**: While the singleton instance can be convenient, it can also cause trouble when overused. Only use it temporarily when the alternative is passing in the factory through several layers of indirection. 

## 🥈 Test the interactions

### 🛠️ Task

Improve the test for `BookingCoordinator.BookFlight()` so that:
* It checks the booking was saved as expected.
* It checks a notification was sent to the correct place.
* It checks price calculation is correct.
* Verifies logging occurred.

Writing a test like that with conventional methods is tedious. Use the `CallLogger` to create a storybook of calls.

### ☎️ Concept: CallLogger

Now that you can inject dependencies you have another problem: how do you implement test doubles and end up with an easy-to-read test?

The CallLogger uses automatic wrapping to log all method calls:

```csharp
var storybook = new StringBuilder();
var callLogger = new CallLogger(storybook);

// Wrap any object to automatically log its method calls
var service = callLogger.Wrap<IEmailService>(new EmailServiceStub(), "📧");

// All method calls will be automatically logged
service.SendEmail("user@example.com", "Welcome!");
```

#### Controlling What Gets Logged

Use the static `CallLogFormatterContext` methods inside your stub methods to control logging:

```csharp
using static LegacyTestingTools.CallLogFormatterContext;

public class EmailServiceStub : IEmailService, IConstructorCalledWith
{
    public void SendEmail(string recipient, string message)
    {
        // Add contextual information
        AddNote("Email sent to external SMTP server");
        
        // Hide sensitive information
        if (recipient.Contains("admin"))
            IgnoreAllArguments();
    }
    
    public void InternalCleanup()
    {
        // Skip logging internal methods
        IgnoreCall();
    }
    
    public void ConstructorCalledWith(params object[] args)
    {
        // Provide meaningful parameter names
        SetConstructorArgumentNames("smtpServer", "port");
    }
}
```

#### Available CallLogFormatterContext Methods

- `AddNote(string note)` - Add explanatory notes to method calls
- `IgnoreCall()` - Skip logging the current method call entirely
- `IgnoreArgument(int index)` - Hide specific arguments by index
- `IgnoreAllArguments()` - Hide all arguments for the current method
- `IgnoreReturnValue()` - Hide the return value
- `SetConstructorArgumentNames(params string[] names)` - Provide meaningful parameter names for constructor calls

## 🥇 Cover all paths

### 🚜 Task

Now that you have one test, you are on your way. However, writing a large number of similar tests can become tedious. Use a data provider and default values to build a comprehensive test suite for `BookingCoordinator`.

### 📀 Data Provider (xUnit Theory)

Instead of writing multiple similar tests, use xUnit's `[Theory]` and `[InlineData]` attributes to test multiple scenarios:

```csharp
[Theory]
[InlineData("FLIGHT001", 1, "Standard")]
[InlineData("FLIGHT002", 2, "Premium")]
[InlineData("FLIGHT003", 4, "Group")]
public async Task ProcessBooking_ShouldHandleDifferentScenarios(
    string flightCode, 
    int passengerCount, 
    string expectedCategory)
{
    // Arrange
    var storybook = new StringBuilder();
    var callLogger = new CallLogger(storybook);
    var factory = ObjectFactory.Instance();
    
    try
    { 
        // Setup your test doubles using the factory
        factory.SetOne(callLogger.Wrap<IService1>(new Service1Stub(), "🔧"));
        factory.SetOne(callLogger.Wrap<IService2>(new Service2Stub(), "⚙️"));

        // Act
        var result = systemUnderTest.ProcessBooking(flightCode, passengerCount);

        // Assert
        var logOutput = storybook.ToString();
        Assert.Contains(expectedCategory, logOutput);
        Assert.NotNull(result);
    }
    finally
    {
        factory.ClearAll();
    }
}
```

#### Advanced Data Providers

For complex test scenarios, use `[MemberData]` with static methods:

```csharp
public static IEnumerable<object[]> TestScenarios()
{
    yield return new object[] 
    { 
        "SCENARIO_A", 
        new DateTime(2025, 06, 15), // Weekend
        100.00m, // Base amount
        "PREMIUM" // Expected result
    };
    
    yield return new object[] 
    { 
        "SCENARIO_B", 
        new DateTime(2025, 06, 16), // Weekday
        50.00m, // Lower amount
        "STANDARD" // Expected result
    };
}

[Theory]
[MemberData(nameof(TestScenarios))]
public async Task ProcessRequest_ComprehensiveScenarios(
    string scenarioId, 
    DateTime date, 
    decimal amount, 
    string expectedResult)
{
    // Test implementation using the provided data
    // Verify behavior varies correctly across scenarios
}
```

#### Benefits of Data Providers

- **Comprehensive Coverage**: Test edge cases, boundary conditions, and multiple scenarios
- **Maintainable**: Add new test cases by adding data, not duplicating code
- **Clear Documentation**: Test data serves as living documentation of system behavior
- **Regression Protection**: Easily add test cases when bugs are found

#### Best Practices

1. **Start Simple**: Use `[InlineData]` for basic scenarios
2. **Use `[MemberData]` for Complex Cases**: When you need objects, dates, or computed values
3. **Name Meaningfully**: Make test data self-documenting
4. **Group Related Scenarios**: Keep related test cases in the same theory
5. **Verify Multiple Aspects**: Check outputs, side effects, and logs in comprehensive tests