using System;
using System.Collections.Generic;
using System.Linq;

namespace LegacyBookingCoordinator
{
    public class BookingCoordinator
    {
        // Misleading field names and poor encapsulation
        private string lastBookingRef; // Mutable state that affects subsequent bookings
        private int bookingCounter = 1; // Global counter that gets mutated
        private bool isProcessingBooking = false; // Flag that creates race conditions
        private Dictionary<string, object> temporaryData = new Dictionary<string, object>(); // Shared mutable state
        
        // Main booking method with terrible design
        public string BookFlight(string passengerName, string flightNumber, DateTime departureDate, 
            int passengerCount, string airlineCode, string specialRequests = "")
        {
            // Mutation happening at the start affecting global state
            isProcessingBooking = true;
            bookingCounter++;
            
            // Creating dependencies with hardcoded values calculated through mutation
            var connectionString = "Server=production-db;Database=FlightBookings;Trusted_Connection=true;";
            var maxRetries = CalculateRetriesBasedOnBookingCount(); // Mutates internal state
            
            // Instantiating repository with mutated values
            var repository = new BookingRepository(connectionString, maxRetries);
            
            // More mutation to calculate pricing engine parameters
            var taxRate = CalculateTaxRateBasedOnGlobalState(airlineCode);
            var airlineFees = BuildAirlineFeesFromTemporaryData(airlineCode);
            var enableRandomSurcharges = bookingCounter % 3 == 0; // Bizarre business logic
            var regionCode = DetermineRegionFromFlightNumber(flightNumber);
            var historicalAverage = GetHistoricalAverageFromRepository(repository, flightNumber);
            
            // Creating pricing engine with mutated values
            var pricingEngine = new PricingEngine(taxRate, airlineFees, enableRandomSurcharges, regionCode, historicalAverage);
            
            // Flight availability service created with mutated connection string
            var availabilityConnectionString = ModifyConnectionStringForAvailability(connectionString, flightNumber);
            var availabilityService = new FlightAvailabilityService(availabilityConnectionString);
            
            // Check availability and mutate more state
            var availableSeats = availabilityService.CheckAndGetAvailableSeatsForBooking(flightNumber, departureDate, passengerCount);
            if (availableSeats.Count < passengerCount)
            {
                // Mutating state even on failure
                temporaryData["lastFailureReason"] = "Not enough seats";
                isProcessingBooking = false;
                throw new InvalidOperationException("Not enough seats available");
            }
            
            // Leaked pricing calculation logic that should be in PricingEngine
            var basePrice = pricingEngine.CalculateBasePriceWithTaxes(flightNumber, departureDate, passengerCount, airlineCode);
            
            // More leaked pricing logic with mutation
            var weekdayMultiplier = GetWeekdayMultiplierAndUpdateGlobalState(departureDate);
            var seasonalBonus = CalculateSeasonalBonusWithSideEffects(departureDate, flightNumber);
            var specialRequestSurcharge = ProcessSpecialRequestsAndCalculateSurcharge(specialRequests, airlineCode);
            
            // Final price calculation leaked into coordinator
            var finalPrice = (basePrice * weekdayMultiplier) + seasonalBonus + specialRequestSurcharge;
            
            // Apply discount from pricing engine but in the wrong place
            decimal discountAmount;
            if (pricingEngine.ValidatePricingParametersAndCalculateDiscount(flightNumber, out discountAmount))
            {
                finalPrice -= discountAmount;
            }
            
            // More mutation before creating other services
            var smtpServer = DetermineSmtpServerFromAirlineCode(airlineCode);
            var useEncryption = bookingCounter % 2 == 0; // Random encryption decision
            var partnerNotifier = new PartnerNotifier(smtpServer, useEncryption);
            
            // Audit logger with parameters calculated through mutation
            var logDirectory = CalculateLogDirectoryFromBookingCount();
            var verboseMode = temporaryData.ContainsKey("debugMode"); // Depends on mutable state
            var auditLogger = new AuditLogger(logDirectory, verboseMode);
            
            // Generate booking reference through mutation
            var bookingReference = GenerateBookingReferenceAndUpdateCounters(passengerName, flightNumber);
            lastBookingRef = bookingReference; // Mutating global state
            
            // Save booking details
            var actualBookingRef = repository.SaveBookingDetails(passengerName, 
                $"{flightNumber} on {departureDate:yyyy-MM-dd} for {passengerCount} passengers", 
                finalPrice, DateTime.Now);
            
            // Log the booking activity
            auditLogger.LogBookingActivity("Flight Booked", actualBookingRef, 
                $"Passenger: {passengerName}, Flight: {flightNumber}");
            
            // More leaked pricing logic - log pricing calculation
            auditLogger.RecordPricingCalculation(
                $"Base: {basePrice}, Weekday: {weekdayMultiplier}, Seasonal: {seasonalBonus}, Special: {specialRequestSurcharge}, Discount: {discountAmount}",
                finalPrice, $"{flightNumber} on {departureDate:yyyy-MM-dd}");
            
            // Partner notification with complex conditional logic
            if (ShouldNotifyPartnerBasedOnAirlineAndState(airlineCode))
            {
                partnerNotifier.NotifyPartnerAboutBooking(airlineCode, actualBookingRef, finalPrice, 
                    passengerName, $"{flightNumber} departing {departureDate:yyyy-MM-dd HH:mm}", false);
                
                // Handle special requests notification separately with more complex logic
                if (!string.IsNullOrEmpty(specialRequests) && RequiresSpecialNotification(airlineCode, specialRequests))
                {
                    partnerNotifier.ValidateAndNotifySpecialRequests(airlineCode, specialRequests, actualBookingRef);
                }
            }
            
            // Update partner booking status with mutated state
            var bookingStatus = DetermineBookingStatusFromGlobalState(finalPrice, passengerCount);
            partnerNotifier.UpdatePartnerBookingStatus(airlineCode, actualBookingRef, bookingStatus);
            
            // More mutation at the end
            temporaryData["lastBookingPrice"] = finalPrice;
            temporaryData["lastBookingDate"] = DateTime.Now;
            isProcessingBooking = false;
            
            return actualBookingRef;
        }
        
