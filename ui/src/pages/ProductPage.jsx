import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { getProduct } from "../api/catalog";
import { addToCart } from "../api/orders";
import { tokenStore } from "../auth/token";

export default function ProductPage() {
  const { id } = useParams();
  const nav = useNavigate();
  const token = tokenStore.get();

  const [p, setP] = useState(null);
  const [qty, setQty] = useState(1);
  const [msg, setMsg] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const data = await getProduct(id);
        setP(data);
      } catch (err) {
        setMsg(err.message || String(err));
      }
    })();
  }, [id]);

  async function onAdd() {
    setMsg("");
    if (!token) return nav("/login");

    try {
      await addToCart(token, id, Number(qty));
      nav("/cart");
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  if (!p) return <div style={{ padding: 16 }}>{msg || "Loading..."}</div>;

  return (
    <div style={{ padding: 16 }}>
      <Link to="/products">← back</Link>
      <h2 style={{ marginTop: 10 }}>{p.name}</h2>
      <p>{p.description}</p>
      <p>
        <b>Price:</b> ${p.price} | <b>Stock:</b> {p.stockQuantity}
      </p>

      <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
        <input
          type="number"
          min="1"
          value={qty}
          onChange={(e) => setQty(e.target.value)}
          style={{ width: 80 }}
        />
        <button onClick={onAdd}>Add to cart</button>
      </div>

      {msg && <p style={{ marginTop: 10 }}>{msg}</p>}
    </div>
  );
}
