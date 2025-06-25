using System;

namespace LegacyBookingCoordinator
{
    public class CanNotUseInTestsException : Exception
    {
        public CanNotUseInTestsException(string className)
            : base($"Cannot use {className} in tests - this class has external dependencies!")
        {
        }
    }
}