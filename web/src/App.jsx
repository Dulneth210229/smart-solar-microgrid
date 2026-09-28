import { Navigate, Route, Routes } from "react-router-dom";

import { AuthProvider } from "./contexts/AuthContext";

import ProtectedRoute from "./components/ProtectedRoute";

import DashboardLayout from "./layouts/DashboardLayout";

import LoginPage from "./pages/LoginPage";
import StaffDashboardPage from "./pages/StaffDashboardPage";
import PlaceholderPage from "./pages/PlaceholderPage";

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
                <PlaceholderPage title="User Management" />
              </ProtectedRoute>
            }
          />

          <Route
            path="/stations"
            element={
              <ProtectedRoute allowedRoles={["BACKOFFICE"]}>
                <PlaceholderPage title="Solar Stations" />
              </ProtectedRoute>
            }
          />

          <Route
            path="/bookings"
            element={<PlaceholderPage title="Reservations" />}
          />
        </Route>

        <Route path="/" element={<Navigate to="/dashboard" replace />} />

        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AuthProvider>
  );
}

export default App;
