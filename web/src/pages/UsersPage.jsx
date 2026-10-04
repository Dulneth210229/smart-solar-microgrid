import { useEffect, useState } from "react";

import api from "../services/api";

export default function UsersPage() {
  const [users, setUsers] = useState([]);
  const [pending, setPending] = useState([]);
  const [deactivationRequests, setDeactivationRequests] = useState([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const [showCreateForm, setShowCreateForm] = useState(false);

  const [staffForm, setStaffForm] = useState({
    fullName: "",
    email: "",
    phoneNumber: "",
    password: "",
    role: "GRID_OPERATOR",
  });

  const loadUsers = async () => {
    setLoading(true);
    setError("");

    try {
      const [usersResponse, pendingResponse, deactivationResponse] =
        await Promise.all([
          api.get("/Users"),
          api.get("/Users/pending-prosumers"),
          api.get("/Users/deactivation-requests"),
        ]);

      setUsers(usersResponse.data);
      setPending(pendingResponse.data);
      setDeactivationRequests(deactivationResponse.data);
    } catch (exception) {
      setError(exception.response?.data?.message || "Unable to load users.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const activateUser = async (id) => {
    setError("");
    setMessage("");

    try {
      await api.patch(`/Users/${id}/activate`);

      setMessage("User account activated successfully.");

      await loadUsers();
    } catch (exception) {
      setError(exception.response?.data?.message || "Unable to activate user.");
    }
  };

  const deactivateUser = async (id) => {
    const confirmed = window.confirm(
      "Are you sure you want to deactivate this account?",
    );

    if (!confirmed) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await api.patch(`/Users/${id}/deactivate`);

      setMessage("User account deactivated successfully.");

      await loadUsers();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to deactivate user.",
      );
    }
  };

  const handleStaffChange = (event) => {
    const { name, value } = event.target;

    setStaffForm((current) => ({
      ...current,
      [name]: value,
    }));
  };

  const createStaff = async (event) => {
    event.preventDefault();

    setError("");
    setMessage("");

    try {
      await api.post("/Users/staff", staffForm);

      setMessage("Staff account created successfully.");

      setStaffForm({
        fullName: "",
        email: "",
        phoneNumber: "",
        password: "",
        role: "GRID_OPERATOR",
      });

      setShowCreateForm(false);

      await loadUsers();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to create staff account.",
      );
    }
  };

  if (loading) {
    return (
      <div className="py-5 text-center">
        <div className="spinner-border"></div>
      </div>
    );
  }

  return (
    <>
      <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
        <div>
          <h2 className="fw-bold mb-1">User Management</h2>

          <p className="text-secondary mb-0">
            Manage Prosumers, Backoffice users and Grid Operators.
          </p>
        </div>

        <button
          className="btn btn-primary"
          onClick={() => setShowCreateForm((current) => !current)}
        >
          <i className="bi bi-person-plus me-2"></i>
          Create Staff User
        </button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {message && <div className="alert alert-success">{message}</div>}

      {showCreateForm && (
        <div className="card shadow-sm border-0 mb-4">
          <div className="card-body">
            <h5 className="fw-bold mb-3">Create Staff Account</h5>

            <form className="row g-3" onSubmit={createStaff}>
              <div className="col-md-6">
                <label className="form-label">Full Name</label>

                <input
                  name="fullName"
                  className="form-control"
                  value={staffForm.fullName}
                  onChange={handleStaffChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Email</label>

                <input
                  type="email"
                  name="email"
                  className="form-control"
                  value={staffForm.email}
                  onChange={handleStaffChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Phone Number</label>

                <input
                  name="phoneNumber"
                  className="form-control"
                  value={staffForm.phoneNumber}
                  onChange={handleStaffChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Password</label>

                <input
                  type="password"
                  name="password"
                  className="form-control"
                  value={staffForm.password}
                  onChange={handleStaffChange}
                  minLength="8"
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Role</label>

                <select
                  name="role"
                  className="form-select"
                  value={staffForm.role}
                  onChange={handleStaffChange}
                >
                  <option value="GRID_OPERATOR">Grid Operator</option>

                  <option value="BACKOFFICE">Backoffice</option>
                </select>
              </div>

              <div className="col-12">
                <button className="btn btn-success me-2">Create Account</button>

                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={() => setShowCreateForm(false)}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {pending.length > 0 && (
        <div className="card shadow-sm border-0 mb-4">
          <div className="card-body">
            <h5 className="fw-bold">Pending Prosumer Activations</h5>

            <div className="table-responsive">
              <table className="table align-middle">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>NIC</th>
                    <th>Email</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  {pending.map((user) => (
                    <tr key={user.id}>
                      <td>{user.fullName}</td>
                      <td>{user.nic}</td>
                      <td>{user.email}</td>
                      <td>
                        <button
                          className="btn btn-sm btn-success"
                          onClick={() => activateUser(user.id)}
                        >
                          Activate
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {deactivationRequests.length > 0 && (
        <div className="card shadow-sm border-0 mb-4">
          <div className="card-body">
            <h5 className="fw-bold">Deactivation Requests</h5>

            <div className="table-responsive">
              <table className="table align-middle">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>NIC</th>
                    <th>Email</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  {deactivationRequests.map((user) => (
                    <tr key={user.id}>
                      <td>{user.fullName}</td>
                      <td>{user.nic}</td>
                      <td>{user.email}</td>
                      <td>
                        <button
                          className="btn btn-sm btn-danger"
                          onClick={() => deactivateUser(user.id)}
                        >
                          Approve Deactivation
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      <div className="card shadow-sm border-0">
        <div className="card-body">
          <h5 className="fw-bold mb-3">All Users</h5>

          <div className="table-responsive">
            <table className="table table-hover align-middle">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>NIC</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {users.map((user) => (
                  <tr key={user.id}>
                    <td>{user.fullName}</td>

                    <td>{user.nic || "—"}</td>

                    <td>{user.email}</td>

                    <td>
                      <span className="badge text-bg-secondary">
                        {user.role}
                      </span>
                    </td>

                    <td>
                      <span
                        className={`badge ${
                          user.status === "ACTIVE"
                            ? "text-bg-success"
                            : user.status === "PENDING"
                              ? "text-bg-warning"
                              : "text-bg-secondary"
                        }`}
                      >
                        {user.status}
                      </span>
                    </td>

                    <td>
                      {user.status === "DEACTIVATED" ? (
                        <button
                          className="btn btn-sm btn-success"
                          onClick={() => activateUser(user.id)}
                        >
                          Reactivate
                        </button>
                      ) : user.status === "ACTIVE" ? (
                        <button
                          className="btn btn-sm btn-outline-danger"
                          onClick={() => deactivateUser(user.id)}
                        >
                          Deactivate
                        </button>
                      ) : null}
                    </td>
                  </tr>
                ))}

                {users.length === 0 && (
                  <tr>
                    <td colSpan="6" className="text-center text-secondary py-4">
                      No users found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </>
  );
}
