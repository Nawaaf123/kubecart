import { apiFetch } from "./http";

export function listProducts({ search, categoryId } = {}) {
  const params = new URLSearchParams();
  if (search) params.set("search", search);
  if (categoryId) params.set("categoryId", String(categoryId));
  const qs = params.toString();
  return apiFetch(`/api/catalog/products${qs ? `?${qs}` : ""}`);
}

export function getProduct(id) {
  return apiFetch(`/api/catalog/products/${id}`);
}
