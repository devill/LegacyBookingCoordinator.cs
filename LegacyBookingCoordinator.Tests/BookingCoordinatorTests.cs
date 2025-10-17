using SpecRec;
using ObjectFactory;

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

            var factory = ObjectFactory.ObjectFactory.Instance();
            try
            {
                factory.SetOne<IBookingRepository>(new BookingRepositoryStub());
                factory.SetOne<IFlightAvailabilityService>(new FlightAvailabilityServiceStub());
                factory.SetOne<IPartnerNotifier>(new PartnerNotifierStub());
                factory.SetOne<IAuditLogger>(new AuditLoggerStub());
                factory.SetOne<Random>(new RandomStub());

                var result = new BookingCoordinator(bookingDate).BookFlight(
                    passengerName, flightNumber, departureDate,
                    passengerCount, airlineCode, specialRequests
                );

                await Verify(result.ToString());
            }
            finally
            {
                factory.ClearAll();
            }
        }
    }
    
    public class AuditLoggerStub : IAuditLogger
    {
        public void LogBookingActivity(string activity, string bookingReference, string userInfo)
        {
        }

        public void RecordPricingCalculation(string calculationDetails, decimal finalPrice, string flightInfo)
        {
        }

        public void LogErrorWithAlert(Exception ex, string context, string bookingRef)
        {
            throw new NotImplementedException();
        }

        public void FlushAndArchiveLogs()
        {
            throw new NotImplementedException();
        }
    }

    public class PartnerNotifierStub : IPartnerNotifier
    {
        public void NotifyPartnerAboutBooking(string airlineCode, string bookingReference, decimal totalPrice,
            string passengerName,
            string flightDetails, bool isRebooking = false)
        {
        }

        public bool ValidateAndNotifySpecialRequests(string airlineCode, string specialRequests, string bookingRef)
        {
            return true;
        }

        public void UpdatePartnerBookingStatus(string airlineCode, string bookingRef, string newStatus)
        {
        }
    }

    public class FlightAvailabilityServiceStub : IFlightAvailabilityService
    {
        public List<string> CheckAndGetAvailableSeatsForBooking(string flightNumber, DateTime departureDate,
            int passengerCount)
        {
            return ["11A", "11B"];
        }

        public bool IsFlightFullyBooked(string flightNumber, DateTime departureDate)
        {
            throw new NotImplementedException();
        }
    }

    public class BookingRepositoryStub : IBookingRepository
    {
        public string SaveBookingDetails(string passengerName, string flightDetails, decimal price,
            DateTime bookingDate)
        {
            return "APPLE3.14";
        }

        public Dictionary<string, object> GetBookingInfo(string bookingReference)
        {
            throw new NotImplementedException();
        }

        public bool ValidateAndEnrichBookingData(string bookingRef, out decimal actualPrice,
            out string enrichedFlightInfo)
        {
            throw new NotImplementedException();
        }

        public decimal GetHistoricalPricingData(string flightNumber, DateTime date, int dayRange)
        {
            throw new NotImplementedException();
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