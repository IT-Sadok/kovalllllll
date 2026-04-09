import { api } from './axiosInstance';
import type { AuthResponse } from '../types';

export const signUp = (email: string, password: string) =>
  api.post('/users/sign-up', { email, password });

export const signIn = (email: string, password: string) =>
  api.post<AuthResponse>('/users/sign-in', { email, password }).then((r) => r.data);
