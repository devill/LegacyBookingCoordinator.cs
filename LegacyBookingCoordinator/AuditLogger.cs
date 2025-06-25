using System;

namespace LegacyBookingCoordinator
{
    public class AuditLogger : IAuditLogger
    {
        private readonly string filePath;
        private readonly bool enableConsoleOutput;

        public AuditLogger(string logDirectory, bool verboseMode)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        public void LogBookingActivity(string activity, string bookingReference, string userInfo)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        public void RecordPricingCalculation(string calculationDetails, decimal finalPrice, string flightInfo)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        public void LogErrorWithAlert(Exception ex, string context, string bookingRef)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        public void FlushAndArchiveLogs()
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }
    }
}