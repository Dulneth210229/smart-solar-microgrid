import { useEffect, useState } from "react";

import { useNavigate, useParams } from "react-router-dom";

import api from "../services/api";
import { useAuth } from "../contexts/AuthContext";

const emptyForm = {
  startTime: "",
  endTime: "",
  capacitySlots: "",
};

export default function BookingSlotsPage() {
  const { stationId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();

  const isBackoffice = user?.role === "BACKOFFICE";

  const [station, setStation] = useState(null);

  const [slots, setSlots] = useState([]);

  const [form, setForm] = useState(emptyForm);

  const [editingId, setEditingId] = useState(null);

  const [showForm, setShowForm] = useState(false);

  const [error, setError] = useState("");

  const [message, setMessage] = useState("");

  const loadData = async () => {
    try {
      const [stationResponse, slotsResponse] = await Promise.all([
        api.get(`/Stations/${stationId}`),

        api.get(`/BookingSlots/manage/station/${stationId}`),
      ]);

      setStation(stationResponse.data);
      setSlots(slotsResponse.data);
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to load booking slots.",
      );
    }
  };

  useEffect(() => {
    loadData();
  }, [stationId]);

  const toLocalInputValue = (utcValue) => {
    const date = new Date(utcValue);

    const offset = date.getTimezoneOffset();

    const local = new Date(date.getTime() - offset * 60 * 1000);

    return local.toISOString().slice(0, 16);
  };

  const startCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setShowForm(true);
  };

  const startEdit = (slot) => {
    setEditingId(slot.id);

    setForm({
      startTime: toLocalInputValue(slot.startTimeUtc),

      endTime: toLocalInputValue(slot.endTimeUtc),

      capacitySlots: slot.capacitySlots,
    });

    setShowForm(true);
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    setForm((current) => ({
      ...current,
      [name]: value,
    }));
  };

  const saveSlot = async (event) => {
    event.preventDefault();

    setError("");
    setMessage("");

    const payload = {
      startTime: new Date(form.startTime).toISOString(),

      endTime: new Date(form.endTime).toISOString(),

      capacitySlots: Number(form.capacitySlots),
    };

    try {
      if (editingId) {
        await api.put(`/BookingSlots/${editingId}`, payload);

        setMessage("Booking slot updated successfully.");
      } else {
        await api.post("/BookingSlots", {
          stationId,
          ...payload,
        });

        setMessage("Booking slot created successfully.");
      }

      setShowForm(false);
      setEditingId(null);
      setForm(emptyForm);

      await loadData();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to save booking slot.",
      );
    }
  };

  const changeStatus = async (id, action) => {
    try {
      await api.patch(`/BookingSlots/${id}/${action}`);

      setMessage(`Booking slot ${action}d successfully.`);

      await loadData();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to update booking slot.",
      );
    }
  };

  const deleteSlot = async (id) => {
    const confirmed = window.confirm("Permanently delete this booking slot?");

    if (!confirmed) {
      return;
    }

    try {
      await api.delete(`/BookingSlots/${id}`);

      setMessage("Booking slot deleted successfully.");

      await loadData();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to delete booking slot.",
      );
    }
  };

  return (
    <>
      <button
        className="btn btn-link px-0 mb-3"
        onClick={() => navigate("/stations")}
      >
        <i className="bi bi-arrow-left me-2"></i>
        Back to Stations
      </button>

      <div className="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
        <div>
          <h2 className="fw-bold mb-1">Booking Slots</h2>

          <p className="text-secondary mb-0">
            {station?.name || "Solar Station"}
          </p>
        </div>

        <button className="btn btn-primary" onClick={startCreate}>
          <i className="bi bi-plus-lg me-2"></i>
          Add Slot
        </button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {message && <div className="alert alert-success">{message}</div>}

      {showForm && (
        <div className="card border-0 shadow-sm mb-4">
          <div className="card-body">
            <h5 className="fw-bold">
              {editingId ? "Edit Booking Slot" : "Create Booking Slot"}
            </h5>

            <form className="row g-3 mt-1" onSubmit={saveSlot}>
              <div className="col-md-5">
                <label className="form-label">Start Time</label>

                <input
                  type="datetime-local"
                  name="startTime"
                  className="form-control"
                  value={form.startTime}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-5">
                <label className="form-label">End Time</label>

                <input
                  type="datetime-local"
                  name="endTime"
                  className="form-control"
                  value={form.endTime}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-2">
                <label className="form-label">Capacity</label>

                <input
                  type="number"
                  min="1"
                  name="capacitySlots"
                  className="form-control"
                  value={form.capacitySlots}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-12">
                <button className="btn btn-success me-2">Save</button>

                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={() => setShowForm(false)}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <div className="card border-0 shadow-sm">
        <div className="card-body">
          <div className="table-responsive">
            <table className="table table-hover align-middle">
              <thead>
                <tr>
                  <th>Start</th>
                  <th>End</th>
                  <th>Capacity</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {slots.map((slot) => (
                  <tr key={slot.id}>
                    <td>{new Date(slot.startTimeUtc).toLocaleString()}</td>

                    <td>{new Date(slot.endTimeUtc).toLocaleString()}</td>

                    <td>{slot.capacitySlots}</td>

                    <td>
                      <span
                        className={`badge ${
                          slot.isActive
                            ? "text-bg-success"
                            : "text-bg-secondary"
                        }`}
                      >
                        {slot.isActive ? "ACTIVE" : "INACTIVE"}
                      </span>
                    </td>

                    <td>
                      <div className="d-flex gap-2 flex-wrap">
                        <button
                          className="btn btn-sm btn-outline-secondary"
                          onClick={() => startEdit(slot)}
                        >
                          Edit
                        </button>

                        {slot.isActive ? (
                          <button
                            className="btn btn-sm btn-outline-danger"
                            onClick={() => changeStatus(slot.id, "deactivate")}
                          >
                            Deactivate
                          </button>
                        ) : (
                          <button
                            className="btn btn-sm btn-success"
                            onClick={() => changeStatus(slot.id, "activate")}
                          >
                            Activate
                          </button>
                        )}

                        {isBackoffice && (
                          <button
                            className="btn btn-sm btn-danger"
                            onClick={() => deleteSlot(slot.id)}
                          >
                            Delete
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}

                {slots.length === 0 && (
                  <tr>
                    <td colSpan="5" className="text-center text-secondary py-4">
                      No booking slots found.
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
