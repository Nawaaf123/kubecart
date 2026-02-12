import { Link, useNavigate } from "react-router-dom";
import { tokenStore } from "../auth/token";
import { getRoleFromToken } from "../auth/auth";

export default function Navbar() {
  const nav = useNavigate();
  const token = tokenStore.get();
  const role = token ? getRoleFromToken(token) : null;

  function logout() {
    tokenStore.clear();
    nav("/login");
  }

  return (
    <header className="nav">
      <div className="nav-inner">
        {/* Brand */}
        <Link to="/products" className="brand">
          KubeCart
        </Link>

        {/* Left links */}
        <nav className="nav-links">
          <Link to="/products">Products</Link>
          <Link to="/cart">Cart</Link>
          <Link to="/orders">Orders</Link>

          {role === "Admin" && (
            <Link to="/admin" className="pill">
              Admin
            </Link>
          )}
        </nav>

        {/* Right actions */}
        <div className="nav-actions">
          {!token ? (
            <Link to="/login" className="btn">
              Login
            </Link>
          ) : (
            <button className="btn danger" onClick={logout}>
              Logout
            </button>
          )}
        </div>
      </div>
    </header>
  );
}
