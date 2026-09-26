import { api } from './axiosInstance';
import { unwrap, unwrapPaged } from './response';
import type { ApiResponse, Product, ProductCategory, ProductFilters } from '../types';

export const getProducts = (filters: ProductFilters = {}) => {
  const params = new URLSearchParams();
  params.set('page', String(filters.page ?? 1));
  params.set('pageSize', String(filters.pageSize ?? 20));
  for (const [key, value] of Object.entries(filters)) {
    if (key === 'page' || key === 'pageSize' || value === undefined || value === '' || value === false) continue;
    params.set(key, String(value));
  }
  return api.get<ApiResponse<Product[]>>(`/products?${params.toString()}`).then(unwrapPaged);
};

export const getProduct = (id: string) =>
  api.get<ApiResponse<Product>>(`/products/${id}`).then(unwrap);

export const getManufacturers = (category?: ProductCategory) =>
  api.get<ApiResponse<string[]>>('/products/manufacturers', { params: { category } }).then(unwrap);
