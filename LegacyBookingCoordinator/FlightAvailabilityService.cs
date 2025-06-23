using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public class FlightAvailabilityService : IFlightAvailabilityService
    {
        private readonly string airlineApiConfig;
        
        public FlightAvailabilityService(string connectionString)
        {
            throw new CanNotUseInTestsException(nameof(FlightAvailabilityService));
        }

        public List<string> CheckAndGetAvailableSeatsForBooking(string flightNumber, DateTime departureDate, int passengerCount)
        {
            throw new CanNotUseInTestsException(nameof(FlightAvailabilityService));
        }

        public bool IsFlightFullyBooked(string flightNumber, DateTime departureDate)
        {
            throw new CanNotUseInTestsException(nameof(FlightAvailabilityService));
        }
    }
}