using System.Text;
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
            var bookingDate = new DateTime(2025, 03, 04, 14, 00, 56);

            var callLog = CallLog.FromVerifiedFile();

            var factory = ObjectFactory.Instance();

            try
            {
                factory.SetOne(Parrot.Create<IBookingRepository>(callLog, "💾"));
                factory.SetOne(Parrot.Create<IPartnerNotifier>(callLog, "📣"));
                factory.SetOne(Parrot.Create<IAuditLogger>(callLog, "🪵"));
                factory.SetOne(Parrot.Create<IFlightAvailabilityService>(callLog, "✈️"));
                factory.SetOne<Random>(new RandomStub());

                var coordinator = new BookingCoordinator(bookingDate);
                var booking = coordinator.BookFlight(passengerName, flightNumber, departureDate,
                    passengerCount, airlineCode, specialRequests);

                callLog.AppendLine(booking.ToString());

                // Assert
                await callLog.Verify();
            }
            finally
            {
                factory.ClearAll();
            }
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