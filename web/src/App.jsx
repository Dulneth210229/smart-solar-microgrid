import { Navigate, Route, Routes } from "react-router-dom";

import { AuthProvider } from "./contexts/AuthContext";

import ProtectedRoute from "./components/ProtectedRoute";

import DashboardLayout from "./layouts/DashboardLayout";

import LoginPage from "./pages/LoginPage";
import StaffDashboardPage from "./pages/StaffDashboardPage";

import UsersPage from "./pages/UsersPage";
import StationsPage from "./pages/StationsPage";
import BookingSlotsPage from "./pages/BookingSlotsPage";
import ReservationsPage from "./pages/ReservationsPage";
function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route
          element={
            <ProtectedRoute allowedRoles={["BACKOFFICE", "GRID_OPERATOR"]}>
              <DashboardLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/dashboard" element={<StaffDashboardPage />} />

          <Route
            path="/users"
            element={
              <ProtectedRoute allowedRoles={["BACKOFFICE"]}>
                <UsersPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/stations"
            element={
              <ProtectedRoute allowedRoles={["BACKOFFICE", "GRID_OPERATOR"]}>
                <StationsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/stations/:stationId/slots"
            element={
              <ProtectedRoute allowedRoles={["BACKOFFICE", "GRID_OPERATOR"]}>
                <BookingSlotsPage />
              </ProtectedRoute>
            }
          />

          <Route
            path="/bookings"
            element={
              <ProtectedRoute allowedRoles={["BACKOFFICE", "GRID_OPERATOR"]}>
                <ReservationsPage />
              </ProtectedRoute>
            }
          />
        </Route>

        <Route path="/" element={<Navigate to="/dashboard" replace />} />

        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AuthProvider>
  );
}

export default App;
