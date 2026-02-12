// src/api/http.js
export async function apiFetch(path, { method = "GET", token, body } = {}) {
  const headers = { "Content-Type": "application/json" };

  // IMPORTANT: token must be raw JWT, NOT "Bearer xxx"
  if (token) {
    const clean = token.startsWith("Bearer ") ? token.slice(7) : token;
    headers["Authorization"] = `Bearer ${clean}`;
  }

  const res = await fetch(path, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  // If API returns non-json error text, don’t crash JSON.parse
  const text = await res.text();

  if (!res.ok) {
    throw new Error(text || `Request failed: ${res.status} ${res.statusText}`);
  }

  // If empty response
  if (!text) return null;

  // Try json, fallback to text
  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}
