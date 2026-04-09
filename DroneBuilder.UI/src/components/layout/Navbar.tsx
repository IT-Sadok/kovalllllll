import React, { useState, useEffect } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { useCartStore } from '../../store/cartStore';
import { getCartItems } from '../../api/cart';

const DroneIcon = () => (
  <svg className="w-7 h-7" viewBox="0 0 32 32" fill="none">
    <circle cx="16" cy="16" r="4" fill="#00d4ff" />
    <circle cx="16" cy="16" r="6" stroke="#00d4ff" strokeWidth="1" strokeOpacity="0.4" fill="none" />
    {/* Arms */}
    <line x1="16" y1="10" x2="8" y2="4" stroke="#00d4ff" strokeWidth="1.5" />
    <line x1="16" y1="10" x2="24" y2="4" stroke="#00d4ff" strokeWidth="1.5" />
    <line x1="16" y1="22" x2="8" y2="28" stroke="#00d4ff" strokeWidth="1.5" />
    <line x1="16" y1="22" x2="24" y2="28" stroke="#00d4ff" strokeWidth="1.5" />
    {/* Rotors */}
    <ellipse cx="8" cy="4" rx="6" ry="2" stroke="#00d4ff" strokeWidth="1.2" strokeOpacity="0.7" fill="none" />
    <ellipse cx="24" cy="4" rx="6" ry="2" stroke="#00d4ff" strokeWidth="1.2" strokeOpacity="0.7" fill="none" />
    <ellipse cx="8" cy="28" rx="6" ry="2" stroke="#00d4ff" strokeWidth="1.2" strokeOpacity="0.7" fill="none" />
    <ellipse cx="24" cy="28" rx="6" ry="2" stroke="#00d4ff" strokeWidth="1.2" strokeOpacity="0.7" fill="none" />
  </svg>
);

const CartIcon = ({ count }: { count: number }) => (
  <div className="relative">
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
        d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
      />
    </svg>
    {count > 0 && (
      <span className="absolute -top-2 -right-2 w-4 h-4 bg-cyan-400 text-black text-[10px] font-bold rounded-full flex items-center justify-center animate-pulse-glow">
        {count > 9 ? '9+' : count}
      </span>
    )}
  </div>
);