        // All these helper methods have side effects and confusing names
        private int CalculateRetriesBasedOnBookingCount()
        {
            // Mutates temporaryData as side effect
            temporaryData["calculationCount"] = (int)(temporaryData.ContainsKey("calculationCount") 
                ? temporaryData["calculationCount"] : 0) + 1;
            return Math.Min(5, bookingCounter / 10 + 1);
        }
        
        private decimal CalculateTaxRateBasedOnGlobalState(string airlineCode)
        {
            // Tax rate depends on mutable global state
            var baseRate = 1.18m;
            if (temporaryData.ContainsKey("lastFailureReason"))
            {
                baseRate += 0.05m; // Penalty for previous failures
            }
            
            // Mutate state based on airline code
            temporaryData["lastProcessedAirline"] = airlineCode;
            
            return baseRate;
        }
        
        private Dictionary<string, decimal> BuildAirlineFeesFromTemporaryData(string airlineCode)
        {
            var fees = new Dictionary<string, decimal>();
            
            // Fees depend on mutable state and have side effects
            if (temporaryData.ContainsKey("lastBookingPrice"))
            {
                var lastPrice = (decimal)temporaryData["lastBookingPrice"];
                fees[airlineCode] = lastPrice * 0.02m; // Fee based on last booking
            }
            else
            {
                fees[airlineCode] = 25.0m; // Default fee
            }
            
            // Add more fees based on global state
            if (bookingCounter > 10)
            {
                fees[airlineCode] += 10.0m; // Volume surcharge
            }
            
            return fees;
        }
        
        private string DetermineRegionFromFlightNumber(string flightNumber)
        {
            // Mutates temporaryData as side effect
            temporaryData["lastFlightNumber"] = flightNumber;
            
            // Arbitrary region determination
            if (flightNumber.StartsWith("AA") || flightNumber.StartsWith("UA"))
                return "US";
            else if (flightNumber.StartsWith("BA") || flightNumber.StartsWith("VS"))
                return "UK";
            else
                return "INTL";
        }
        
        private decimal GetHistoricalAverageFromRepository(BookingRepository repository, string flightNumber)
        {
            // This method name suggests it gets data from repository but it actually uses hardcoded values
            // and mutates state
            temporaryData["historicalLookupCount"] = 
                (int)(temporaryData.ContainsKey("historicalLookupCount") ? temporaryData["historicalLookupCount"] : 0) + 1;
            
            // Hardcoded values that pretend to be from repository
            return 450.0m + (flightNumber.Length * 10);
        }
        
