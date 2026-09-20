import { api } from './axiosInstance';
import type { CurrentUser } from '../types';

export const signUp = (email: string, password: string) =>
  api.post('/users/sign-up', { email, password });

// Returns no body: the access token is set as an HttpOnly cookie the browser cannot read.
export const signIn = (email: string, password: string) =>
  api.post('/users/sign-in', { email, password });

// Only the server can clear an HttpOnly cookie, so signing out is a request rather than a local wipe.
export const signOut = () => api.post('/users/sign-out');

export const getCurrentUser = () =>
  api.get<CurrentUser>('/users/me').then((r) => r.data);
