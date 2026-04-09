import { api } from './axiosInstance';
import type { CartItem } from '../types';

// GET /carts/items — returns CartItem[] for authenticated user
export const getCartItems = () =>
  api.get<CartItem[]>('/carts/items').then((r) => r.data);

// POST /carts/items
export const addToCart = (productId: string, quantity: number = 1) =>
  api.post<CartItem>('/carts/items', { productId, quantity }).then((r) => r.data);

// DELETE /carts/items/{productId} ← IMPORTANT: param is productId, NOT itemId/cartId
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
