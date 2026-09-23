import { api } from './axiosInstance';
import { unwrap, unwrapPaged } from './response';
import type { ApiResponse, Order, ShippingDetails, PaymentSession } from '../types';

// GET /orders?page&pageSize → PagedResult<Order>
export const getOrders = (page = 1, pageSize = 20) =>
  api.get<ApiResponse<Order[]>>(`/orders?page=${page}&pageSize=${pageSize}`).then(unwrapPaged);

// POST /orders body: ShippingDetails → Order
export const createOrder = (payload: ShippingDetails) =>
  api.post<ApiResponse<Order>>('/orders', payload).then(unwrap);

// POST /orders/{orderId}/payment → Stripe Checkout URL, or isPaid when no redirect is needed
export const startOrderPayment = (orderId: string) =>
  api.post<ApiResponse<PaymentSession>>(`/orders/${orderId}/payment`).then(unwrap);

export const cancelOrder = (orderId: string) =>
  api.patch(`/orders/${orderId}/cancel`);

// GET /orders/admin — all orders for admin
export const getAdminOrders = (page = 1, pageSize = 20) =>
  api.get<ApiResponse<Order[]>>(`/orders/admin?page=${page}&pageSize=${pageSize}`).then(unwrapPaged);

// PATCH /orders/{orderId}/status
export const updateOrderStatus = (orderId: string, status: number) =>
  api.patch(`/orders/${orderId}/status`, status, {
    headers: { 'Content-Type': 'application/json' }
  });
