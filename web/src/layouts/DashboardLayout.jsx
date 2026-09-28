import { NavLink, Outlet, useNavigate } from "react-router-dom";

import { useAuth } from "../contexts/AuthContext";

export default function DashboardLayout() {
  const { user, logout } = useAuth();

  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="container-fluid">
      <div className="row">
        <aside className="col-md-3 col-lg-2 p-3 dashboard-sidebar text-white">
          <div className="mb-4">
            <h4 className="fw-bold mb-1">Smart Solar</h4>

            <small className="text-white-50">
              {user?.role === "BACKOFFICE" ? "Backoffice" : "Grid Operator"}
            </small>
          </div>

          <nav className="nav flex-column">
            <NavLink to="/dashboard" className="nav-link">
              <i className="bi bi-speedometer2 me-2"></i>
              Dashboard
            </NavLink>

            {user?.role === "BACKOFFICE" && (
              <>
                <NavLink to="/users" className="nav-link">
                  <i className="bi bi-people me-2"></i>
                  Users
                </NavLink>

                <NavLink to="/stations" className="nav-link">
                  <i className="bi bi-lightning-charge me-2"></i>
                  Stations
                </NavLink>
              </>
            )}

            <NavLink to="/bookings" className="nav-link">
              <i className="bi bi-calendar-check me-2"></i>
              Reservations
            </NavLink>
          </nav>

          <hr />

          <div className="small mb-3">
            <div className="fw-semibold">{user?.fullName}</div>

            <div className="text-white-50">{user?.email}</div>
          </div>

          <button
            type="button"
            className="btn btn-outline-light w-100"
            onClick={handleLogout}
          >
            <i className="bi bi-box-arrow-right me-2"></i>
            Logout
          </button>
        </aside>

        <main className="col-md-9 col-lg-10 px-md-4 py-4">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
