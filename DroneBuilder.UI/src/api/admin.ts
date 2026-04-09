import { api } from './axiosInstance';
import type {
  Product,
  WarehouseItem,
  Warehouse,
  Property,
  Value,
  Image,
  PagedResult,
  CreateProductRequest,
  UpdateProductRequest,
  CreatePropertyRequest,
  UpdatePropertyRequest,
} from '../types';

// ─── Products (Admin) ─────────────────────────────────────────────────────────
export const adminCreateProduct = (data: CreateProductRequest) =>
  api.post<Product>('/products', data).then((r) => r.data);

export const adminUpdateProduct = (id: string, data: UpdateProductRequest) =>
  api.patch<Product>(`/products/${id}`, data).then((r) => r.data);

export const adminDeleteProduct = (id: string) =>
  api.delete(`/products/${id}`);

export const assignValueToProductProperty = (productId: string, propertyId: string, valueId: string) =>
  api.post(`/products/${productId}/properties/${propertyId}/values/${valueId}`);

export const removeValueFromProductProperty = (productId: string, propertyId: string, valueId: string) =>
  api.delete(`/products/${productId}/properties/${propertyId}/values/${valueId}`);

export const removePropertyFromProduct = (productId: string, propertyId: string) =>
  api.delete(`/products/${productId}/properties/${propertyId}`);

// ─── Warehouse ────────────────────────────────────────────────────────────────
// GET /warehouse → Warehouse (summary: name, createdAt)
export const getWarehouse = () =>
  api.get<Warehouse>('/warehouse').then((r) => r.data);

// GET /warehouse/items → PagedResult<WarehouseItem>
export const getWarehouseItems = (page = 1, pageSize = 20) =>
  api.get<PagedResult<WarehouseItem>>(`/warehouse/items?page=${page}&pageSize=${pageSize}`).then((r) => r.data);

// GET /warehouse/items/{warehouseItemId}
export const getWarehouseItem = (itemId: string) =>
  api.get<WarehouseItem>(`/warehouse/items/${itemId}`).then((r) => r.data);

// POST /warehouse/items/{warehouseItemId} body: { quantityToAdd }
export const addWarehouseQuantity = (itemId: string, quantityToAdd: number) =>
  api.post<WarehouseItem>(`/warehouse/items/${itemId}`, { quantityToAdd }).then((r) => r.data);

// DELETE /warehouse/items/{warehouseItemId} body: { quantityToRemove } ← DELETE with body
export const removeWarehouseQuantity = (itemId: string, quantityToRemove: number) =>
  api.delete<WarehouseItem>(`/warehouse/items/${itemId}`, { data: { quantityToRemove } }).then((r) => r.data);

// ─── Images ──────────────────────────────────────────────────────────────────
// POST /images/upload — multipart/form-data: file (IFormFile), productId (Guid)
export const uploadImage = (file: File, productId: string) => {
  const formData = new FormData();
  formData.append('file', file);
  return api.post<Image>(`/images/upload?productId=${productId}`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }).then((r) => r.data);
};

// DELETE /images/{imageId}
export const deleteImage = (imageId: string) =>
  api.delete(`/images/${imageId}`);

// POST /images/{imageId}/set-primary
export const setPrimaryImage = (imageId: string) =>
  api.post(`/images/${imageId}/set-primary`);

// GET /images
export const getImages = () =>
  api.get<Image[]>('/images').then((r) => r.data);

// GET /images/product/{productId}
export const getProductImages = (productId: string) =>
  api.get<Image[]>(`/images/product/${productId}`).then((r) => r.data);

// GET /images/{imageId}
export const getImage = (imageId: string) =>
  api.get<Image>(`/images/${imageId}`).then((r) => r.data);

// ─── Properties ──────────────────────────────────────────────────────────────
// GET /properties — public
export const getProperties = () =>
  api.get<Property[]>('/properties').then((r) => r.data);

// POST /properties [Admin] body: { name, values: { text }[] }
export const createProperty = (data: CreatePropertyRequest) =>
  api.post<Property>('/properties', data).then((r) => r.data);

// PATCH /properties/{propertyId} [Admin] body: { name? }
export const updateProperty = (id: string, data: UpdatePropertyRequest) =>
  api.patch<Property>(`/properties/${id}`, data).then((r) => r.data);

// DELETE /properties/{propertyId} [Admin]
export const deleteProperty = (id: string) =>
  api.delete(`/properties/${id}`);

// GET /properties/{propertyId}/values → returns Property (with values)
export const getPropertyWithValues = (propertyId: string) =>
  api.get<Property>(`/properties/${propertyId}/values`).then((r) => r.data);

// POST /properties/{propertyId}/values/{valueId} [Admin]
export const assignValueToProperty = (propertyId: string, valueId: string) =>
  api.post(`/properties/${propertyId}/values/${valueId}`);

// DELETE /properties/{propertyId}/values/{valueId} [Admin]
export const removeValueFromProperty = (propertyId: string, valueId: string) =>
  api.delete(`/properties/${propertyId}/values/${valueId}`);

// ─── Values ──────────────────────────────────────────────────────────────────
// GET /values — public
export const getValues = () =>
  api.get<Value[]>('/values').then((r) => r.data);

// POST /values [Admin] body: { text, propertyId }
export const createValue = (text: string, propertyId?: string) =>
  api.post<Value>('/values', { text, propertyId }).then((r) => r.data);

// PATCH /values/{valueId} [Admin] body: { text? }
export const updateValue = (id: string, text: string) =>
  api.patch<Value>(`/values/${id}`, { text }).then((r) => r.data);

// DELETE /values/{valueId} [Admin]
export const deleteValue = (id: string) =>
  api.delete(`/values/${id}`);

// GET /values/{valueId} [Admin]
export const getValue = (id: string) =>
  api.get<Value>(`/values/${id}`).then((r) => r.data);
