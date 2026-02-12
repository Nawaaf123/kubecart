// src/api/admin.js
import { apiFetch } from "./http";

// ---------- Catalog Admin Products ----------
export function adminCreateProduct(token, dto) {
  return apiFetch("/api/catalog/admin/products", {
    method: "POST",
    token,
    body: dto,
  });
}

export function adminUpdateProduct(token, id, dto) {
  return apiFetch(`/api/catalog/admin/products/${id}`, {
    method: "PUT",
    token,
    body: dto,
  });
}

export function adminDeleteProduct(token, id) {
  return apiFetch(`/api/catalog/admin/products/${id}`, {
    method: "DELETE",
    token,
  });
}

// Public list (for admin list UI)
export function adminListProducts() {
  // public endpoint in your backend
  return apiFetch("/api/catalog/products");
}

// ---------- Orders Admin ----------
export function adminUpdateOrderStatus(token, orderId, status) {
  return apiFetch(`/api/orders/admin/${orderId}/status`, {
    method: "PUT",
    token,
    body: { status },
  });
}
