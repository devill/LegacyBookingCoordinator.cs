using System;

namespace LegacyBookingCoordinator
{
    public interface IAuditLogger
    {
        void LogBookingActivity(string activity, string userInfo);
        void RecordPricingCalculation(string calculationDetails, decimal finalPrice, string flightInfo);
        void LogErrorWithAlert(Exception ex, string context, string bookingRef);
        void FlushAndArchiveLogs();
    }
}