import { api } from './axiosInstance';
import { unwrap } from './response';
import type { ApiResponse, BuildCheck, BuildItem } from '../types';

export const checkBuild = (items: BuildItem[]) =>
  api.post<ApiResponse<BuildCheck>>('/builds/check', { items }).then(unwrap);
