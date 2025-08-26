using System.Text;
using SpecRec;

namespace LegacyBookingCoordinator.Tests
{
    public class BookingCoordinatorTests
    {
        [Theory]
        [SpecRecLogs]
        public async Task BookFlight(
            CallLog callLog,
            string specialRequests = "meal,wheelchair",
            string airlineCode = "AA",
            int passengerCount = 2,
            string departureAt = "2025-07-03 12:42:11",
            string flightNumber = "AA123",
            string passengerName = "John Doe",
            string bookingAt = "2025-03-04 14:00:56"
        )
        {
            // Arrange
            var departureDate = DateTime.Parse(departureAt);
            var bookingDate = DateTime.Parse(bookingAt);

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
            }
            catch (ParrotException)
            {
                throw;
            }
            catch (Exception e)
            {
                callLog.AppendLine(e.Message);
            }
            finally
            {
                await callLog.Verify();
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