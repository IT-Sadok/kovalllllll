import { api } from './axiosInstance';
import { unwrap, unwrapPaged } from './response';
import type { ApiResponse, ComponentType, Product } from '../types';

export const getProducts = (filters: {
  page?: number;
  pageSize?: number;
  name?: string;
  minPrice?: number | '';
  maxPrice?: number | '';
  category?: string;
  componentType?: ComponentType;
} = {}) => {
  const params = new URLSearchParams();
  params.set('page', String(filters.page ?? 1));
  params.set('pageSize', String(filters.pageSize ?? 20));
  if (filters.name) params.set('name', filters.name);
  if (filters.minPrice !== undefined && filters.minPrice !== '') params.set('minPrice', String(filters.minPrice));
  if (filters.maxPrice !== undefined && filters.maxPrice !== '') params.set('maxPrice', String(filters.maxPrice));
  if (filters.category) params.set('category', filters.category);
  if (filters.componentType) params.set('componentType', filters.componentType);
  return api.get<ApiResponse<Product[]>>(`/products?${params.toString()}`).then(unwrapPaged);
};

export const getProduct = (id: string) =>
  api.get<ApiResponse<Product>>(`/products/${id}`).then(unwrap);

export const getCategories = () => api.get<ApiResponse<string[]>>('/products/categories').then(unwrap);
