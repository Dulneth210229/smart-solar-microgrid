import { useEffect, useState } from "react";

import api from "../services/api";

export default function ReservationsPage() {
  const [reservations, setReservations] = useState([]);

  const [selected, setSelected] = useState(null);

  const [error, setError] = useState("");

  const [message, setMessage] = useState("");

  const [loading, setLoading] = useState(true);

  const loadReservations = async () => {
    setLoading(true);

    try {
      const response = await api.get("/Reservations/pending");

      setReservations(response.data);
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to load reservations.",
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadReservations();
  }, []);

  const viewReservation = async (id) => {
    try {
      const response = await api.get(`/Reservations/${id}`);

      setSelected(response.data);
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to load reservation.",
      );
    }
  };

  const approve = async (id) => {
    setError("");
    setMessage("");

    try {
      await api.patch(`/Reservations/${id}/approve`);

      setMessage("Reservation approved successfully.");

      setSelected(null);

      await loadReservations();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to approve reservation.",
      );
    }
  };

  const cancel = async (id) => {
    const confirmed = window.confirm("Cancel this reservation?");

    if (!confirmed) {
      return;
    }

    setError("");
    setMessage("");

    try {
      await api.patch(`/Reservations/${id}/cancel`);

      setMessage("Reservation cancelled successfully.");

      setSelected(null);

      await loadReservations();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to cancel reservation.",
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
      <div className="mb-4">
        <h2 className="fw-bold mb-1">Reservations</h2>

        <p className="text-secondary mb-0">
          Review and process pending energy reservations.
        </p>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {message && <div className="alert alert-success">{message}</div>}

      {selected && (
        <div className="card border-0 shadow-sm mb-4">
          <div className="card-body">
            <div className="d-flex justify-content-between align-items-center mb-3">
              <h5 className="fw-bold mb-0">Reservation Details</h5>

              <button
                className="btn-close"
                onClick={() => setSelected(null)}
              ></button>
            </div>

            <div className="row g-3">
              <div className="col-md-4">
                <strong>Prosumer NIC</strong>
                <div>{selected.prosumerNic}</div>
              </div>

              <div className="col-md-4">
                <strong>Station</strong>
                <div>{selected.stationName}</div>
              </div>

              <div className="col-md-4">
                <strong>Status</strong>
                <div>{selected.status}</div>
              </div>

              <div className="col-md-4">
                <strong>Start</strong>
                <div>{new Date(selected.startTimeUtc).toLocaleString()}</div>
              </div>

              <div className="col-md-4">
                <strong>Energy</strong>
                <div>{selected.energyAmountKwh} kWh</div>
              </div>

              <div className="col-md-4">
                <strong>Transfer Type</strong>
                <div>{selected.transferType}</div>
              </div>
            </div>

            {selected.status === "PENDING" && (
              <div className="mt-4">
                <button
                  className="btn btn-success me-2"
                  onClick={() => approve(selected.id)}
                >
                  Approve
                </button>

                <button
                  className="btn btn-outline-danger"
                  onClick={() => cancel(selected.id)}
                >
                  Cancel
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      <div className="card border-0 shadow-sm">
        <div className="card-body">
          <h5 className="fw-bold mb-3">Pending Reservations</h5>

          <div className="table-responsive">
            <table className="table table-hover align-middle">
              <thead>
                <tr>
                  <th>Prosumer</th>
                  <th>Station</th>
                  <th>Date</th>
                  <th>Energy</th>
                  <th>Type</th>
                  <th>Action</th>
                </tr>
              </thead>

              <tbody>
                {reservations.map((reservation) => (
                  <tr key={reservation.id}>
                    <td>{reservation.prosumerNic}</td>

                    <td>{reservation.stationName}</td>

                    <td>
                      {new Date(reservation.startTimeUtc).toLocaleString()}
                    </td>

                    <td>{reservation.energyAmountKwh} kWh</td>

                    <td>{reservation.transferType}</td>

                    <td>
                      <button
                        className="btn btn-sm btn-primary"
                        onClick={() => viewReservation(reservation.id)}
                      >
                        Review
                      </button>
                    </td>
                  </tr>
                ))}

                {reservations.length === 0 && (
                  <tr>
                    <td colSpan="6" className="text-center py-4 text-secondary">
                      No pending reservations.
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
