using System;

namespace LegacyBookingCoordinator
{
    public class PartnerNotifier : IPartnerNotifier
    {
        private readonly string logDestination;
        private readonly bool enableSecureMode;
        
        public PartnerNotifier(string smtpServer, bool useEncryption)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }

        public void NotifyPartnerAboutBooking(string airlineCode, string bookingReference, 
            decimal totalPrice, string passengerName, string flightDetails, bool isRebooking = false)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }

        public bool ValidateAndNotifySpecialRequests(string airlineCode, string specialRequests, string bookingRef)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }

        public void UpdatePartnerBookingStatus(string airlineCode, string bookingRef, string newStatus)
        {
            throw new CanNotUseInTestsException(nameof(PartnerNotifier));
        }
    }
}