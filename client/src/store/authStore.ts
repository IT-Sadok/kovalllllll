import { create } from 'zustand';
import type { AuthUser, CurrentUser } from '../types';
import { getCurrentUser } from '../api/auth';

// The token now lives in an HttpOnly cookie, so the browser cannot read it and cannot tell whether
// it is signed in without asking the server. Hydration is therefore asynchronous, and anything that
// gates on authentication has to wait for 'loading' to resolve instead of assuming anonymous.
type AuthStatus = 'loading' | 'authenticated' | 'anonymous';

interface AuthState {
  user: AuthUser | null;
  status: AuthStatus;
  hydrate: () => Promise<void>;
  clear: () => void;
}

function toAuthUser(current: CurrentUser): AuthUser {
  return {
    id: current.id,
    email: current.email,
    role: current.roles.includes('Admin') ? 'Admin' : 'User',
  };
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  status: 'loading',

  hydrate: async () => {
    try {
      const current = await getCurrentUser();
      set({ user: toAuthUser(current), status: 'authenticated' });
    } catch {
      // A 401 here is the normal "not signed in" answer, not a failure worth surfacing.
      set({ user: null, status: 'anonymous' });
    }
  },

  clear: () => set({ user: null, status: 'anonymous' }),
}));
