using System.Text;
using LegacyBookingCoordinator;
using SpecRec;

namespace LegacyBookingCoordinator.Tests
{
    public class BookingCoordinatorTests
    {
        [Fact]
        public async Task BookFlight_ShouldCreateBookingSuccessfully()
        {
            // Arrange
            var passengerName = "John Doe";
            var flightNumber = "AA123";
            var departureDate = new DateTime(2025, 07, 03, 12, 42, 11);
            var passengerCount = 2;
            var airlineCode = "AA";
            var specialRequests = "meal,wheelchair";
            
            var result = new BookingCoordinator().BookFlight(
                passengerName, flightNumber, departureDate,
                passengerCount, airlineCode, specialRequests
            );

            await Verify(result.ToString());
        }
    }
}