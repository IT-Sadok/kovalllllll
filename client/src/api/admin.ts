import { api } from './axiosInstance';
import { unwrap, unwrapPaged } from './response';
import type {
  Product,
  WarehouseItem,
  Warehouse,
  Property,
  Value,
  Image,
  ApiResponse,
  ComponentSpec,
  CreateProductRequest,
  UpdateProductRequest,
  CreatePropertyRequest,
  UpdatePropertyRequest,
} from '../types';

// ─── Products (Admin) ─────────────────────────────────────────────────────────
export const adminCreateProduct = (data: CreateProductRequest) =>
  api.post<ApiResponse<Product>>('/products', data).then(unwrap);

export const adminUpdateProduct = (id: string, data: UpdateProductRequest) =>
  api.patch<ApiResponse<Product>>(`/products/${id}`, data).then(unwrap);

export const adminDeleteProduct = (id: string) =>
  api.delete(`/products/${id}`);

// Delisted products are hidden from every other product read, so they get their own listing.
export const adminGetDelistedProducts = (page = 1, pageSize = 15) =>
  api.get<ApiResponse<Product[]>>(`/products/delisted?page=${page}&pageSize=${pageSize}`).then(unwrapPaged);

export const adminRestoreProduct = (id: string) =>
  api.post(`/products/${id}/restore`);

export const assignValueToProductProperty = (productId: string, propertyId: string, valueId: string) =>
  api.post(`/products/${productId}/properties/${propertyId}/values/${valueId}`);

export const removeValueFromProductProperty = (productId: string, propertyId: string, valueId: string) =>
  api.delete(`/products/${productId}/properties/${propertyId}/values/${valueId}`);

export const removePropertyFromProduct = (productId: string, propertyId: string) =>
  api.delete(`/products/${productId}/properties/${propertyId}`);

export const setProductSpec = (productId: string, spec: ComponentSpec) =>
  api.put<ApiResponse<Product>>(`/products/${productId}/spec`, spec).then(unwrap);

export const removeProductSpec = (productId: string) =>
  api.delete(`/products/${productId}/spec`);

// ─── Warehouse ────────────────────────────────────────────────────────────────
// GET /warehouse → Warehouse (summary: name, createdAt)
export const getWarehouse = () =>
  api.get<ApiResponse<Warehouse>>('/warehouse').then(unwrap);

// GET /warehouse/items → PagedResult<WarehouseItem>
export const getWarehouseItems = (page = 1, pageSize = 20) =>
  api.get<ApiResponse<WarehouseItem[]>>(`/warehouse/items?page=${page}&pageSize=${pageSize}`).then(unwrapPaged);

// GET /warehouse/items/{warehouseItemId}
export const getWarehouseItem = (itemId: string) =>
  api.get<ApiResponse<WarehouseItem>>(`/warehouse/items/${itemId}`).then(unwrap);

// POST /warehouse/items/{warehouseItemId} body: { quantityToAdd }
export const addWarehouseQuantity = (itemId: string, quantityToAdd: number) =>
  api.post<ApiResponse<WarehouseItem>>(`/warehouse/items/${itemId}`, { quantityToAdd }).then(unwrap);

// DELETE /warehouse/items/{warehouseItemId} body: { quantityToRemove } ← DELETE with body
export const removeWarehouseQuantity = (itemId: string, quantityToRemove: number) =>
  api.delete<ApiResponse<WarehouseItem>>(`/warehouse/items/${itemId}`, { data: { quantityToRemove } }).then(unwrap);

// ─── Images ──────────────────────────────────────────────────────────────────
// POST /images/upload — multipart/form-data: file (IFormFile), productId (Guid)
export const uploadImage = (file: File, productId: string) => {
  const formData = new FormData();
  formData.append('file', file);
  return api.post<ApiResponse<Image>>(`/images/upload?productId=${productId}`, formData).then(unwrap);
};

// DELETE /images/{imageId}
export const deleteImage = (imageId: string) =>
  api.delete(`/images/${imageId}`);

// POST /images/{imageId}/set-primary
export const setPrimaryImage = (imageId: string) =>
  api.post(`/images/${imageId}/set-primary`);

// GET /images
export const getImages = () =>
  api.get<ApiResponse<Image[]>>('/images').then(unwrap);

// GET /images/product/{productId}
export const getProductImages = (productId: string) =>
  api.get<ApiResponse<Image[]>>(`/images/product/${productId}`).then(unwrap);

// GET /images/{imageId}
export const getImage = (imageId: string) =>
  api.get<ApiResponse<Image>>(`/images/${imageId}`).then(unwrap);

// ─── Properties ──────────────────────────────────────────────────────────────
// GET /properties — public
export const getProperties = () =>
  api.get<ApiResponse<Property[]>>('/properties').then(unwrap);

// POST /properties [Admin] body: { name, values: { text }[] }
export const createProperty = (data: CreatePropertyRequest) =>
  api.post<ApiResponse<Property>>('/properties', data).then(unwrap);

// PATCH /properties/{propertyId} [Admin] body: { name? }
export const updateProperty = (id: string, data: UpdatePropertyRequest) =>
  api.patch<ApiResponse<Property>>(`/properties/${id}`, data).then(unwrap);

// DELETE /properties/{propertyId} [Admin]
export const deleteProperty = (id: string) =>
  api.delete(`/properties/${id}`);

// GET /properties/{propertyId}/values → returns Property (with values)
export const getPropertyWithValues = (propertyId: string) =>
  api.get<ApiResponse<Property>>(`/properties/${propertyId}/values`).then(unwrap);

// POST /properties/{propertyId}/values/{valueId} [Admin]
export const assignValueToProperty = (propertyId: string, valueId: string) =>
  api.post(`/properties/${propertyId}/values/${valueId}`);

// DELETE /properties/{propertyId}/values/{valueId} [Admin]
export const removeValueFromProperty = (propertyId: string, valueId: string) =>
  api.delete(`/properties/${propertyId}/values/${valueId}`);

// ─── Values ──────────────────────────────────────────────────────────────────
// GET /values — public
export const getValues = () =>
  api.get<ApiResponse<Value[]>>('/values').then(unwrap);

// POST /values [Admin] body: { text, propertyId }
export const createValue = (text: string, propertyId: string) =>
  api.post<ApiResponse<Value>>('/values', { text, propertyId }).then(unwrap);

// PATCH /values/{valueId} [Admin] body: { text? }
export const updateValue = (id: string, text: string) =>
  api.patch<ApiResponse<Value>>(`/values/${id}`, { text }).then(unwrap);

// DELETE /values/{valueId} [Admin]
export const deleteValue = (id: string) =>
  api.delete(`/values/${id}`);

// GET /values/{valueId} [Admin]
export const getValue = (id: string) =>
  api.get<ApiResponse<Value>>(`/values/${id}`).then(unwrap);
