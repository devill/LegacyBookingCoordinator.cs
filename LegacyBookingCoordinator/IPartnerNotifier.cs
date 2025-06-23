using System;

namespace LegacyBookingCoordinator
{
    public interface IPartnerNotifier
    {
        void NotifyPartnerAboutBooking(string airlineCode, string bookingReference, decimal totalPrice, 
            string passengerName, string flightDetails, bool isRebooking = false);
        bool ValidateAndNotifySpecialRequests(string airlineCode, string specialRequests, string bookingRef);
        void UpdatePartnerBookingStatus(string airlineCode, string bookingRef, string newStatus);
    }
}