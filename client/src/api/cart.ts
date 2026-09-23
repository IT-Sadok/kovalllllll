import { api } from './axiosInstance';
import { unwrap } from './response';
import type { ApiResponse, CartItem } from '../types';

// GET /carts/items — returns CartItem[] for authenticated user
export const getCartItems = () =>
  api.get<ApiResponse<CartItem[]>>('/carts/items').then(unwrap);

// POST /carts/items
export const addToCart = (productId: string, quantity: number = 1) =>
  api.post<ApiResponse<CartItem>>('/carts/items', { productId, quantity }).then(unwrap);

// DELETE /carts/items/{productId}
export const removeCartItem = (productId: string) =>
  api.delete(`/carts/items/${productId}`);

// PATCH /carts/items/{productId} — sets absolute quantity
export const updateCartItemQuantity = (productId: string, quantity: number) =>
  api.patch(`/carts/items/${productId}`, quantity, {
    headers: { 'Content-Type': 'application/json' }
  });

// DELETE /carts/clear
export const clearCart = () =>
  api.delete('/carts/clear');
