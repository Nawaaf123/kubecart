import { useEffect, useState } from "react";
import { listProducts } from "../api/catalog";
import { Link } from "react-router-dom";

export default function ProductsPage() {
  const [items, setItems] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const data = await listProducts();
        setItems(data);
      } catch (err) {
        setError(err.message || String(err));
      }
    })();
  }, []);

  if (error) {
    return <div>Error: {error}</div>;
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Products</h2>

      {items.length === 0 && <p>No products found</p>}

      {items.map((p) => (
        <div
          key={p.id}
          style={{ border: "1px solid #ccc", marginBottom: 8, padding: 8 }}
        >
          <b>{p.name}</b> — ${p.price}
          <br />
          <Link to={`/products/${p.id}`}>View</Link>
        </div>
      ))}
    </div>
  );
}
