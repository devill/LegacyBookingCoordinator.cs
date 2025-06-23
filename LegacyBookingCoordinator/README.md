
# 🧩 Code Kata: *Legacy Flight Booking System Refactor*

### 🎯 Objective:

Use the `GlobalObjectDispatcher` pattern to introduce testability into an entangled legacy system responsible for managing flight bookings, pricing, and external integrations.

#### What is the GlobalObjectDispatcher?

**TODO:** this section needs to be written.

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
2. **PricingEngine**: Applies dynamic pricing rules based on time, demand, and random airline quirks. Mostly takes care of price calculation apart from the occasional REST API call, and some parts of the calculation that leaked out into the BookingCoordinator.
3. **PartnerNotifier**: Notifies airlines about confirmed bookings. Needs to be called with different set of arguments depending on the airline. The logic for this is implemented as a contrived set of if else and switch statements with some duplication here and there.
4. **AuditLogger**: Writes booking activity logs to disk.
5. **BookingRepository**: Saves booking data to a proprietary database for which the licence is so expensive, we only have it in production.
6. **BookingCoordinator**: The class that ties it all together, has zero tests, and lots of mutation, and object creation based on fields calculated inside the function before instantiation.

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