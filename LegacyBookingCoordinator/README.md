
# Code Kata: *Legacy Flight Booking System Testing*

# 🎯 Objective:

Use the `GlobalObjectDispatcher` pattern to introduce testability into an entangled legacy system responsible for managing flight bookings, pricing, and external integrations.

## What is the GlobalObjectDispatcher?

Did you ever run into a codebase so awkward and full of hard to override dependencies that even the thought of writing a test is daunting? 

The `GlobalObjectDispatcher` is meant to solve this problem. It is a simple class that acts as a drop-in replacement for the `new` keyword, allowing you to control object creation in tests.

**Instead of:**
```csharp
var logger = new AuditLogger(logDirectory, verboseMode);
```

**Use:**
```csharp
var logger = god.Create<AuditLogger>(logDirectory, verboseMode);
```

Or for classes with interfaces:
```csharp
var logger = god.Create<IAuditLogger, AuditLogger>(logDirectory, verboseMode);
```

In tests, you can override what gets created:
```csharp
var god = GlobalObjectDispatcher.Instance();
god.SetAlways<AuditLogger>(new FakeAuditLogger());     // For concrete types
god.SetAlways<IAuditLogger>(new FakeAuditLogger());    // For interface types  
god.SetOne<PricingEngine>(new FakePricingEngine());    // Return this fake once, then normal creation
```

**Important**: All tests that set objects on the `GlobalObjectDispatcher` should call `GlobalObjectDispatcher.Instance().ClearAll()` to make sure tests remain independent. 

### Fair Warning

While this is an incredibly useful pattern to get your first tests running in a hairy codebase, **avoid over reliance** on it. Since it relies on global state it can become tedious to work with. 

Ideally once your code is under test, you can refactor to a state where injecting dependencies is done via the constructor. 

## 💼 Business Context:

Your company maintains a **legacy monolithic flight booking system**, originally written in a hurry for a client with ever-changing airline partnership rules. The original developers are long gone, and now you’re tasked with adding **unit tests** and eventually decoupling and refactoring the system.

**Unfortunately:**

* Classes instantiate each other *directly* with `new`.
* Side effects (logging, emailing, pricing calls) happen all over the place.
* There is **no clean dependency injection**, no container, no interfaces.
* Changes require fear-driven development, unless something changes…

## 🔧 What the legacy code does

The booking system coordinates:

1. **FlightAvailabilityService**: Queries seat availability.
2. **PricingEngine**: Applies dynamic pricing rules based on time, demand, and airline quirks.
3. **PartnerNotifier**: Notifies airlines about confirmed bookings with airline-specific formatting.
4. **AuditLogger**: Writes booking activity logs to disk.
5. **BookingRepository**: Saves booking data to a proprietary database (only available in production).
6. **BookingCoordinator**: The main orchestrator that coordinates all the services.

## 🧪 Test Challenge

Write a test for `BookingCoordinator.BookFlight()` that:

* Asserts a notification was sent.
* Asserts the correct price calculation.
* Verifies logging occurred.
* All *without touching real files or the console output*.

This is **impossible** without changing the code. But with `GlobalObjectDispatcher`, you can refactor the `new` calls to use `god.Create<T>()`, and inject test doubles that record behavior.

Then write a test using:

```csharp
god.SetAlways<AuditLogger>(new FakeAuditLogger());
```