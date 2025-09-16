
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

## 🥉 Bronze: Inject dependencies

Did you ever run into a codebase so awkward and full of hard to override dependencies that even the thought of writing a test is daunting? When the dreaded `new` keyword liters a codebase, writing tests after the fact is a nightmare. Luckily, the `ObjectFactory` can help you out.

### 🔧 Task

Write a test for `BookingCoordinator.BookFlight()` that:
* Uses stubs instead of the untestable classes
* Checks that it returns the booking reference produced by the `BookingRepository`
* All *without extensive changes to the production code*.

This is **impossible** without changing the code. With SpecRec's `ObjectFactory`, you can refactor the `new` calls to use `factory.Create<T>()` and inject test doubles.

### 🏭 Concept: ObjectFactory

The `ObjectFactory` acts as a drop-in replacement for the `new` keyword, allowing you to control object creation in tests.

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
var factory = ObjectFactory.Instance();
try
{
    factory.SetOne<IBookingRepository>(new BookingRepositoryStub());
    factory.SetOne<IFlightAvailabilityService>(new FlightAvailabilityServiceStub());
    // ... run your test
}
finally
{
    factory.ClearAll(); // Important for test isolation
}
```

#### Singleton instance

You can either inject an instance of the factory (harder, but better long term) or use the Singleton instance:
```
ObjectFactory.Instance().Create<YourClass>(constructor, arguments);
```

Or use the shorthand (requires `using static SpecRec.GlobalObjectFactory;`):
```
Create<YourClass>(constructor, arguments);
```

## 🥉🎖️ Bronze with Context: Simplify test setup

Managing the ObjectFactory manually with try/finally blocks gets tedious. The `Context` API provides a cleaner way to manage test setup and teardown.

### 🔧 Task

Convert your test to use `Context.Verify` which automatically handles ObjectFactory cleanup and provides a foundation for more advanced features.

### 📋 Concept: Context.Verify

When using SpecRec we use the `[Theory]` annotation with the `[SpecRecLogs]` data provider. The data provider will always include a `Context` as the first parameter. It acts as a wrapper for the most frequently used SpecRec features and provides the `Context.Verify` wrapper method that will record interactions and verify them against previously approved results.

Here is how you can call `factory.SetOne<>()` using the context:

```csharp
[Theory]
[SpecRecLogs]
public async Task BookFlight_ShouldCreateBookingSuccessfully(Context context)
{
    await context.Verify(async () => {
        context.SetOne<IBookingRepository>(new BookingRepositoryStub());
        // ... setup other dependencies

        var coordinator = new BookingCoordinator(bookingDate);
        return coordinator.BookFlight(/* parameters */).ToString();
    });
}
```

⚠️ **Important**: The `Context` automatically handles test isolation and cleanup, so no manual ObjectFactory clearing is required.

#### Constructor arguments

If you want to test constructor arguments make sure your test double implements `IConstructorCalledWith`. If it does the `ConstructorCalledWith` method will get called with the constructor parameters upon object creation.

```csharp
public void ConstructorCalledWith(ConstructorParameterInfo[] parameters)
```

Each `ConstructorParameterInfo` contains the parameter name and value, allowing for better test logging and verification.

## 📦 Prerequisites

This kata requires the **SpecRec** NuGet package (version 0.0.3 or later) which provides the enhanced ObjectFactory and related testing utilities. 

## 🥈 Silver: Test the interactions

Now that you can inject dependencies, you have another problem: how do you implement test doubles and end up with an easy-to-read test? Setting up multiple mocks can become very time-consuming, but with a `CallLogger` it's easy. 

### 🛠️ Task

Improve the test for `BookingCoordinator.BookFlight()` so that:
* It checks the booking was saved as expected.
* It checks a notification was sent to the correct place.
* It checks price calculation is correct.
* Verifies logging occurred.

Use the `Context` to wrap test doubles and create a storybook of calls. 

### ☎️ Concept: CallLogger / Context.Wrap

The Context can wrap test doubles to automatically log all method calls:

```csharp
await context.Verify(async () => {
    context.SetOne(context.Wrap<IEmailService>(new EmailServiceStub(), "📧"));
    
    // All method calls will be automatically logged
    service.SendEmail("user@example.com", "Welcome!");
});
```

## 🏅 Gold: Eliminate stub implementations

Writing stub implementations for every dependency gets tedious fast. But did you notice that the call logs actually contain the return values? What if the return values were parsed from the latest verified call log? That is what the `Parrot` test double does for you.

### 🎯 Task

Replace your wrapped stubs with `Parrot` test doubles that automatically replay method interactions from verified files. This eliminates the need to write and maintain stub implementations entirely.

### 🦜 Concept: Context.Parrot

The Context can create Parrot test doubles that replay method interactions from verified files:

```csharp
await context.Verify(async () => {
    context.SetOne(context.Parrot<IBookingRepository>("💾"));
    context.SetOne(context.Parrot<IFlightAvailabilityService>("✈️"));
    context.SetOne(context.Parrot<IPartnerNotifier>("📣"));
    context.SetOne(context.Parrot<IAuditLogger>("🪵"));
    context.SetOne(context.Parrot<Random>("🎲"));

    // The Parrot will automatically replay interactions from the verified file
    var coordinator = new BookingCoordinator(bookingDate);
    return coordinator.BookFlight(/* parameters */).ToString();
});
```

Normally the first run throws a `ParrotMissingReturnValueException`. Fill in return values in the `.received.txt` file, rename to `.verified.txt`. Repeat until green.

However, since you already have a verified call log with return values, the test should pass right away.

## 🏅🎖️ Gold with Substitute: Fluent API

Manually creating and registering Parrots gets repetitive. The `Context.Substitute()` fluent API provides a cleaner way to set up multiple test doubles.

### 🔗 Concept: Context.Substitute

Using `Context.Substitute()` you can let SpecRec handle Parrot creation for you:

```csharp
await context
    .Substitute<IBookingRepository>("💾")
    .Substitute<IFlightAvailabilityService>("✈️")
    .Substitute<IPartnerNotifier>("📣")
    .Substitute<IAuditLogger>("🪵")
    .Substitute<Random>("🎲")
    .Verify(async () =>
        new BookingCoordinator(bookingDate).BookFlight(
            passengerName, flightNumber, departureDate,
            passengerCount, airlineCode, specialRequests
        ).ToString());
