import { api } from './axiosInstance';
import { unwrap } from './response';
import type { ApiResponse, BuildCheck, BuildItem, SavedBuild } from '../types';

export const checkBuild = (items: BuildItem[]) =>
  api.post<ApiResponse<BuildCheck>>('/builds/check', { items }).then(unwrap);

export const getBuilds = () =>
  api.get<ApiResponse<SavedBuild[]>>('/builds').then(unwrap);

export const createBuild = (name: string, items: BuildItem[]) =>
  api.post<ApiResponse<SavedBuild>>('/builds', { name, items }).then(unwrap);

export const updateBuild = (id: string, name: string, items: BuildItem[]) =>
  api.put<ApiResponse<SavedBuild>>(`/builds/${id}`, { name, items }).then(unwrap);

export const deleteBuild = (id: string) =>
  api.delete<ApiResponse>(`/builds/${id}`);
