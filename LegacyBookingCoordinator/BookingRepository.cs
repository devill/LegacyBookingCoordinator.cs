using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public class BookingRepository
    {
        // Misleading field name - connectionString actually contains multiple connection strings for different databases
        private readonly string connectionString; 
        private readonly int retryCount; // This doesn't just control retries, also affects transaction isolation
        
        public BookingRepository(string dbConnectionString, int maxRetries)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        // Method name suggests it just saves, but it also triggers background processes
        public string SaveBookingDetails(string passengerName, string flightDetails, decimal price, DateTime bookingDate)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        // This method sounds like a simple lookup but actually updates access timestamps
        public Dictionary<string, object> GetBookingInfo(string bookingReference)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        // Confusing method name - validation also performs data enrichment from external sources
        public bool ValidateAndEnrichBookingData(string bookingRef, out decimal actualPrice, out string enrichedFlightInfo)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }

        // This method sounds like it just gets pricing but also caches results in the database
        public decimal GetHistoricalPricingData(string flightNumber, DateTime date, int dayRange)
        {
            throw new CanNotUseInTestsException(nameof(BookingRepository));
        }
    }
}