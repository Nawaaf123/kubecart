// src/auth/auth.js
export function parseJwt(token) {
  try {
    const base64 = token.split(".")[1];
    const json = atob(base64.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(json);
  } catch {
    return null;
  }
}

export function getUserIdFromToken(token) {
  const p = parseJwt(token);
  return p?.sub ?? null;
}

export function getEmailFromToken(token) {
  const p = parseJwt(token);
  return p?.email ?? null;
}

// Role claim key is usually this URI in ASP.NET
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export function getRoleFromToken(token) {
  const p = parseJwt(token);
  return p?.[ROLE_CLAIM] ?? p?.role ?? null;
}

export function isAdmin(token) {
  const role = getRoleFromToken(token);
  return String(role).toLowerCase() === "admin";
}
