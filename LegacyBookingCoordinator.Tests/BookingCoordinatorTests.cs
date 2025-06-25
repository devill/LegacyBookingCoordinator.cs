using System.Text;
using LegacyBookingCoordinator;
using LegacyTestingTools;
using static LegacyTestingTools.CallLogFormatterContext;

namespace LegacyBookingCoordinator.Tests
{
    public class BookingCoordinatorTests
    {
        [Fact]
        public async Task BookFlight_ShouldCreateBookingSuccessfully()
        {
            // Arrange
            var storybook = new StringBuilder();

            var cl = new CallLogger(storybook);
            var factory = ObjectFactory.Instance();

            var passengerName = "John Doe";
            var flightNumber = "AA123";
            var departureDate = new DateTime(2025, 07, 03, 12, 42, 11);
            var passengerCount = 2;
            var airlineCode = "AA";
            var specialRequests = "meal,wheelchair";
            var bookingDate = new DateTime(2025, 03, 04, 14, 00, 56);
            
            var coordinator = new BookingCoordinator(bookingDate);

            try
            { 
                factory.SetOne(cl.Wrap<IBookingRepository>(new BookingRepositoryStub(), "💾"));
                factory.SetOne(cl.Wrap<IPartnerNotifier>(new PartnerNotifierStub(), "📣"));
                factory.SetOne(cl.Wrap<IAuditLogger>(new AuditLoggerStub(), "🪵"));
                factory.SetOne(cl.Wrap<IFlightAvailabilityService>(new FlightAvailabilityServiceStub(), "✈️"));
                factory.SetOne<Random>(new RandomStub());

                var bookingReference = coordinator.BookFlight(passengerName, flightNumber, departureDate,
                    passengerCount, airlineCode, specialRequests);

                storybook.AppendLine($"Final booking reference returned: {bookingReference}");

                // Assert
                await Verify(storybook.ToString());
            }
            finally
            {
                factory.ClearAll();
            }
        }

    }

    public class AuditLoggerStub : IAuditLogger,  IConstructorCalledWith
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

        public void ConstructorCalledWith(params object[] args)
        {
            SetConstructorArgumentNames("logDirectory", "verboseMode");
        }
    }

    public class PartnerNotifierStub : IPartnerNotifier,  IConstructorCalledWith
    {
        public void NotifyPartnerAboutBooking(string airlineCode, string bookingReference, decimal totalPrice, string passengerName,
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

        public void ConstructorCalledWith(params object[] args)
        {
            SetConstructorArgumentNames("smtpServer", "useEncryption");
        }
    }

    public class FlightAvailabilityServiceStub : IFlightAvailabilityService, IConstructorCalledWith
    {
        public List<string> CheckAndGetAvailableSeatsForBooking(string flightNumber, DateTime departureDate, int passengerCount)
        {
            return ["11A", "11B"];
        }

        public bool IsFlightFullyBooked(string flightNumber, DateTime departureDate)
        {
            throw new NotImplementedException();
        }

        public void ConstructorCalledWith(params object[] args)
        {
            SetConstructorArgumentNames("connectionString");
        }
    }

    public class BookingRepositoryStub : IBookingRepository, IConstructorCalledWith
    {
        public string SaveBookingDetails(string passengerName, string flightDetails, decimal price, DateTime bookingDate)
        {
            return "DRW6N";
        }

        public Dictionary<string, object> GetBookingInfo(string bookingReference)
        {
            throw new NotImplementedException();
        }

        public bool ValidateAndEnrichBookingData(string bookingRef, out decimal actualPrice, out string enrichedFlightInfo)
        {
            throw new NotImplementedException();
        }

        public decimal GetHistoricalPricingData(string flightNumber, DateTime date, int dayRange)
        {
            throw new NotImplementedException();
        }

        public void ConstructorCalledWith(params object[] args)
        {
            SetConstructorArgumentNames("dbConnectionString", "maxRetries");
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