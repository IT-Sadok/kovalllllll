import { api } from './axiosInstance';
import type { Product, PagedResult } from '../types';

export const getProducts = (filters: {
  page?: number;
  pageSize?: number;
  name?: string;
  minPrice?: number | '';
  maxPrice?: number | '';
  category?: string;
} = {}) => {
  const params = new URLSearchParams();
  params.set('page', String(filters.page ?? 1));
  params.set('pageSize', String(filters.pageSize ?? 20));
  if (filters.name) params.set('name', filters.name);
  if (filters.minPrice !== undefined && filters.minPrice !== '') params.set('minPrice', String(filters.minPrice));
  if (filters.maxPrice !== undefined && filters.maxPrice !== '') params.set('maxPrice', String(filters.maxPrice));
  if (filters.category) params.set('category', filters.category);
  return api.get<PagedResult<Product>>(`/products?${params.toString()}`).then((r) => r.data);
};

export const getProduct = (id: string) =>
  api.get<Product>(`/products/${id}`).then((r) => r.data);

// GET /products/{productId}/properties → returns ProductPropertiesResponse, extract the properties array
export const getProductProperties = (id: string) =>
  api.get<import('../types').ProductPropertiesResponse>(`/products/${id}/properties`).then((r) => r.data.properties);

export const getCategories = () => api.get<string[]>('/products/categories').then(r => r.data);
