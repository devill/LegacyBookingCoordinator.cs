using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public class FlightAvailabilityService
    {
        // This variable name is misleading - it's not actually a config
        private readonly string airlineApiConfig;
        
        public FlightAvailabilityService(string connectionString)
        {
            throw new CanNotUseInTestsException(nameof(FlightAvailabilityService));
        }

        // Confusing method name - sounds like it checks availability but also returns seats
        public List<string> CheckAndGetAvailableSeatsForBooking(string flightNumber, DateTime departureDate, int passengerCount)
        {
            throw new CanNotUseInTestsException(nameof(FlightAvailabilityService));
        }

        // This method has side effects despite the name suggesting it's just a query
        public bool IsFlightFullyBooked(string flightNumber, DateTime departureDate)
        {
            throw new CanNotUseInTestsException(nameof(FlightAvailabilityService));
        }
    }
}