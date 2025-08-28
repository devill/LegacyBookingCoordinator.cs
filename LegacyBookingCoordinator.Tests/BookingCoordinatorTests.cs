using System.Text;
using LegacyBookingCoordinator;
using SpecRec;

namespace LegacyBookingCoordinator.Tests
{
    public class BookingCoordinatorTests
    {
        [Theory]
        [SpecRecLogs]
        public async Task BookFlight_ShouldCreateBookingSuccessfully(Context context)
        {
            // Arrange
            var passengerName = "John Doe";
            var flightNumber = "AA123";
            var departureDate = new DateTime(2025, 07, 03, 12, 42, 11);
            var passengerCount = 2;
            var airlineCode = "AA";
            var specialRequests = "meal,wheelchair";

            await context.Verify(async () => {
                var coordinator = new BookingCoordinator();
                return coordinator.BookFlight(passengerName, flightNumber, departureDate,
                    passengerCount, airlineCode, specialRequests).ToString();
            });
        }
    }
}