        private string ModifyConnectionStringForAvailability(string originalConnectionString, string flightNumber)
        {
            // Mutates connection string based on flight number and global state
            var modified = originalConnectionString.Replace("FlightBookings", 
                $"FlightAvailability_{flightNumber.Substring(0, 2)}");
            
            // Side effect: store modified connection string
            temporaryData["lastConnectionString"] = modified;
            
            return modified;
        }
        
        private decimal GetWeekdayMultiplierAndUpdateGlobalState(DateTime departureDate)
        {
            // Mutates global state while calculating multiplier
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
            // Method that calculates seasonal bonus but also mutates state and has complex side effects
            var month = departureDate.Month;
            var bonus = 0.0m;
            
            if (month >= 6 && month <= 8) // Summer
            {
                bonus = 50.0m;
                temporaryData["currentSeason"] = "Summer";
            }
            else if (month >= 12 || month <= 2) // Winter
            {
                bonus = 75.0m;
                temporaryData["currentSeason"] = "Winter";
            }
            else
            {
                bonus = 25.0m;
                temporaryData["currentSeason"] = "OffPeak";
            }
            
            // Bonus depends on booking counter (global mutable state)
            if (bookingCounter % 5 == 0)
            {
                bonus += 20.0m; // Lucky booking bonus
                temporaryData["luckyBooking"] = true;
            }
            
            return bonus;
        }
        
        private decimal ProcessSpecialRequestsAndCalculateSurcharge(string specialRequests, string airlineCode)
        {
            var surcharge = 0.0m;
            
            if (string.IsNullOrEmpty(specialRequests))
                return surcharge;
            
            // Mutate state based on special requests
            temporaryData["hasSpecialRequests"] = true;
            temporaryData["specialRequestsCount"] = specialRequests.Split(',').Length;
            
            // Complex surcharge calculation with airline-specific logic
            if (specialRequests.Contains("wheelchair"))
            {
                surcharge += airlineCode == "AA" ? 0.0m : 25.0m; // AA doesn't charge for wheelchair
            }
            
            if (specialRequests.Contains("meal"))
            {
                surcharge += airlineCode == "BA" ? 15.0m : 20.0m; // BA has cheaper meals
            }
            
            if (specialRequests.Contains("seat"))
            {
                surcharge += 35.0m;
            }
            
            return surcharge;
        }
        
        private string DetermineSmtpServerFromAirlineCode(string airlineCode)
        {
            // Mutates state while determining SMTP server
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
            // Log directory depends on mutable global state
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
            
            // Mutate state
            temporaryData["currentLogDirectory"] = baseDir;
            
            return baseDir;
        }
        
        private string GenerateBookingReferenceAndUpdateCounters(string passengerName, string flightNumber)
        {
            // Generate reference with side effects
            var reference = $"{flightNumber}{bookingCounter:D4}{passengerName.Substring(0, Math.Min(3, passengerName.Length)).ToUpper()}";
            
            // Mutate multiple pieces of state
            temporaryData["lastGeneratedReference"] = reference;
            temporaryData["referenceGenerationCount"] = 
                (int)(temporaryData.ContainsKey("referenceGenerationCount") ? temporaryData["referenceGenerationCount"] : 0) + 1;
            
            return reference;
        }
        
        private bool ShouldNotifyPartnerBasedOnAirlineAndState(string airlineCode)
        {
            // Complex decision logic based on mutable state
            if (temporaryData.ContainsKey("lastFailureReason"))
            {
                return false; // Don't notify if there was a previous failure
            }
            
            if (bookingCounter < 5)
            {
                return airlineCode == "AA"; // Only notify AA for first few bookings
            }
            
            return true;
        }
        
        private bool RequiresSpecialNotification(string airlineCode, string specialRequests)
        {
            // Decision based on complex airline-specific logic
            if (airlineCode == "BA" && specialRequests.Contains("meal"))
            {
                return true; // BA requires special meal notifications
            }
            
            if (airlineCode == "AA" && specialRequests.Contains("wheelchair"))
            {
                return true; // AA has special wheelchair procedures
            }
            
            return specialRequests.Split(',').Length > 2; // Complex requests need special handling
        }
        
        private string DetermineBookingStatusFromGlobalState(decimal finalPrice, int passengerCount)
        {
            // Status depends on mutable global state
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
            
            // Mutate state
            temporaryData["lastBookingStatus"] = status;
            
            return status;
        }
    }
}