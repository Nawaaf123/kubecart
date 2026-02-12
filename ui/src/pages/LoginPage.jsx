import { useState } from "react";
import { login, me } from "../api/auth";
import { tokenStore } from "../auth/token";
import { useNavigate } from "react-router-dom";

export default function LoginPage() {
  const nav = useNavigate();
  const [email, setEmail] = useState("admin@test.com");
  const [password, setPassword] = useState("Pass@123");
  const [msg, setMsg] = useState("");

  async function onLogin(e) {
    e.preventDefault();
    setMsg("");
    try {
      const res = await login(email, password); // { token }
      tokenStore.set(res.token);

      // quick check token works
      await me(res.token);

      nav("/products");
    } catch (err) {
      setMsg(err.message || String(err));
    }
  }

  return (
    <div style={{ padding: 16 }}>
      <h2>Login</h2>
      <form onSubmit={onLogin} style={{ display: "grid", gap: 10, maxWidth: 360 }}>
        <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="email" />
        <input value={password} onChange={(e) => setPassword(e.target.value)} placeholder="password" type="password" />
        <button type="submit">Login</button>
      </form>
      {msg && <p style={{ marginTop: 10 }}>{msg}</p>}
    </div>
  );
}
