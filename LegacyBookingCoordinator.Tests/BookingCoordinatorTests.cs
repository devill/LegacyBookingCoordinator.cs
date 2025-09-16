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

            await context.Verify(async () => {
                context.SetOne(context.Parrot<IBookingRepository>("💾"));
                context.SetOne(context.Parrot<IFlightAvailabilityService>("✈️"));
                context.SetOne(context.Parrot<IPartnerNotifier>("📣"));
                context.SetOne(context.Parrot<IAuditLogger>("🪵"));
                context.SetOne(context.Parrot<Random>("🎲"));
                
                var coordinator = new BookingCoordinator(bookingDate);
                return coordinator.BookFlight(passengerName, flightNumber, departureDate,
                    passengerCount, airlineCode, specialRequests).ToString();
            });  
        }
    }
}