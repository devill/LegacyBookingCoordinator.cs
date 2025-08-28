using System.CodeDom;
using System.Text;
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
            string departureAt = "2025-07-03 12:42:11",
            int passengerCount = 2,
            string airlineCode = "AA",
            string specialRequests = "meal,wheelchair",
            string bookingAt = "2025-03-04 14:00:56"
            )
        {
            var departureDate = DateTime.Parse(departureAt);
            var bookingDate = DateTime.Parse(bookingAt);
            
            await context.Verify(async () => {
                context
                    .Substitute<IBookingRepository>("💾")
                    .Substitute<IFlightAvailabilityService>("✈️")
                    .Substitute<IPartnerNotifier>("📣")
                    .Substitute<IAuditLogger>("🪵");
                
                context.SetOne<Random>(new RandomStub());
                
                var coordinator = new BookingCoordinator(bookingDate);
                return coordinator.BookFlight(passengerName, flightNumber, departureDate,
                    passengerCount, airlineCode, specialRequests).ToString();
            });
        }
    }

    public class RandomStub : Random
    {
        public override int Next(int minValue, int maxValue)
        {
            return 3;
        }
    }
}