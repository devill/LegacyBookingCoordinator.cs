using System;

namespace LegacyBookingCoordinator
{
    public class PartnerNotifier
    {
        // Misleading variable name - sounds like it's just for logging but actually sends emails
        private readonly string logDestination;
        private readonly bool enableSecureMode; // This flag doesn't actually do anything security-related
        
        public PartnerNotifier(string smtpServer, bool useEncryption)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }

        // Method with confusing parameters - airline type affects the entire notification format
        public void NotifyPartnerAboutBooking(string airlineCode, string bookingReference, 
            decimal totalPrice, string passengerName, string flightDetails, bool isRebooking = false)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }

        // This method sounds like it validates but actually sends notifications too
        public bool ValidateAndNotifySpecialRequests(string airlineCode, string specialRequests, string bookingRef)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }

        // Poorly named method - doesn't just update, also triggers external calls
        public void UpdatePartnerBookingStatus(string airlineCode, string bookingRef, string newStatus)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }
    }
}