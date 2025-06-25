using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public class BookingRepository : IBookingRepository
    {
        private readonly string connectionString;
        private readonly int retryCount;

        public BookingRepository(string dbConnectionString, int maxRetries)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        public string SaveBookingDetails(string passengerName, string flightDetails, decimal price,
            DateTime bookingDate)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        public Dictionary<string, object> GetBookingInfo(string bookingReference)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        public bool ValidateAndEnrichBookingData(string bookingRef, out decimal actualPrice,
            out string enrichedFlightInfo)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        public decimal GetHistoricalPricingData(string flightNumber, DateTime date, int dayRange)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }
    }
}