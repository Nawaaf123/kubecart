// src/pages/AdminPage.jsx
import { useEffect, useMemo, useState } from "react";
import { tokenStore } from "../auth/token";
import { isAdmin, getEmailFromToken, getRoleFromToken } from "../auth/auth";
import {
  adminCreateProduct,
  adminDeleteProduct,
  adminListProducts,
  adminUpdateOrderStatus,
  adminUpdateProduct,
} from "../api/admin";

export default function AdminPage() {
  const token = tokenStore.get();

  const role = useMemo(() => (token ? getRoleFromToken(token) : null), [token]);
  const email = useMemo(() => (token ? getEmailFromToken(token) : null), [token]);

  const [msg, setMsg] = useState("");

  // Products list
  const [products, setProducts] = useState([]);
  const [loadingProducts, setLoadingProducts] = useState(false);

  // Create/Edit Product form
  const [editingId, setEditingId] = useState(null); // Guid string or null
  const [p, setP] = useState({
    categoryId: 1,
    name: "",
    description: "",
    price: 0,
    stockQuantity: 0,
  });

  // Update Order Status form
  const [orderId, setOrderId] = useState("");
  const [status, setStatus] = useState("Approved");

  if (!token) return <div>Please login as admin.</div>;
  if (!isAdmin(token))
    return (
      <div>
        <h2>Admin</h2>
        <p>Signed in as: {email}</p>
        <p>Role: {role}</p>
        <p style={{ marginTop: 10 }}>
          <b>Access denied.</b> (Admin only)
        </p>
      </div>
    );

  async function loadProducts() {
    setMsg("");
    setLoadingProducts(true);
    try {
      const list = await adminListProducts();
      setProducts(Array.isArray(list) ? list : []);
    } catch (err) {
      setMsg(err.message || String(err));
    } finally {
      setLoadingProducts(false);
    }
  }

  useEffect(() => {
    loadProducts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function resetForm() {
    setEditingId(null);
    setP({ categoryId: 1, name: "", description: "", price: 0, stockQuantity: 0 });
  }

  function onEditClick(row) {
    setMsg("");
    setEditingId(row.id);
    setP({
      categoryId: row.categoryId ?? 1,
      name: row.name ?? "",
      description: row.description ?? "", // list endpoint might not include description; ok
      price: row.price ?? 0,
      stockQuantity: row.stockQuantity ?? 0,
    });
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  async function onDeleteClick(row) {
    setMsg("");
    const ok = window.confirm(`Delete product "${row.name}"?`);
    if (!ok) return;

    try {
      await adminDeleteProduct(token, row.id);
      setMsg(`🗑️ Deleted: ${row.name}`);
      if (editingId === row.id) resetForm();
      await loadProducts();
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  async function onSubmitProduct(e) {
    e.preventDefault();
    setMsg("");

    const dto = {
      categoryId: Number(p.categoryId),
      name: String(p.name || "").trim(),
      description: p.description ? String(p.description).trim() : null,
      price: Number(p.price),
      stockQuantity: Number(p.stockQuantity),
    };

    if (!dto.name) return setMsg("Name is required.");
    if (!dto.categoryId || dto.categoryId <= 0) return setMsg("CategoryId must be valid.");
    if (Number.isNaN(dto.price) || dto.price < 0) return setMsg("Price must be >= 0.");
    if (Number.isNaN(dto.stockQuantity) || dto.stockQuantity < 0)
      return setMsg("StockQuantity must be >= 0.");

    try {
      if (editingId) {
        await adminUpdateProduct(token, editingId, dto);
        setMsg(`✅ Product updated. id = ${editingId}`);
      } else {
        const res = await adminCreateProduct(token, dto);
        setMsg(`✅ Product created. productId = ${res.productId}`);
      }

      resetForm();
      await loadProducts();
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  async function onUpdateStatus(e) {
    e.preventDefault();
    setMsg("");
    try {
      if (!orderId) return setMsg("OrderId is required.");
      const res = await adminUpdateOrderStatus(token, orderId, status);
      setMsg(`✅ Status updated. ${JSON.stringify(res)}`);
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  return (
    <div>
      <h2>Admin Dashboard</h2>
      <p>Signed in as: {email}</p>
      <p>Role: {role}</p>

      {msg && <p style={{ marginTop: 12 }}>{msg}</p>}

      <hr />

      <h3>{editingId ? "Edit Product" : "Create Product"}</h3>
      <form onSubmit={onSubmitProduct} style={{ display: "grid", gap: 8, maxWidth: 520 }}>
        <label>
          CategoryId
          <input
            value={p.categoryId}
            onChange={(e) => setP((x) => ({ ...x, categoryId: e.target.value }))}
          />
        </label>

        <label>
          Name
          <input value={p.name} onChange={(e) => setP((x) => ({ ...x, name: e.target.value }))} />
        </label>

        <label>
          Description
          <input
            value={p.description}
            onChange={(e) => setP((x) => ({ ...x, description: e.target.value }))}
          />
        </label>

        <label>
          Price
          <input
            type="number"
            step="0.01"
            value={p.price}
            onChange={(e) => setP((x) => ({ ...x, price: e.target.value }))}
          />
        </label>

        <label>
          StockQuantity
          <input
            type="number"
            value={p.stockQuantity}
            onChange={(e) => setP((x) => ({ ...x, stockQuantity: e.target.value }))}
          />
        </label>

        <div style={{ display: "flex", gap: 8 }}>
          <button type="submit">{editingId ? "Save Changes" : "Create Product"}</button>
          {editingId && (
            <button type="button" onClick={resetForm}>
              Cancel
            </button>
          )}
          <button type="button" onClick={loadProducts}>
            Refresh List
          </button>
        </div>

        {editingId && (
          <div style={{ fontSize: 12, opacity: 0.8 }}>
            Editing product id: <code>{editingId}</code>
          </div>
        )}
      </form>

      <hr />

      <h3>Products</h3>
      {loadingProducts ? (
        <div>Loading products...</div>
      ) : products.length === 0 ? (
        <div>No products found.</div>
      ) : (
        <div style={{ overflowX: "auto" }}>
          <table cellPadding="8" style={{ borderCollapse: "collapse", minWidth: 720 }}>
            <thead>
              <tr>
                <th align="left">Name</th>
                <th align="left">CategoryId</th>
                <th align="left">Price</th>
                <th align="left">Stock</th>
                <th align="left">Id</th>
                <th align="left">Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((row) => (
                <tr key={row.id} style={{ borderTop: "1px solid #ddd" }}>
                  <td>{row.name}</td>
                  <td>{row.categoryId}</td>
                  <td>${row.price}</td>
                  <td>{row.stockQuantity}</td>
                  <td style={{ fontSize: 12 }}>
                    <code>{row.id}</code>
                  </td>
                  <td>
                    <div style={{ display: "flex", gap: 8 }}>
                      <button type="button" onClick={() => onEditClick(row)}>
                        Edit
                      </button>
                      <button type="button" onClick={() => onDeleteClick(row)}>
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <p style={{ fontSize: 12, opacity: 0.8, marginTop: 8 }}>
            Note: list endpoint may not include Description; edit screen will show blank unless you
            add a “get product detail” load on edit (we can add that next if you want).
          </p>
        </div>
      )}

      <hr />

      <h3>Update Order Status</h3>
      <form onSubmit={onUpdateStatus} style={{ display: "grid", gap: 8, maxWidth: 520 }}>
        <label>
          OrderId (paste from Orders page)
          <input value={orderId} onChange={(e) => setOrderId(e.target.value)} />
        </label>

        <label>
          Status
          <select value={status} onChange={(e) => setStatus(e.target.value)}>
            <option value="Approved">Approved</option>
            <option value="Rejected">Rejected</option>
            <option value="Shipped">Shipped</option>
            <option value="Cancelled">Cancelled</option>
            <option value="Pending">Pending</option>
          </select>
        </label>

        <button type="submit">Update Status</button>
      </form>
    </div>
  );
}
