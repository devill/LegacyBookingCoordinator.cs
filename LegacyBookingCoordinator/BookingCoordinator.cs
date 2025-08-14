/*
 * WARNING: ABANDON ALL HOPE, YE WHO ENTER HERE
 *
 * This is the infamous BookingCoordinator - a monument to technical debt
 * and a testament to what happens when deadlines triumph over design.
 *
 * This code has claimed many victims. It's entangled, stateful, and has
 * side effects that ripple through dimensions unknown to mortal developers.
 * Every attempt to "improve" it has only made it stronger and more vengeful.
 *
 * The original developers have long since fled to safer pastures (or therapy).
 * Managers have learned not to mention refactoring within earshot of this file.
 * Even the automated tests are afraid to look directly at it.
 *
 * In case you decided to ignore this warning increment the counter below and sign
 * with your name and an emoji reflecting your current mental state.
 *
 * You are victim: #10
 * The knights who gave their best before you:
 *  - Jack 🥵
 *  - Bob 😱
 *  - Mary 🫣
 *  - Jack 🤬(again)
 *  - Nathan 🥺
 *  - Mary 🙈
 *  - June 😵
 *  - Nathan 🤮
 *  - Jack 😵‍💫 (I still didn't learn my lesson)
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace LegacyBookingCoordinator
{
    /// <summary>
    /// Main coordinator for flight booking operations
    /// Integrates with all airline partners and handles end-to-end booking flow
    /// Last updated: 2018 (needs refactoring for new airline partnerships)
    /// </summary>
    public class BookingCoordinator
    {
        private readonly DateTime _bookingDate;
        private string lastBookingRef; // Stores reference for debugging purposes
        private int bookingCounter = 1; // Global counter for booking sequence
        private bool isProcessingBooking = false; // Thread safety flag (NOTE: not actually thread-safe)

        private Dictionary<string, object>
            temporaryData = new Dictionary<string, object>(); // Temporary storage for calculation intermediates

        public BookingCoordinator(DateTime bookingDate)
        {
            _bookingDate = bookingDate;
        }

        public BookingCoordinator()
        {
            _bookingDate = DateTime.Now;
        }

        /// <summary>
        /// Main entry point for flight booking process
        /// Coordinates all services and returns booking object
        /// WARNING: This method is not thread-safe due to shared state
        /// </summary>
        public Booking BookFlight(string passengerName, string flightNumber, DateTime departureDate,
            int passengerCount, string airlineCode, string specialRequests = "")
        {
            // Set processing flag to prevent concurrent access
            isProcessingBooking = true;
            bookingCounter++; // Increment global booking counter

            // Initialize database connection (TODO: move to configuration file)
            var connectionString = "Server=production-db;Database=FlightBookings;Trusted_Connection=true;";
            var maxRetries = CalculateRetriesBasedOnBookingCount(); // Dynamic retry calculation

            // Create repository with calculated parameters
            var repository = Create<IBookingRepository, BookingRepository>(connectionString, maxRetries);

            // Calculate pricing engine parameters based on current state
            var taxRate = CalculateTaxRateBasedOnGlobalState(airlineCode);
            var airlineFees = BuildAirlineFeesFromTemporaryData(airlineCode);
            var enableRandomSurcharges = bookingCounter % 3 == 0; // Enable surcharges every 3rd booking
            var regionCode = DetermineRegionFromFlightNumber(flightNumber);
            var historicalAverage = GetHistoricalAverageFromRepository(repository, flightNumber);

            var pricingEngine = new PricingEngine(taxRate, airlineFees, enableRandomSurcharges, regionCode,
                historicalAverage, _bookingDate);

            var availabilityConnectionString = ModifyConnectionStringForAvailability(connectionString, flightNumber);
            var availabilityService =
                Create<IFlightAvailabilityService, FlightAvailabilityService>(availabilityConnectionString);

            var availableSeats =
                availabilityService.CheckAndGetAvailableSeatsForBooking(flightNumber, departureDate, passengerCount);
            if (availableSeats.Count < passengerCount)
            {
                temporaryData["lastFailureReason"] = "Not enough seats";
                isProcessingBooking = false;
                throw new InvalidOperationException("Not enough seats available");
            }

            var basePrice =
                pricingEngine.CalculateBasePriceWithTaxes(flightNumber, departureDate, passengerCount, airlineCode);

            // Apply additional pricing adjustments not handled by PricingEngine
            var weekdayMultiplier = GetWeekdayMultiplierAndUpdateGlobalState(departureDate);
            var seasonalBonus = CalculateSeasonalBonusWithSideEffects(departureDate, flightNumber);
            var specialRequestSurcharge = ProcessSpecialRequestsAndCalculateSurcharge(specialRequests, airlineCode);

            // Calculate final price with all adjustments
            var finalPrice = (basePrice * weekdayMultiplier) + seasonalBonus + specialRequestSurcharge;

            // Apply any promotional discounts
            decimal discountAmount;
            if (pricingEngine.ValidatePricingParametersAndCalculateDiscount(flightNumber, out discountAmount))
            {
                finalPrice -= discountAmount;
            }

            // Configure partner notification settings
            var smtpServer = DetermineSmtpServerFromAirlineCode(airlineCode);
            var useEncryption = bookingCounter % 2 == 0; // Alternate encryption for load balancing
            var partnerNotifier = Create<IPartnerNotifier, PartnerNotifier>(smtpServer, useEncryption);

            // Setup audit logging with dynamic configuration
            var logDirectory = CalculateLogDirectoryFromBookingCount();
            var verboseMode = temporaryData.ContainsKey("debugMode"); // Enable verbose mode if debug flag set
            var auditLogger = Create<IAuditLogger, AuditLogger>(logDirectory, verboseMode);

            // Generate unique booking reference
            var bookingReference = GenerateBookingReferenceAndUpdateCounters(passengerName, flightNumber);
            lastBookingRef = bookingReference; // Store for debugging and error tracking

            // Save booking details
            var actualBookingRef = repository.SaveBookingDetails(passengerName,
                $"{flightNumber} on {departureDate:yyyy-MM-dd} for {passengerCount} passengers",
                finalPrice, _bookingDate);

            // Log the booking activity
            auditLogger.LogBookingActivity("Flight Booked", actualBookingRef,
                $"Passenger: {passengerName}, Flight: {flightNumber}");

            auditLogger.RecordPricingCalculation(
                $"Base: {basePrice}, Weekday: {weekdayMultiplier}, Seasonal: {seasonalBonus}, Special: {specialRequestSurcharge}, Discount: {discountAmount}",
                finalPrice, $"{flightNumber} on {departureDate:yyyy-MM-dd}");

            // Partner notification 
            if (ShouldNotifyPartnerBasedOnAirlineAndState(airlineCode))
            {
                partnerNotifier.NotifyPartnerAboutBooking(airlineCode, actualBookingRef, finalPrice,
                    passengerName, $"{flightNumber} departing {departureDate:yyyy-MM-dd HH:mm}", false);

                // Handle special requests 
                if (!string.IsNullOrEmpty(specialRequests) && RequiresSpecialNotification(airlineCode, specialRequests))
                {
                    partnerNotifier.ValidateAndNotifySpecialRequests(airlineCode, specialRequests, actualBookingRef);
                }
            }

            var bookingStatus = DetermineBookingStatusFromGlobalState(finalPrice, passengerCount);
            partnerNotifier.UpdatePartnerBookingStatus(airlineCode, actualBookingRef, bookingStatus);

            temporaryData["lastBookingPrice"] = finalPrice;
            temporaryData["lastBookingDate"] = _bookingDate;
            isProcessingBooking = false;

            return new Booking(actualBookingRef, passengerName, flightNumber, departureDate,
                passengerCount, airlineCode, finalPrice, specialRequests, _bookingDate, bookingStatus);
        }

        private int CalculateRetriesBasedOnBookingCount()
        {
            temporaryData["calculationCount"] = (int)(temporaryData.ContainsKey("calculationCount")
                ? temporaryData["calculationCount"]
                : 0) + 1;
            return Math.Min(5, bookingCounter / 10 + 1);
        }

        private decimal CalculateTaxRateBasedOnGlobalState(string airlineCode)
        {
            var baseRate = 1.18m;
            if (temporaryData.ContainsKey("lastFailureReason"))
            {
                baseRate += 0.05m;
            }

            temporaryData["lastProcessedAirline"] = airlineCode;

            return baseRate;
        }

        private Dictionary<string, decimal> BuildAirlineFeesFromTemporaryData(string airlineCode)
        {
            var fees = new Dictionary<string, decimal>();

            if (temporaryData.ContainsKey("lastBookingPrice"))
            {
                var lastPrice = (decimal)temporaryData["lastBookingPrice"];
                fees[airlineCode] = lastPrice * 0.02m;
            }
            else
            {
                fees[airlineCode] = 25.0m;
            }

            if (bookingCounter > 10)
            {
                fees[airlineCode] += 10.0m;
            }

            return fees;
        }

        private string DetermineRegionFromFlightNumber(string flightNumber)
        {
            temporaryData["lastFlightNumber"] = flightNumber;

            if (flightNumber.StartsWith("AA") || flightNumber.StartsWith("UA"))
                return "US";
            else if (flightNumber.StartsWith("BA") || flightNumber.StartsWith("VS"))
                return "UK";
            else
                return "INTL";
        }

        private decimal GetHistoricalAverageFromRepository(IBookingRepository repository, string flightNumber)
        {
            temporaryData["historicalLookupCount"] =
                (int)(temporaryData.ContainsKey("historicalLookupCount") ? temporaryData["historicalLookupCount"] : 0) +
                1;

            return 450.0m + (flightNumber.Length * 10);
        }

        private string ModifyConnectionStringForAvailability(string originalConnectionString, string flightNumber)
        {
            var modified = originalConnectionString.Replace("FlightBookings",
                $"FlightAvailability_{flightNumber.Substring(0, 2)}");

            temporaryData["lastConnectionString"] = modified;

            return modified;
        }

        private decimal GetWeekdayMultiplierAndUpdateGlobalState(DateTime departureDate)
        {
            temporaryData["lastDepartureDate"] = departureDate;

            var dayOfWeek = departureDate.DayOfWeek;
            if (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Sunday)
            {
                temporaryData["isPeakDay"] = true;
                return 1.25m;
            }
            else if (dayOfWeek == DayOfWeek.Tuesday || dayOfWeek == DayOfWeek.Wednesday)
            {
                temporaryData["isPeakDay"] = false;
                return 0.9m;
            }

            temporaryData["isPeakDay"] = false;
            return 1.0m;
        }

        private decimal CalculateSeasonalBonusWithSideEffects(DateTime departureDate, string flightNumber)
        {
            var month = departureDate.Month;
            var bonus = 0.0m;

            if (month >= 6 && month <= 8)
            {
                bonus = 50.0m;
                temporaryData["currentSeason"] = "Summer";
            }
            else if (month >= 12 || month <= 2)
            {
                bonus = 75.0m;
                temporaryData["currentSeason"] = "Winter";
            }
            else
            {
                bonus = 25.0m;
                temporaryData["currentSeason"] = "OffPeak";
            }

            if (bookingCounter % 5 == 0)
            {
                bonus += 20.0m;
                temporaryData["luckyBooking"] = true;
            }

            return bonus;
        }

        private decimal ProcessSpecialRequestsAndCalculateSurcharge(string specialRequests, string airlineCode)
        {
            var surcharge = 0.0m;

            if (string.IsNullOrEmpty(specialRequests))
                return surcharge;

            temporaryData["hasSpecialRequests"] = true;
            temporaryData["specialRequestsCount"] = specialRequests.Split(',').Length;

            if (specialRequests.Contains("wheelchair"))
            {
                surcharge += airlineCode == "AA" ? 0.0m : 25.0m;
            }

            if (specialRequests.Contains("meal"))
            {
                surcharge += airlineCode == "BA" ? 15.0m : 20.0m;
            }

            if (specialRequests.Contains("seat"))
            {
                surcharge += 35.0m;
            }

            return surcharge;
        }

        private string DetermineSmtpServerFromAirlineCode(string airlineCode)
        {
            temporaryData["lastSmtpLookup"] = DateTime.Now;

            switch (airlineCode)
            {
                case "AA":
                    return "smtp.american.com";
                case "UA":
                    return "smtp.united.com";
                case "BA":
                    return "smtp.britishairways.com";
                default:
                    return "smtp.generic-airline.com";
            }
        }

        private string CalculateLogDirectoryFromBookingCount()
        {
            var baseDir = @"C:\Logs\BookingLogs";

            if (bookingCounter > 100)
            {
                baseDir += @"\HighVolume";
            }
            else if (bookingCounter > 50)
            {
                baseDir += @"\MediumVolume";
            }
            else
            {
                baseDir += @"\LowVolume";
            }

            temporaryData["currentLogDirectory"] = baseDir;

            return baseDir;
        }

        private string GenerateBookingReferenceAndUpdateCounters(string passengerName, string flightNumber)
        {
            var reference =
                $"{flightNumber}{bookingCounter:D4}{passengerName.Substring(0, Math.Min(3, passengerName.Length)).ToUpper()}";

            temporaryData["lastGeneratedReference"] = reference;
            temporaryData["referenceGenerationCount"] =
                (int)(temporaryData.ContainsKey("referenceGenerationCount")
                    ? temporaryData["referenceGenerationCount"]
                    : 0) + 1;

            return reference;
        }

        private bool ShouldNotifyPartnerBasedOnAirlineAndState(string airlineCode)
        {
            if (temporaryData.ContainsKey("lastFailureReason"))
            {
                return false;
            }

            if (bookingCounter < 5)
            {
                return airlineCode == "AA";
            }

            return true;
        }

        private bool RequiresSpecialNotification(string airlineCode, string specialRequests)
        {
            if (airlineCode == "BA" && specialRequests.Contains("meal"))
            {
                return true;
            }

            if (airlineCode == "AA" && specialRequests.Contains("wheelchair"))
            {
                return true;
            }

            return specialRequests.Split(',').Length > 2;
        }

        private string DetermineBookingStatusFromGlobalState(decimal finalPrice, int passengerCount)
        {
            var status = "CONFIRMED";

            if (temporaryData.ContainsKey("isPeakDay") && (bool)temporaryData["isPeakDay"])
            {
                status = "CONFIRMED_PEAK";
            }

            if (finalPrice > 1000)
            {
                status = "CONFIRMED_PREMIUM";
            }

            if (passengerCount > 5)
            {
                status = "CONFIRMED_GROUP";
            }

            temporaryData["lastBookingStatus"] = status;

            return status;
        }
    }
}