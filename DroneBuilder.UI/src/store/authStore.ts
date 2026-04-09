import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';
import type { AuthUser, DecodedToken } from '../types';

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  login: (token: string) => void;
  logout: () => void;
}

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const ID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

function decodeUser(token: string): AuthUser | null {
  try {
    const decoded = jwtDecode<DecodedToken>(token);
    const role = decoded[ROLE_CLAIM] as string | undefined;
    const id = decoded[ID_CLAIM] || decoded.sub || '';
    const email = decoded.email || '';
    return {
      id,
      email,
      role: (role === 'Admin' ? 'Admin' : 'User') as AuthUser['role'],
    };
  } catch {
    return null;
  }
}

// Re-hydrate from localStorage on page load
const savedToken = localStorage.getItem('token');

export const useAuthStore = create<AuthState>((set) => ({
  token: savedToken,
  user: savedToken ? decodeUser(savedToken) : null,

  login: (token: string) => {
    localStorage.setItem('token', token);
    set({ token, user: decodeUser(token) });
  },

  logout: () => {
    localStorage.removeItem('token');
    set({ token: null, user: null });
  },
}));
