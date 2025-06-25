using System.Text;
using LegacyBookingCoordinator;

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

            var storybook = new StringBuilder();

            var coordinator = new BookingCoordinator();
            coordinator.BookFlight(passengerName, flightNumber, departureDate,
                passengerCount, airlineCode, specialRequests);

            // Assert
            await Verify(storybook.ToString());
        }
    }
}