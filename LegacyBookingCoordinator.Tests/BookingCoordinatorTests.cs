using System.CodeDom;
using System.Text;
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
            var bookingDate = new DateTime(2025, 03, 04, 14, 00, 56);

            await context
                .Substitute<IBookingRepository>("💾")
                .Substitute<IFlightAvailabilityService>("✈️")
                .Substitute<IPartnerNotifier>("📣")
                .Substitute<IAuditLogger>("🪵")
                .Substitute<Random>("🎲")
                .Verify(async () =>
                    new BookingCoordinator(bookingDate).BookFlight(
                        passengerName, flightNumber, departureDate,
                        passengerCount, airlineCode, specialRequests
                    ).ToString());
        }
    }
}