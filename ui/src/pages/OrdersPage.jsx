import { useEffect, useState } from "react";
import { myOrders } from "../api/orders";
import { tokenStore } from "../auth/token";
import { useNavigate } from "react-router-dom";

export default function OrdersPage() {
  const nav = useNavigate();
  const token = tokenStore.get();
  const [orders, setOrders] = useState([]);
  const [msg, setMsg] = useState("");

  async function load() {
    setMsg("");
    if (!token) return nav("/login");
    try {
      const data = await myOrders(token);
      setOrders(Array.isArray(data) ? data : []);
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  useEffect(() => { load(); }, []);

  return (
    <div style={{ padding: 16 }}>
      <h2>My Orders</h2>
      {msg && <p>{msg}</p>}

      <div style={{ display: "grid", gap: 10 }}>
        {orders.map((o) => (
          <div key={o.id} style={{ border: "1px solid #ddd", padding: 10, borderRadius: 8 }}>
            <b>Order:</b> {o.id} <br />
            <b>Status:</b> {o.status} <br />
            <b>Total:</b> {o.total}
          </div>
        ))}
      </div>
    </div>
  );
}
