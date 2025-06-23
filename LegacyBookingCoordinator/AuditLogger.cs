using System;

namespace LegacyBookingCoordinator
{
    public class AuditLogger
    {
        // Confusing field names - filePath sounds like it's just for files but also controls database logging
        private readonly string filePath;
        private readonly bool enableConsoleOutput; // This actually writes to multiple places, not just console
        
        public AuditLogger(string logDirectory, bool verboseMode)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        // Method name suggests it's just logging but it also increments global counters
        public void LogBookingActivity(string activity, string bookingReference, string userInfo)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        // This method sounds harmless but actually sends data to external compliance systems
        public void RecordPricingCalculation(string calculationDetails, decimal finalPrice, string flightInfo)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        // Misleading method name - doesn't just log errors, also triggers alerts
        public void LogErrorWithAlert(Exception ex, string context, string bookingRef)
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }

        // This method has side effects despite sounding like a simple log flush
        public void FlushAndArchiveLogs()
        {
            throw new CanNotUseInTestsException(nameof(AuditLogger));
        }
    }
}