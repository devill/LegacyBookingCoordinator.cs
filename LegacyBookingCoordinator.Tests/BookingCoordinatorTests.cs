using SpecRec;

namespace LegacyBookingCoordinator.Tests
{
    public class BookingCoordinatorTests
    {
        [Theory]
        [SpecRecLogs]
        public async Task BookFlight(
            Context context,
            string passengerName = "John Doe",
            string flightNumber = "AA123",
            DateTime? departureDate = null,
            int passengerCount = 2,
            string airlineCode = "AA",
            string specialRequests = "meal,wheelchair",
            DateTime? bookingDate = null
        )
        {
            // Use default values if DateTime parameters are not set
            var actualDepartureDate = departureDate ?? new DateTime(2025, 07, 03, 12, 42, 11);
            var actualBookingDate = bookingDate ?? new DateTime(2025, 03, 04, 14, 00, 56);

            await context
                .Substitute<IBookingRepository>("💾")
                .Substitute<IFlightAvailabilityService>("✈️")
                .Substitute<IPartnerNotifier>("📣")
                .Substitute<IAuditLogger>("🪵")
                .Substitute<Random>("🎲")
                .Verify(async () =>
                    new BookingCoordinator(actualBookingDate).BookFlight(
                        passengerName, flightNumber, actualDepartureDate,
                        passengerCount, airlineCode, specialRequests
                    ).ToString());
        }
    }
}