// ─── Auth ────────────────────────────────────────────────────────────────────
export interface AuthResponse {
  accessToken: string;
}

export interface DecodedToken {
  sub?: string;
  email?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  exp?: number;
  iat?: number;
}

export type UserRole = 'Admin' | 'User';

export interface AuthUser {
  id: string;
  email: string;
  role: UserRole;
}

// ─── Products ─────────────────────────────────────────────────────────────────
export interface Value {
  id: string;
  text: string;
}

export interface Property {
  id: string;
  name: string;
  values: Value[];
}

export interface Image {
  id: string;
  url: string;
  fileName: string;
  uploadedAt: string;
  isPrimary: boolean;
}

export interface Product {
  id: string;
  name: string;
  price: number;
  category: string;
  properties: Property[];
  images?: Image[];
}

export interface ProductPropertiesResponse {
  id: string;
  name: string;
  price: number;
  category: string;
  properties: Property[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProductFilters {
  page?: number;
  pageSize?: number;
  name?: string;
  minPrice?: number | '';
  maxPrice?: number | '';
  category?: string;
}

// ─── Cart ─────────────────────────────────────────────────────────────────────
export interface CartItem {
  cartId: string;
  productId: string;
  productName: string;
  productImageUrl?: string;
  quantity: number;
  price: number;
}

export interface Cart {
  userId: string;
  cartItems: CartItem[];
  totalPrice: number;
  createdAt: string;
}

export interface CreateCartItem {
  productId: string;
  quantity: number;
}

// ─── Orders ───────────────────────────────────────────────────────────────────
// Backend enum: New=0, Paid=1, Sent=2, Completed=3, Cancelled=4
export type OrderStatus = 0 | 1 | 2 | 3 | 4;

export const OrderStatusLabel: Record<OrderStatus, string> = {
  0: 'New',
  1: 'Paid',
  2: 'Sent',
  3: 'Completed',
  4: 'Cancelled',
};

export interface OrderItem {
  productId: string;
  orderId: string;
  productName: string;
  productImageUrl?: string;
  quantity: number;
  price: number;
}

export interface ShippingDetails {
  fullName: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  phoneNumber: string;
}

export interface Order {
  id: string;
  userId: string;
  userEmail: string;
  status: OrderStatus;
  orderItems: OrderItem[];
  totalPrice: number;
  shippingDetails: string; // JSON string — parse via JSON.parse(order.shippingDetails) as ShippingDetails
  createdAt: string;
}

// ─── Warehouse ────────────────────────────────────────────────────────────────
export interface Warehouse {
  name: string;
  createdAt: string;
}

export interface WarehouseItem {
  id: string;
  warehouseId: string;
  productId: string;
  productName: string;
  quantity: number;
}

// ─── Request models ───────────────────────────────────────────────────────────
export interface CreateProductRequest {
  name: string;
  price: number;
  category: string;
  properties: {
    name: string;
    values: { text: string }[];
  };
}

export interface UpdateProductRequest {
  name?: string;
  price?: number;
  category?: string;
}

export interface CreatePropertyRequest {
  name: string;
  values: { text: string }[];
}

export interface UpdatePropertyRequest {
  name?: string;
}

export interface CreateValueRequest {
  text: string;
}

export interface UpdateValueRequest {
  text?: string;
}

export interface AddQuantityRequest {
  quantityToAdd: number;
}

export interface RemoveQuantityRequest {
  quantityToRemove: number;
}
