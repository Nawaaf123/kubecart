import { useEffect, useState } from "react";
import { getCart, checkout } from "../api/orders";
import { tokenStore } from "../auth/token";
import { useNavigate } from "react-router-dom";

export default function CartPage() {
  const nav = useNavigate();
  const token = tokenStore.get();
  const [items, setItems] = useState([]);
  const [msg, setMsg] = useState("");

  async function load() {
    setMsg("");
    if (!token) return nav("/login");
    try {
      const data = await getCart(token);
      setItems(Array.isArray(data) ? data : []);
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  useEffect(() => { load(); }, []);

  async function onCheckout() {
    setMsg("");
    try {
      const res = await checkout(token, 0, 0);
      setMsg(`Order created: ${res.orderId} | Total: ${res.total}`);
      nav("/orders");
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Cart</h2>
      {msg && <p>{msg}</p>}

      {items.length === 0 ? (
        <p>Cart is empty.</p>
      ) : (
        <>
          <ul>
            {items.map((x) => (
              <li key={x.productId}>
                {x.productId} — qty: {x.quantity}
              </li>
            ))}
          </ul>
          <button onClick={onCheckout}>Checkout</button>
        </>
      )}
    </div>
  );
}
