/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: ReservationConstants.cs
 * Description: Contains supported energy reservation statuses.
 */

namespace SmartSolar.API.Helpers
{
    public static class ReservationStatuses
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Cancelled = "CANCELLED";
        public const string Completed = "COMPLETED";
    }
}