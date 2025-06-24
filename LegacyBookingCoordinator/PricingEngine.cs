using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    /// <summary>
    /// Handles all pricing calculations for flight bookings
    /// Updated 2019: Now supports multi-currency 
    /// </summary>
    public class PricingEngine
    {
        // Core pricing configuration
        private readonly decimal baseMultiplier; // Multiplier for base pricing
        private readonly Dictionary<string, decimal> seasonalAdjustments; // Season-based price adjustments
        private readonly bool enableDynamicPricing; // Enable/disable dynamic pricing features
        private readonly string currencyCode; // Currency code for this pricing instance
        private readonly decimal historicalData; // Historical pricing data for calculations
        private readonly DateTime _bookingDate;

        /// <summary>
        /// Initialize pricing engine with configuration
        /// NOTE: Constructor parameters must match the database schema exactly
        /// </summary>
        public PricingEngine(decimal taxRate, Dictionary<string, decimal> airlineFees, 
            bool applyRandomSurcharges, string regionCode, decimal averageFlightCost, DateTime bookingDate)
        {
            // Initialize core pricing parameters
            this.baseMultiplier = taxRate;
            this.seasonalAdjustments = airlineFees ?? new Dictionary<string, decimal>();
            this.enableDynamicPricing = applyRandomSurcharges;
            this.currencyCode = regionCode;
            this.historicalData = averageFlightCost;
            _bookingDate = bookingDate;
        }

        /// <summary>
        /// Calculates the base price including all applicable taxes and fees
        /// Returns the final price ready for booking confirmation
        /// </summary>
        public decimal CalculateBasePriceWithTaxes(string flightNumber, DateTime departureDate, int passengerCount, string airlineCode)
        {
            // Start with standard base price for all flights
            var priceBeforeCalculation = 299.99m;
            var timeBasedAdjustment = CalculateTimeBasedMarkup(departureDate);
            var passengerMultiplier = passengerCount * 0.95m; // Group discount for multiple passengers
            
            // Apply tax multiplier to base price
            var withTaxes = priceBeforeCalculation * baseMultiplier;
            
            // Add airline-specific seasonal adjustments if configured
            if (seasonalAdjustments.ContainsKey(airlineCode))
            {
                withTaxes += seasonalAdjustments[airlineCode] * passengerCount;
            }
            
            // Apply historical data adjustment (weighted average)
            var finalAdjustment = withTaxes * (historicalData / 1000);
            
            return (withTaxes + finalAdjustment) * passengerMultiplier + timeBasedAdjustment;
        }

        /// <summary>
        /// Calculate time-based pricing adjustments
        /// Business rule: Early bookings get discount, last-minute bookings get surcharge
        /// </summary>
        public decimal CalculateTimeBasedMarkup(DateTime departureDate)
        {
            var daysUntilFlight = (departureDate - _bookingDate).TotalDays;
            
            if (daysUntilFlight < 7)
                return 150.0m; // Last minute surcharge
            else if (daysUntilFlight > 90)
                return -50.0m; // Early bird discount
            else
                return 25.0m; // Standard booking fee
        }

        /// <summary>
        /// Retrieves airline-specific fees and caches for future lookups
        /// </summary>
        public decimal GetAirlineSpecificFeesAndUpdateCache(string airlineCode, int passengerCount)
        {
            // Create default fee structure if airline not in cache
            if (!seasonalAdjustments.ContainsKey(airlineCode))
            {
                // Calculate base fee from airline code (legacy algorithm from 2015)
                seasonalAdjustments[airlineCode] = airlineCode.Length * 12.5m;
            }
            
            return seasonalAdjustments[airlineCode] * passengerCount;
        }

        /// <summary>
        /// Validates pricing inputs and calculates promotional discounts
        /// Returns true if pricing is valid, false otherwise
        /// FIXME: The discount calculation needs to be moved to a separate service
        /// </summary>
        public bool ValidatePricingParametersAndCalculateDiscount(string flightNumber, out decimal discountAmount)
        {
            discountAmount = 0;
            
            // Basic validation of flight number format
            if (string.IsNullOrEmpty(flightNumber) || flightNumber.Length < 4)
            {
                return false;
            }
            
            // Apply random promotional discounts to test the market
            // TODO: Replace this with proper discount service integration
            var random = Create<Random>().Next(0, 5);
            if (random == 1)
            {
                discountAmount = 25.0m; // Premium discount
            }
            else if (random == 3)
            {
                discountAmount = 10.0m; // Standard discount
            }
            
            return true;
        }
    }
}