const Navbar: React.FC = () => {
  const { user, logout } = useAuthStore();
  const { itemCount, setItemCount } = useCartStore();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);
  const [scrolled, setScrolled] = useState(false);

  useEffect(() => {
    const handler = () => setScrolled(window.scrollY > 20);
    window.addEventListener('scroll', handler);
    return () => window.removeEventListener('scroll', handler);
  }, []);

  // Fetch cart count on mount if logged in
  useEffect(() => {
    if (user) {
      getCartItems()
        .then((items) => setItemCount(items.length))
        .catch(() => {});
    }
  }, [user, setItemCount]);

  const handleLogout = () => {
    logout();
    navigate('/');
    setMenuOpen(false);
  };

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `text-sm font-medium transition-colors duration-200 ${
      isActive ? 'text-cyan-400' : 'text-slate-400 hover:text-white'
    }`;

  return (
    <header
      className={`fixed top-0 left-0 right-0 z-40 transition-all duration-300 ${
        scrolled
          ? 'bg-[#080b14]/90 backdrop-blur-xl border-b border-[rgba(0,212,255,0.08)]'
          : 'bg-transparent'
      }`}
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link to="/" id="nav-logo" className="flex items-center gap-2 group">
            <DroneIcon />
            <span className="font-orbitron font-bold text-lg text-white group-hover:text-cyan-400 transition-colors">
              DRONE<span className="text-cyan-400">BUILDER</span>
            </span>
          </Link>

          {/* Desktop Nav */}
          <nav className="hidden md:flex items-center gap-6">
            <NavLink to="/" id="nav-home" className={navLinkClass} end>Catalog</NavLink>
            {user?.role === 'Admin' && (
              <>
                <NavLink to="/admin/products" id="nav-admin-products" className={navLinkClass}>
                  Products
                </NavLink>
                <NavLink to="/admin/warehouse" id="nav-admin-warehouse" className={navLinkClass}>
                  Warehouse
                </NavLink>
                <NavLink to="/admin/properties" id="nav-admin-properties" className={navLinkClass}>
                  Properties
                </NavLink>
                <NavLink to="/admin/values" id="nav-admin-values" className={navLinkClass}>
                  Values
                </NavLink>
                <NavLink to="/admin/orders" id="nav-admin-orders" className={navLinkClass}>
                  Manage Orders
                </NavLink>
              </>
            )}
          </nav>

          {/* Right side */}
          <div className="flex items-center gap-3">
            {user ? (
              <>
                <NavLink
                  to="/cart"
                  id="nav-cart"
                  className="p-2 text-slate-400 hover:text-cyan-400 transition-colors"
                >
                  <CartIcon count={itemCount} />
                </NavLink>
                <NavLink
                  to="/orders"
                  id="nav-orders"
                  className={({ isActive }) =>
                    `hidden sm:block text-sm font-medium transition-colors ${
                      isActive ? 'text-cyan-400' : 'text-slate-400 hover:text-white'
                    }`
                  }
                >
                  Orders
                </NavLink>
                <div className="flex items-center gap-2 pl-2 border-l border-white/10">
                  <span className="hidden sm:block text-xs text-slate-500 max-w-[120px] truncate">
                    {user.email}
                  </span>
                  {user.role === 'Admin' && (
                    <span className="text-xs px-2 py-0.5 rounded-full bg-violet-500/20 text-violet-400 border border-violet-500/30 font-medium">
                      Admin
                    </span>
                  )}
                  <button
                    onClick={handleLogout}
                    id="nav-logout"
                    className="text-sm text-slate-400 hover:text-red-400 transition-colors cursor-pointer"
                  >
                    Logout
                  </button>
                </div>
              </>
            ) : (
              <>
                <Link
                  to="/login"
                  id="nav-login"
                  className="text-sm text-slate-400 hover:text-white transition-colors"
                >
                  Sign In
                </Link>
                <Link
                  to="/register"
                  id="nav-register"
                  className="text-sm px-4 py-1.5 rounded-lg bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 hover:bg-cyan-500/20 transition-all duration-200"
                >
                  Register
                </Link>
              </>
            )}

            {/* Mobile hamburger */}
            <button
              className="md:hidden text-slate-400 hover:text-white transition-colors"
              onClick={() => setMenuOpen(!menuOpen)}
              id="nav-mobile-toggle"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                {menuOpen ? (
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                ) : (
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                )}
              </svg>
            </button>
          </div>
        </div>

        {/* Mobile menu */}
        {menuOpen && (
          <div className="md:hidden border-t border-white/5 py-4 space-y-2">
            <NavLink to="/" className={navLinkClass} onClick={() => setMenuOpen(false)} end>
              <div className="py-2">Catalog</div>
            </NavLink>
            {user && (
              <>
                <NavLink to="/cart" className={navLinkClass} onClick={() => setMenuOpen(false)}>
                  <div className="py-2">Cart ({itemCount})</div>
                </NavLink>
                <NavLink to="/orders" className={navLinkClass} onClick={() => setMenuOpen(false)}>
                  <div className="py-2">Orders</div>
                </NavLink>
              </>
            )}
            {user?.role === 'Admin' && (
              <>
                <NavLink to="/admin/products" className={navLinkClass} onClick={() => setMenuOpen(false)}>
                  <div className="py-2">Admin Products</div>
                </NavLink>
                <NavLink to="/admin/warehouse" className={navLinkClass} onClick={() => setMenuOpen(false)}>
                  <div className="py-2">Warehouse</div>
                </NavLink>
                <NavLink to="/admin/properties" className={navLinkClass} onClick={() => setMenuOpen(false)}>
                  <div className="py-2">Properties</div>
                </NavLink>
                <NavLink to="/admin/values" className={navLinkClass} onClick={() => setMenuOpen(false)}>
                  <div className="py-2">Values</div>
                </NavLink>
                <NavLink to="/admin/orders" className={navLinkClass} onClick={() => setMenuOpen(false)}>
                  <div className="py-2">Manage Orders</div>
                </NavLink>
              </>
            )}
          </div>
        )}
      </div>
    </header>
  );
};

export default Navbar;
