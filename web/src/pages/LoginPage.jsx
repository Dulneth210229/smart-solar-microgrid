import { useState } from "react";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

export default function LoginPage() {
  const { login, isAuthenticated } = useAuth();

  const navigate = useNavigate();

  const [identifier, setIdentifier] = useState("");

  const [password, setPassword] = useState("");

  const [error, setError] = useState("");

  const [loading, setLoading] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  const handleSubmit = async (event) => {
    event.preventDefault();

    setError("");
    setLoading(true);

    try {
      await login(identifier, password);

      navigate("/dashboard");
    } catch (exception) {
      const message =
        exception.response?.data?.message ||
        exception.message ||
        "Unable to login.";

      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="container">
        <div className="row justify-content-center align-items-center min-vh-100">
          <div className="col-12 col-md-7 col-lg-5 col-xl-4">
            <div className="card shadow-lg border-0 login-card">
              <div className="card-body p-4 p-lg-5">
                <div className="text-center mb-4">
                  <div className="login-icon mb-3">
                    <i className="bi bi-sun-fill"></i>
                  </div>

                  <h3 className="fw-bold mb-1">Smart Solar</h3>

                  <p className="text-secondary mb-0">
                    Microgrid Trading System
                  </p>
                </div>

                {error && (
                  <div className="alert alert-danger" role="alert">
                    {error}
                  </div>
                )}

                <form onSubmit={handleSubmit}>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="identifier">
                      Email
                    </label>

                    <input
                      id="identifier"
                      type="text"
                      className="form-control form-control-lg"
                      placeholder="admin@smartsolar.lk"
                      value={identifier}
                      onChange={(event) => setIdentifier(event.target.value)}
                      required
                    />
                  </div>

                  <div className="mb-4">
                    <label className="form-label" htmlFor="password">
                      Password
                    </label>

                    <input
                      id="password"
                      type="password"
                      className="form-control form-control-lg"
                      placeholder="Enter password"
                      value={password}
                      onChange={(event) => setPassword(event.target.value)}
                      required
                    />
                  </div>

                  <button
                    type="submit"
                    className="btn btn-primary btn-lg w-100"
                    disabled={loading}
                  >
                    {loading ? "Signing in..." : "Sign In"}
                  </button>
                </form>

                <p className="small text-secondary text-center mt-4 mb-0">
                  Backoffice and Grid Operator access
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
