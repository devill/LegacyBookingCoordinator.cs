
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

The CallLogger takes a StringBuilder and appends a formatted log for your call. This way you just need to verify the collected calls at the end of the test.

Example:
```csharp
class TestDoubleFake : ISomeService, IConstructorCalledWith
{
    private readonly CallLogger _callLogger;

    public TestDoubleFake(StringBuilder storybook)
    {
        _callLogger = new CallLogger(storybook, "📝");
    }
    
    public string DoSomething(int count, string name)
    {
        var result = "success";
        _callLogger
            .withArgument(count, "count")
            .withArgument(name, "name")
            .withReturn(result)
            .log();
        return result;
    }

    public void ConstructorCalledWith(params object[] args)
    {
        _callLogger
            .withArgument((string)args[0], "connectionString")
            .withArgument((int)args[1], "timeout")
            .log();
    }
}
```

## 🥇 Cover all paths

### 🚜 Task

Now that you have one test, you are on your way. However, writing a large number of similar tests can become tedious. Use a data provider and default values to build a comprehensive test suite for `BookingCoordinator`.

### 📀 Data Provider

**TODO**