using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public interface IBookingRepository
    {
        string SaveBookingDetails(string passengerName, string flightDetails, decimal price, DateTime bookingDate);
        Dictionary<string, object> GetBookingInfo(string bookingReference);
        bool ValidateAndEnrichBookingData(string bookingRef, out decimal actualPrice, out string enrichedFlightInfo);
        decimal GetHistoricalPricingData(string flightNumber, DateTime date, int dayRange);
    }
}