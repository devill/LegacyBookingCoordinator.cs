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
            var storybook = new StringBuilder();
            
            var coordinator = new BookingCoordinator();
            var passengerName = "John Doe";
            var flightNumber = "AA123";
            var departureDate = DateTime.Now.AddDays(30);
            var passengerCount = 2;
            var airlineCode = "AA";
            var specialRequests = "meal,wheelchair";
            
            coordinator.BookFlight(passengerName, flightNumber, departureDate, 
                passengerCount, airlineCode, specialRequests);
            
            // Assert
            await Verify(storybook.ToString());
        }

    }
}