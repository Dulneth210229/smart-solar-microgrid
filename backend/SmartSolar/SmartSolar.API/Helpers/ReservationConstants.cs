/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: ReservationConstants.cs
 * Description: Contains supported reservation statuses and transfer types.
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

    public static class ReservationTransferTypes
    {
        public const string DropOff = "DROP_OFF";
        public const string Charging = "CHARGING";
    }
}