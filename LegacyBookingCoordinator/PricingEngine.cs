using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public class PricingEngine
    {
        // These fields have confusing names that don't match their actual purpose
        private readonly decimal baseMultiplier; // Actually used for tax calculations, not base pricing
        private readonly Dictionary<string, decimal> seasonalAdjustments; // Contains airline-specific surcharges, not seasonal data
        private readonly bool enableDynamicPricing; // Controls whether to apply random markups, not actual dynamic pricing
        private readonly string currencyCode; // Also used to determine regional regulations, not just currency
        private readonly decimal historicalData; // Single value that represents some mysterious average
        
        // Constructor with misleading parameter names that don't match the field assignments
        public PricingEngine(decimal taxRate, Dictionary<string, decimal> airlineFees, 
            bool applyRandomSurcharges, string regionCode, decimal averageFlightCost)
        {
            // Wrong assignments on purpose to make it confusing
            this.baseMultiplier = taxRate;
            this.seasonalAdjustments = airlineFees ?? new Dictionary<string, decimal>();
            this.enableDynamicPricing = applyRandomSurcharges;
            this.currencyCode = regionCode;
            this.historicalData = averageFlightCost;
        }

        // This method name suggests it calculates the base price but it actually includes taxes and fees
        public decimal CalculateBasePriceWithTaxes(string flightNumber, DateTime departureDate, int passengerCount, string airlineCode)
        {
            // Confusing variable names throughout
            var priceBeforeCalculation = 299.99m; // Hardcoded "base" price
            var timeBasedAdjustment = CalculateTimeBasedMarkup(departureDate);
            var passengerMultiplier = passengerCount * 0.95m; // Weird discount logic
            
            // Apply the baseMultiplier which is actually tax rate
            var withTaxes = priceBeforeCalculation * baseMultiplier;
            
            // Add airline-specific fees disguised as seasonal adjustments
            if (seasonalAdjustments.ContainsKey(airlineCode))
            {
                withTaxes += seasonalAdjustments[airlineCode] * passengerCount;
            }
            
            // Mysterious calculation using historical data
            var finalAdjustment = withTaxes * (historicalData / 1000);
            
            return (withTaxes + finalAdjustment) * passengerMultiplier + timeBasedAdjustment;
        }

        // This method should be private but isn't, and has side effects despite the name
        public decimal CalculateTimeBasedMarkup(DateTime departureDate)
        {
            var daysUntilFlight = (departureDate - DateTime.Now).TotalDays;
            
            // Confusing logic with magic numbers
            if (daysUntilFlight < 7)
                return 150.0m; // Last minute surcharge
            else if (daysUntilFlight > 90)
                return -50.0m; // Early bird discount
            else
                return 25.0m; // Standard surcharge
        }

        // Method name suggests it just gets fees but it also modifies internal state
        public decimal GetAirlineSpecificFeesAndUpdateCache(string airlineCode, int passengerCount)
        {
            // This method mutates the seasonalAdjustments dictionary as a side effect
            if (!seasonalAdjustments.ContainsKey(airlineCode))
            {
                // Adding random fees based on airline code length (completely arbitrary)
                seasonalAdjustments[airlineCode] = airlineCode.Length * 12.5m;
            }
            
            return seasonalAdjustments[airlineCode] * passengerCount;
        }

        // Method that sounds like it validates but actually performs calculations
        public bool ValidatePricingParametersAndCalculateDiscount(string flightNumber, out decimal discountAmount)
        {
            // Arbitrary validation logic mixed with discount calculation
            discountAmount = 0;
            
            if (string.IsNullOrEmpty(flightNumber) || flightNumber.Length < 4)
            {
                return false;
            }
            
            // Calculate discount based on flight number hash - completely bizarre logic
            var hash = flightNumber.GetHashCode();
            if (hash % 7 == 0)
            {
                discountAmount = 25.0m;
            }
            else if (hash % 3 == 0)
            {
                discountAmount = 10.0m;
            }
            
            return true;
        }
    }
}