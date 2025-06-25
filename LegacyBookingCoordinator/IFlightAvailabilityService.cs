using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public interface IFlightAvailabilityService
    {
        List<string> CheckAndGetAvailableSeatsForBooking(string flightNumber, DateTime departureDate,
            int passengerCount);

        bool IsFlightFullyBooked(string flightNumber, DateTime departureDate);
    }
}