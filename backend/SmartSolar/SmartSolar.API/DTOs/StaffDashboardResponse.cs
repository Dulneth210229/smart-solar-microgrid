/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: StaffDashboardResponse.cs
 * Description: Represents operational reservation statistics for staff dashboards.
 */

namespace SmartSolar.API.DTOs
{
    public class StaffDashboardResponse
    {
        public int PendingReservations { get; set; }

        public int ApprovedFutureReservations { get; set; }

        public int CompletedReservations { get; set; }

        public int ActiveStations { get; set; }
    }
}