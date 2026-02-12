import { apiFetch } from "./http";

export function getCart(token) {
  return apiFetch("/api/orders/cart", { token });
}

export function addToCart(token, productId, quantity) {
  return apiFetch("/api/orders/cart/items", {
    method: "POST",
    token,
    body: { productId, quantity },
  });
}

export function checkout(token, tax = 0, shipping = 0) {
  return apiFetch("/api/orders/checkout", {
    method: "POST",
    token,
    body: { tax, shipping },
  });
}

export function myOrders(token) {
  return apiFetch("/api/orders", { token });
}

export function myOrder(token, id) {
  return apiFetch(`/api/orders/${id}`, { token });
}


export function listMyOrders(token) {
  return apiFetch("/api/orders", { token });
}

