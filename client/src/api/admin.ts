import { api } from './axiosInstance';
import { unwrap, unwrapPaged } from './response';
import type {
  Product,
  WarehouseItem,
  Warehouse,
  Image,
  ApiResponse,
  ComponentSpec,
  ProductAttribute,
  ImportRun,
  CreateProductRequest,
  UpdateProductRequest,
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

export const setProductSpec = (productId: string, spec: ComponentSpec) =>
  api.put<ApiResponse<Product>>(`/products/${productId}/spec`, spec).then(unwrap);

export const removeProductSpec = (productId: string) =>
  api.delete(`/products/${productId}/spec`);

export const setProductAttributes = (productId: string, attributes: ProductAttribute[]) =>
  api.put<ApiResponse<Product>>(`/products/${productId}/attributes`, attributes).then(unwrap);

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

// POST /warehouse/items/restock body: { quantity } — sets every empty item to that quantity
export const restockEmptyWarehouseItems = (quantity: number) =>
  api.post<ApiResponse<{ restockedItems: number }>>('/warehouse/items/restock', { quantity }).then(unwrap);

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

// ─── Imports ─────────────────────────────────────────────────────────────────
export const startRaceDayQuadsImport = () =>
  api.post<ApiResponse<ImportRun>>('/imports/racedayquads').then(unwrap);

export const getImportRuns = () =>
  api.get<ApiResponse<ImportRun[]>>('/imports').then(unwrap);
