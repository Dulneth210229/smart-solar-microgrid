/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: ProsumerDashboardResponse.cs
 * Description: Represents booking statistics shown on the Prosumer dashboard.
 */

namespace SmartSolar.API.DTOs
{
    public class ProsumerDashboardResponse
    {
        public int CurrentReservations { get; set; }

        public int PendingReservations { get; set; }

        public int ApprovedFutureReservations { get; set; }

        public int CompletedReservations { get; set; }

        public int CancelledReservations { get; set; }
    }
}