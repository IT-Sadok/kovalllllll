import { api } from './axiosInstance';
import type { Order, ShippingDetails, PagedResult, PaymentSession } from '../types';

// GET /orders?page&pageSize → PagedResult<Order>
export const getOrders = (page = 1, pageSize = 20) =>
  api.get<PagedResult<Order>>(`/orders?page=${page}&pageSize=${pageSize}`).then((r) => r.data);

// POST /orders body: ShippingDetails → Order
export const createOrder = (payload: ShippingDetails) =>
  api.post<Order>('/orders', payload).then((r) => r.data);

// POST /orders/{orderId}/payment → Stripe Checkout URL, or isPaid when no redirect is needed
export const startOrderPayment = (orderId: string) =>
  api.post<PaymentSession>(`/orders/${orderId}/payment`).then((r) => r.data);

export const cancelOrder = (orderId: string) =>
  api.patch(`/orders/${orderId}/cancel`);

// GET /orders/admin — all orders for admin
export const getAdminOrders = (page = 1, pageSize = 20) =>
  api.get<PagedResult<Order>>(`/orders/admin?page=${page}&pageSize=${pageSize}`).then((r) => r.data);

// PATCH /orders/{orderId}/status
export const updateOrderStatus = (orderId: string, status: number) =>
  api.patch(`/orders/${orderId}/status`, status, {
    headers: { 'Content-Type': 'application/json' }
  });
