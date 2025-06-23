I'd like to design a new code kata that demonstrates how people can inject dependencies into a particularly awful legacy system using the GlobalObjectDispatcher (a.k.a. GOD) creation pattern. (Yes, that's is a very creatively chosen name for the pattern.) README.md contains the description of the kate for workshop participants.

# How the pattern works:

We create a generic (or template) or in the case of dynamically typed languages a singleton object - the GlobalObjectDispatcher - that will act as a fancy `new` keyword, that allows us to override what is being created. The exact implementation of this might differ for each language, but looks something like this:
```
class GlobalObjectDispatcher {
    public static GlobalObjectDispatcher Instance();
    public T Create<T>(...);
    public SetOne<T>(T obj);
    public SetN<T>(T obj, int n);
    public SetAlways<T>(T obj);
    public Clear<T>();
}
```

Create takes arguments dynamically, and passes them on directly to the constructor invoked by the new keyword.

Now teams can use this to inject dependencies with minimal change:

Original line:
```
obj = new ProblematicClass(necessary, arguments);
```

Changes to:
```
obj = GlobalObjectDispatcher.Instance().Create<ProblematicClass>(necessary, arguments);
```

Or if they can pass in an instance of `god` they can avoid having to rely on the singleton instance of it:
```
obj = god.Create<ProblematicClass>(necessary, arguments);
```

# The problem this solves:

Now teams can use the GlobalObjectDispatcher to override the creation of classes with their own test doubles, all without having to introduce tricky parameter passing gymnastics inside untested code. This is particularly useful in code bases that are themselves composed of several weirdly interconnected classes that mutate states and rely on outside collaborators that are hard or downright impossible to create inside of tests.

# The task:

I'd like you to implement a truly horrible code base that people can use to practice writing tests using the pattern described above in C#. Make the code hard to read (but well-intentioned, so let's put in some misleading comments and variable names too) and rely on mutation to calculate fields that are then passed into constructors.

#### Classes that should not be used in tests
These are implemented to throw `CanNotUseInTests` when they are instantiated or one of their functions are called:
- `FlightAvailabilityService`
- `PartnerNotifier`
- `AuditLogger`
- `BookingRepository`

#### Production code
The main classes to be implemented by you are `BookingCoordinator` and `PricingEngine`. Make sure that PricingEnginer receives values in its constructor that come from `PricingEngine` and `BookingRepository`. Make sure that some of the pricing calculation is awkwardly leaked into the `BookingCoordinator`.

#### Supporting test code
Please also include an implementation of `GlobalObjectDispatcher` and a single test that that calls `BookingCoordinator.BookFlight()` but fails with the `CanNotUseInTests` exception.