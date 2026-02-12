import { apiFetch } from "./http";

// POST http://localhost:5001/api/auth/register
export function register(email, password, role = "Customer") {
  return apiFetch("/api/auth/register", {
    method: "POST",
    body: { email, password, role },
  });
}

// POST http://localhost:5001/api/auth/login
// returns: { token: "..." }
export async function login(email, password) {
  const res = await apiFetch("/api/auth/login", {
    method: "POST",
    body: { email, password },
  });

  // res should be { token }
  if (!res?.token) {
    throw new Error("Login succeeded but no token returned from API.");
  }

  return res;
}

// GET http://localhost:5001/api/auth/me  (requires Authorization header)
// returns: { userId, email, role }
export function me(token) {
  return apiFetch("/api/auth/me", { token });
}
