import { useEffect, useState } from "react";

import api from "../services/api";
import { useAuth } from "../contexts/AuthContext";

export default function StaffDashboardPage() {
  const { user } = useAuth();

  const [dashboard, setDashboard] = useState(null);

  const [loading, setLoading] = useState(true);

  const [error, setError] = useState("");

  useEffect(() => {
    const loadDashboard = async () => {
      try {
        const response = await api.get("/Dashboard/staff");

        setDashboard(response.data);
      } catch (exception) {
        setError(
          exception.response?.data?.message || "Unable to load dashboard.",
        );
      } finally {
        setLoading(false);
      }
    };

    loadDashboard();
  }, []);

  if (loading) {
    return (
      <div className="py-5 text-center">
        <div className="spinner-border" role="status"></div>
      </div>
    );
  }

  return (
    <>
      <div className="mb-4">
        <h2 className="fw-bold">Welcome, {user?.fullName}</h2>

        <p className="text-secondary">Smart Solar Microgrid Trading System</p>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {dashboard && (
        <div className="row g-4">
          <div className="col-md-6 col-xl-3">
            <div className="card stat-card shadow-sm h-100">
              <div className="card-body">
                <div className="text-secondary mb-2">Pending Reservations</div>

                <h2 className="fw-bold">{dashboard.pendingReservations}</h2>
              </div>
            </div>
          </div>

          <div className="col-md-6 col-xl-3">
            <div className="card stat-card shadow-sm h-100">
              <div className="card-body">
                <div className="text-secondary mb-2">Approved Upcoming</div>

                <h2 className="fw-bold">
                  {dashboard.approvedFutureReservations}
                </h2>
              </div>
            </div>
          </div>

          <div className="col-md-6 col-xl-3">
            <div className="card stat-card shadow-sm h-100">
              <div className="card-body">
                <div className="text-secondary mb-2">Completed Transfers</div>

                <h2 className="fw-bold">{dashboard.completedReservations}</h2>
              </div>
            </div>
          </div>

          <div className="col-md-6 col-xl-3">
            <div className="card stat-card shadow-sm h-100">
              <div className="card-body">
                <div className="text-secondary mb-2">Active Stations</div>

                <h2 className="fw-bold">{dashboard.activeStations}</h2>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
