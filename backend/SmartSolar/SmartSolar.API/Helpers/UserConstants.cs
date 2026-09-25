/*
 * Module: SE4040 - Enterprise Application Development
 * Project: Smart Solar Microgrid Trading System
 * File: UserConstants.cs
 * Description: Contains supported user roles and account statuses.
 */

namespace SmartSolar.API.Helpers
{
    public static class UserRoles
    {
        public const string Backoffice = "BACKOFFICE";
        public const string GridOperator = "GRID_OPERATOR";
        public const string Prosumer = "PROSUMER";
    }

    public static class UserStatuses
    {
        public const string Pending = "PENDING";
        public const string Active = "ACTIVE";
        public const string DeactivationRequested = "DEACTIVATION_REQUESTED";
        public const string Deactivated = "DEACTIVATED";
    }
}