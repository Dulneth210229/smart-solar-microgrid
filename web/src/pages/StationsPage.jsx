import { useEffect, useState } from "react";

import { useNavigate } from "react-router-dom";

import api from "../services/api";
import { useAuth } from "../contexts/AuthContext";

const emptyForm = {
  name: "",
  location: "",
  latitude: "",
  longitude: "",
  capacityKw: "",
  batteryCapacityKwh: "",
  totalBatterySlots: "",
  openingTime: "08:00",
  closingTime: "18:00",
  operatingDays: [],
};

const dayOptions = [
  "MONDAY",
  "TUESDAY",
  "WEDNESDAY",
  "THURSDAY",
  "FRIDAY",
  "SATURDAY",
  "SUNDAY",
];

export default function StationsPage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const isBackoffice = user?.role === "BACKOFFICE";

  const [stations, setStations] = useState([]);

  const [form, setForm] = useState(emptyForm);

  const [editingId, setEditingId] = useState(null);

  const [showForm, setShowForm] = useState(false);

  const [error, setError] = useState("");

  const [message, setMessage] = useState("");

  const [loading, setLoading] = useState(true);

  const loadStations = async () => {
    setLoading(true);

    try {
      const endpoint = isBackoffice ? "/Stations/manage" : "/Stations";

      const response = await api.get(endpoint);

      setStations(response.data);
    } catch (exception) {
      setError(exception.response?.data?.message || "Unable to load stations.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStations();
  }, [isBackoffice]);

  const handleChange = (event) => {
    const { name, value } = event.target;

    setForm((current) => ({
      ...current,
      [name]: value,
    }));
  };

  const toggleDay = (day) => {
    setForm((current) => {
      const exists = current.operatingDays.includes(day);

      return {
        ...current,
        operatingDays: exists
          ? current.operatingDays.filter((item) => item !== day)
          : [...current.operatingDays, day],
      };
    });
  };

  const startCreate = () => {
    setEditingId(null);
    setForm(emptyForm);
    setShowForm(true);
  };

  const startEdit = (station) => {
    setEditingId(station.id);

    setForm({
      name: station.name,
      location: station.location,
      latitude: station.latitude,
      longitude: station.longitude,
      capacityKw: station.capacityKw,
      batteryCapacityKwh: station.batteryCapacityKwh,
      totalBatterySlots: station.totalBatterySlots,
      openingTime: station.openingTime,
      closingTime: station.closingTime,
      operatingDays: station.operatingDays || [],
    });

    setShowForm(true);
  };

  const saveStation = async (event) => {
    event.preventDefault();

    setError("");
    setMessage("");

    const payload = {
      name: form.name,
      location: form.location,
      latitude: Number(form.latitude),
      longitude: Number(form.longitude),
      capacityKw: Number(form.capacityKw),
      batteryCapacityKwh: Number(form.batteryCapacityKwh),
      totalBatterySlots: Number(form.totalBatterySlots),
      openingTime: form.openingTime,
      closingTime: form.closingTime,
      operatingDays: form.operatingDays,
    };

    try {
      if (editingId) {
        await api.put(`/Stations/${editingId}`, payload);

        setMessage("Station updated successfully.");
      } else {
        await api.post("/Stations", payload);

        setMessage("Station created successfully.");
      }

      setShowForm(false);
      setEditingId(null);
      setForm(emptyForm);

      await loadStations();
    } catch (exception) {
      setError(exception.response?.data?.message || "Unable to save station.");
    }
  };

  const changeStatus = async (id, action) => {
    try {
      await api.patch(`/Stations/${id}/${action}`);

      setMessage(`Station ${action}d successfully.`);

      await loadStations();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to update station status.",
      );
    }
  };

  const updateAvailability = async (station) => {
    const value = window.prompt(
      `Enter available battery slots (0 - ${station.totalBatterySlots})`,
      station.availableBatterySlots,
    );

    if (value === null) {
      return;
    }

    try {
      await api.patch(`/Stations/${station.id}/availability`, {
        availableBatterySlots: Number(value),
      });

      setMessage("Station availability updated.");

      await loadStations();
    } catch (exception) {
      setError(
        exception.response?.data?.message || "Unable to update availability.",
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
      <div className="d-flex justify-content-between align-items-center flex-wrap gap-3 mb-4">
        <div>
          <h2 className="fw-bold mb-1">Solar Stations</h2>

          <p className="text-secondary mb-0">
            Manage microgrid nodes, schedules and battery availability.
          </p>
        </div>

        {isBackoffice && (
          <button className="btn btn-primary" onClick={startCreate}>
            <i className="bi bi-plus-lg me-2"></i>
            Add Station
          </button>
        )}
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {message && <div className="alert alert-success">{message}</div>}

      {showForm && isBackoffice && (
        <div className="card border-0 shadow-sm mb-4">
          <div className="card-body">
            <h5 className="fw-bold mb-3">
              {editingId ? "Edit Station" : "Create Station"}
            </h5>

            <form className="row g-3" onSubmit={saveStation}>
              <div className="col-md-6">
                <label className="form-label">Station Name</label>

                <input
                  className="form-control"
                  name="name"
                  value={form.name}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Location</label>

                <input
                  className="form-control"
                  name="location"
                  value={form.location}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Latitude</label>

                <input
                  type="number"
                  step="any"
                  className="form-control"
                  name="latitude"
                  value={form.latitude}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Longitude</label>

                <input
                  type="number"
                  step="any"
                  className="form-control"
                  name="longitude"
                  value={form.longitude}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-4">
                <label className="form-label">Capacity (kW)</label>

                <input
                  type="number"
                  step="any"
                  className="form-control"
                  name="capacityKw"
                  value={form.capacityKw}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-4">
                <label className="form-label">Battery Capacity (kWh)</label>

                <input
                  type="number"
                  step="any"
                  className="form-control"
                  name="batteryCapacityKwh"
                  value={form.batteryCapacityKwh}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-4">
                <label className="form-label">Battery Slots</label>

                <input
                  type="number"
                  className="form-control"
                  name="totalBatterySlots"
                  value={form.totalBatterySlots}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Opening Time</label>

                <input
                  type="time"
                  className="form-control"
                  name="openingTime"
                  value={form.openingTime}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-md-6">
                <label className="form-label">Closing Time</label>

                <input
                  type="time"
                  className="form-control"
                  name="closingTime"
                  value={form.closingTime}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="col-12">
                <label className="form-label">Operating Days</label>

                <div className="d-flex flex-wrap gap-2">
                  {dayOptions.map((day) => (
                    <label key={day} className="border rounded px-3 py-2">
                      <input
                        type="checkbox"
                        className="form-check-input me-2"
                        checked={form.operatingDays.includes(day)}
                        onChange={() => toggleDay(day)}
                      />

                      {day}
                    </label>
                  ))}
                </div>
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
                  <th>Station</th>
                  <th>Location</th>
                  <th>Slots</th>
                  <th>Schedule</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>

              <tbody>
                {stations.map((station) => (
                  <tr key={station.id}>
                    <td>
                      <div className="fw-semibold">{station.name}</div>

                      <small className="text-secondary">
                        {station.capacityKw} kW
                      </small>
                    </td>

                    <td>{station.location}</td>

                    <td>
                      {station.availableBatterySlots}/
                      {station.totalBatterySlots}
                    </td>

                    <td>
                      {station.openingTime}
                      {" - "}
                      {station.closingTime}
                    </td>

                    <td>
                      <span
                        className={`badge ${
                          station.isActive
                            ? "text-bg-success"
                            : "text-bg-secondary"
                        }`}
                      >
                        {station.isActive ? "ACTIVE" : "INACTIVE"}
                      </span>
                    </td>

                    <td>
                      <div className="d-flex flex-wrap gap-2">
                        <button
                          className="btn btn-sm btn-outline-primary"
                          onClick={() =>
                            navigate(`/stations/${station.id}/slots`)
                          }
                        >
                          Slots
                        </button>

                        {station.isActive && (
                          <button
                            className="btn btn-sm btn-outline-info"
                            onClick={() => updateAvailability(station)}
                          >
                            Availability
                          </button>
                        )}

                        {isBackoffice && (
                          <>
                            <button
                              className="btn btn-sm btn-outline-secondary"
                              onClick={() => startEdit(station)}
                            >
                              Edit
                            </button>

                            {station.isActive ? (
                              <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() =>
                                  changeStatus(station.id, "deactivate")
                                }
                              >
                                Deactivate
                              </button>
                            ) : (
                              <button
                                className="btn btn-sm btn-success"
                                onClick={() =>
                                  changeStatus(station.id, "activate")
                                }
                              >
                                Activate
                              </button>
                            )}
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}

                {stations.length === 0 && (
                  <tr>
                    <td colSpan="6" className="text-center py-4 text-secondary">
                      No stations found.
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