```

This syntax has a cool side effect: instead of using the same Parrot over and over, `factory.Create` will create a new one every time with separate IDs. This syntax gives you the ability to keep track of multiple different objects of the same type. 

## 💎 Ruby: Comprehensive scenario testing

By now we have one test, but oh no... we need more. 😩 Don't worry, it won't take forever! Now that we read values from the verified call logs, we can have multiple call logs testing different scenarios. 

### 🎯 Task

Transform your single test into a comprehensive test suite that covers multiple booking scenarios using the `SpecRecLogsAttribute`. Instead of writing multiple similar tests, you'll define the system under test once and create verified files for each scenario.

### 🏆 Concept: SpecRecLogsAttribute

Use `[SpecRecLogs]` to turn verified files into test cases:

```csharp
[Theory]
[SpecRecLogs]
public async Task BookFlight_MultipleScenarios(Context context)
{
    // Same test setup for all scenarios
    // Each .verified.txt file becomes a separate test case
}
```

Create files like `BookFlight_MultipleScenarios.StandardBooking.verified.txt` and `BookFlight_MultipleScenarios.NoAvailability.verified.txt`. Each file runs as its own test automatically.

If you need to inject test values, you can also include those in the call log. 

First, you need to add the parameters to your test:

```csharp
[Theory]
[SpecRecLogs]
public async Task BookFlight_MultipleScenarios(Context context, string name = 'John Doe', int age = 42)
{

}
```

And then specify values at the start of the call log:

```
📋 <Test Inputs>
  🔸 name: "Jane Smith"
  🔸 age: 23
```

If you specified default values, you can omit any of the input values.