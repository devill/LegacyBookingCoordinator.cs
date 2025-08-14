using System;

namespace LegacyBookingCoordinator
{
    public class Booking(
        string bookingReference,
        string passengerName,
        string flightNumber,
        DateTime departureDate,
        int passengerCount,
        string airlineCode,
        decimal finalPrice,
        string specialRequests,
        DateTime bookingDate,
        string status)
    {
        public string BookingReference { get; } = bookingReference;
        public string PassengerName { get; } = passengerName;
        public string FlightNumber { get; } = flightNumber;
        public DateTime DepartureDate { get; } = departureDate;
        public int PassengerCount { get; } = passengerCount;
        public string AirlineCode { get; } = airlineCode;
        public decimal FinalPrice { get; } = finalPrice;
        public string SpecialRequests { get; } = specialRequests;
        public DateTime BookingDate { get; } = bookingDate;
        public string Status { get; } = status;

        public override string ToString()
        {
            var result = new System.Text.StringBuilder();
            
            result.AppendLine($"New booking: {BookingReference}");
            result.AppendLine($"  👤 {PassengerName}");
            result.AppendLine($"  ✈️ {FlightNumber}");
            result.AppendLine($"  📅 {DepartureDate:yyyy-MM-dd HH:mm}");
            result.AppendLine($"  👥 {PassengerCount}");
            result.AppendLine($"  🏢 {AirlineCode}");
            result.AppendLine($"  💰 ${FinalPrice:F2}");
            
            if (!string.IsNullOrEmpty(SpecialRequests))
            {
                result.AppendLine($"  🎯 {SpecialRequests}");
            }
            
            result.AppendLine($"  📝 {BookingDate:yyyy-MM-dd HH:mm}");
            result.Append($"  ✅ {Status}");
            
            return result.ToString();
        }
    }
}