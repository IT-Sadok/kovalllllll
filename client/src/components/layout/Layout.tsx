import React from 'react';
import { Link } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import Navbar from './Navbar';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
    <div className="min-h-screen flex flex-col" style={{ background: 'var(--color-bg)' }}>
      <Navbar />
      <main className="flex-1 pt-16">
        {children}
      </main>
      <footer className="border-t border-white/5 py-6 mt-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex flex-col sm:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-2">
            <span className="font-orbitron text-sm font-bold">
              DRONE<span className="text-cyan-400">BUILDER</span>
            </span>
          </div>
          <p className="text-xs text-slate-600">
            © {new Date().getFullYear()} DroneBuilder. Professional UAV solutions.
          </p>
          <div className="flex gap-4">
            <Link to="/" className="text-xs text-slate-600 hover:text-slate-400 transition-colors">Catalog</Link>
            <Link to="/orders" className="text-xs text-slate-600 hover:text-slate-400 transition-colors">Orders</Link>
          </div>
        </div>
      </footer>

      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            background: '#1a2234',
            border: '1px solid rgba(0,212,255,0.2)',
            color: '#e2e8f0',
            borderRadius: '12px',
            fontSize: '14px',
          },
          success: {
            iconTheme: { primary: '#00d4ff', secondary: '#000' },
          },
          error: {
            iconTheme: { primary: '#ef4444', secondary: '#fff' },
          },
        }}
      />
    </div>
  );
};

export default Layout;
