import { createContext, useContext, useState } from "react";
import api from "../services/api";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const savedUser = localStorage.getItem("smartSolarUser");

    return savedUser ? JSON.parse(savedUser) : null;
  });

  const login = async (identifier, password) => {
    const response = await api.post("/Auth/login", {
      identifier,
      password,
    });

    const authData = response.data;

    // The web application is intended only for staff roles.
    if (authData.role !== "BACKOFFICE" && authData.role !== "GRID_OPERATOR") {
      throw new Error(
        "This web application is available only to Backoffice and Grid Operator users.",
      );
    }

    localStorage.setItem("smartSolarToken", authData.token);

    const userData = {
      userId: authData.userId,
      fullName: authData.fullName,
      email: authData.email,
      role: authData.role,
      status: authData.status,
    };

    localStorage.setItem("smartSolarUser", JSON.stringify(userData));

    setUser(userData);

    return userData;
  };

  const logout = () => {
    localStorage.removeItem("smartSolarToken");
    localStorage.removeItem("smartSolarUser");

    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        logout,
        isAuthenticated: Boolean(user),
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
