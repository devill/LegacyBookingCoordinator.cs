using LegacyBookingCoordinator;

namespace LegacyBookingCoordinator.Tests
{
    public class BookingCoordinatorTests
    {
        [Fact]
        public void BookFlight_ShouldCreateBookingSuccessfully()
        {
            // Arrange
            var coordinator = new BookingCoordinator();
            var passengerName = "John Doe";
            var flightNumber = "AA123";
            var departureDate = DateTime.Now.AddDays(30);
            var passengerCount = 2;
            var airlineCode = "AA";
            var specialRequests = "meal,wheelchair";
            
            coordinator.BookFlight(passengerName, flightNumber, departureDate, 
                passengerCount, airlineCode, specialRequests);
        }

    }
}