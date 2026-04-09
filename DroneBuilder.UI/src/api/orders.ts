import { api } from './axiosInstance';
import type { Order, ShippingDetails, PagedResult } from '../types';

// GET /orders?page&pageSize → PagedResult<Order>
export const getOrders = (page = 1, pageSize = 20) =>
  api.get<PagedResult<Order>>(`/orders?page=${page}&pageSize=${pageSize}`).then((r) => r.data);

// POST /orders body: ShippingDetails → Order
export const createOrder = (payload: ShippingDetails) =>
  api.post<Order>('/orders', payload).then((r) => r.data);

// PATCH /orders/{orderId}/pay
export const payOrder = (orderId: string) =>
  api.patch(`/orders/${orderId}/pay`);

// GET /orders/admin — all orders for admin
export const getAdminOrders = (page = 1, pageSize = 20) =>
  api.get<PagedResult<Order>>(`/orders/admin?page=${page}&pageSize=${pageSize}`).then((r) => r.data);

// PATCH /orders/{orderId}/status
export const updateOrderStatus = (orderId: string, status: number) =>
  api.patch(`/orders/${orderId}/status`, status, {
    headers: { 'Content-Type': 'application/json' }
  });
