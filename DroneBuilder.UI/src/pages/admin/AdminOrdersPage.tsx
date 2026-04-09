import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getAdminOrders, updateOrderStatus } from '../../api/orders';
import { OrderStatusLabel, type OrderStatus } from '../../types';
import Skeleton from '../../components/ui/Skeleton';
import EmptyState from '../../components/ui/EmptyState';

const AdminOrdersPage: React.FC = () => {
  const [page, setPage] = useState(1);
  const [expandedOrder, setExpandedOrder] = useState<string | null>(null);
  const pageSize = 10;
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['admin', 'orders', page],
    queryFn: () => getAdminOrders(page, pageSize),
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ orderId, status }: { orderId: string; status: number }) =>
      updateOrderStatus(orderId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] });
      toast.success('Status updated');
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.Message || 'Update failed');
    },
  });

  const formatDate = (dateString: string) => {
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(dateString));
  };

  const getStatusStyle = (status: OrderStatus) => {
    switch (status) {
      case 0: return 'from-blue-500/20 to-indigo-500/20 text-blue-400 border-blue-500/30';
      case 1: return 'from-emerald-500/20 to-teal-500/20 text-emerald-400 border-emerald-500/30';
      case 2: return 'from-purple-500/20 to-pink-500/20 text-purple-400 border-purple-500/30';
      case 3: return 'from-cyan-500/20 to-blue-500/20 text-cyan-400 border-cyan-500/30';
      case 4: return 'from-red-500/20 to-orange-500/20 text-red-400 border-red-500/30';
      default: return 'from-slate-500/20 to-slate-600/20 text-slate-400 border-slate-500/30';
    }
  };

  const orders = data?.items || [];
  const totalCount = data?.totalCount || 0;
  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="page-enter max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-10">
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div className="space-y-2">
          <div className="inline-flex items-center px-3 py-1 rounded-full bg-cyan-500/10 border border-cyan-500/20 text-[10px] font-bold text-cyan-400 uppercase tracking-widest">
            Fulfillment Center
          </div>
          <h1 className="text-4xl font-black text-white font-orbitron tracking-tight">
            Order <span className="text-transparent bg-clip-text bg-gradient-to-r from-cyan-400 to-blue-500">Pipeline</span>
          </h1>
          <p className="text-slate-400 max-w-md">Real-time oversight and status management for all drone deployments.</p>
        </div>
        
        <div className="flex items-center gap-4 bg-white/5 backdrop-blur-md p-2 rounded-2xl border border-white/10">
          <div className="px-4 py-2 text-center">
            <p className="text-[10px] text-slate-500 uppercase font-bold tracking-wider">Total Orders</p>
            <p className="text-xl font-bold text-white font-orbitron">{totalCount}</p>
          </div>
        </div>
      </div>

      <div className="space-y-6">
        {isLoading ? (
          <div className="grid grid-cols-1 gap-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} className="h-24 w-full rounded-2xl" />
            ))}
          </div>
        ) : orders.length === 0 ? (
          <EmptyState title="Pipeline empty" description="No orders found in the system." />
        ) : (
          <div className="grid grid-cols-1 gap-4">
            {orders.map((order) => (
              <div 
                key={order.id} 
                className={`group glass-card transition-all duration-500 border-l-4 ${
                  expandedOrder === order.id ? 'ring-2 ring-cyan-500/50' : 'hover:bg-white/[0.03]'
                } ${
                  order.status === 0 ? 'border-l-blue-500' :
                  order.status === 1 ? 'border-l-emerald-500' :
                  order.status === 2 ? 'border-l-purple-500' :
                  order.status === 3 ? 'border-l-cyan-500' : 'border-l-red-500'
                }`}
              >
                <div className="p-5 flex flex-col md:flex-row items-center gap-6">
                  {/* Order ID & Date */}
                  <div className="w-full md:w-48 space-y-1">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-bold text-slate-500 font-orbitron uppercase">Order</span>
                      <span className="text-sm font-bold text-white">#{order.id.split('-')[0].toUpperCase()}</span>
                    </div>
                    <p className="text-[10px] text-slate-500 font-medium">{formatDate(order.createdAt)}</p>
                  </div>

                  {/* Customer */}
                  <div className="w-full md:flex-1 space-y-1">
                    <p className="text-[10px] text-slate-500 uppercase font-bold tracking-wider">Recipient</p>
                    <div className="flex items-center gap-2">
                      <div className="w-6 h-6 rounded-full bg-gradient-to-br from-slate-700 to-slate-800 flex items-center justify-center text-[10px] text-white">
                        {order.userEmail[0].toUpperCase()}
                      </div>
                      <span className="text-sm text-slate-200 truncate max-w-[200px]">{order.userEmail}</span>
                    </div>
                  </div>

                  {/* Value */}
                  <div className="w-full md:w-32 space-y-1">
                    <p className="text-[10px] text-slate-500 uppercase font-bold tracking-wider">Total Value</p>
                    <p className="text-lg font-bold text-cyan-400 font-orbitron">${order.totalPrice.toLocaleString()}</p>
                  </div>

                  {/* Status Badge */}
                  <div className="w-full md:w-40">
                    <div className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-xl border bg-gradient-to-br font-bold text-[10px] uppercase tracking-widest ${getStatusStyle(order.status)}`}>
                      <span className="relative flex h-2 w-2">
                        <span className={`animate-ping absolute inline-flex h-full w-full rounded-full opacity-75 ${
                          order.status === 0 ? 'bg-blue-400' :
                          order.status === 1 ? 'bg-emerald-400' :
                          order.status === 2 ? 'bg-purple-400' :
                          order.status === 3 ? 'bg-cyan-400' : 'bg-red-400'
                        }`}></span>
                        <span className={`relative inline-flex rounded-full h-2 w-2 ${
                          order.status === 0 ? 'bg-blue-500' :
                          order.status === 1 ? 'bg-emerald-500' :
                          order.status === 2 ? 'bg-purple-500' :
                          order.status === 3 ? 'bg-cyan-500' : 'bg-red-500'
                        }`}></span>
                      </span>
                      {OrderStatusLabel[order.status]}
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="w-full md:w-auto flex items-center gap-2">
                    <button
                      onClick={() => setExpandedOrder(expandedOrder === order.id ? null : order.id)}
                      className="p-2.5 rounded-xl bg-white/5 border border-white/10 text-slate-400 hover:text-white hover:bg-white/10 transition-all cursor-pointer"
                    >
                      <svg className={`w-5 h-5 transition-transform duration-300 ${expandedOrder === order.id ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                      </svg>
                    </button>
                    
                    <div className="relative group/menu">
                      <button className="p-2.5 rounded-xl bg-cyan-500/10 border border-cyan-500/20 text-cyan-400 hover:bg-cyan-500/20 transition-all cursor-pointer">
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z" />
                        </svg>
                      </button>
                      
                      <div className="absolute right-0 top-12 w-48 bg-[#0f172a] border border-white/10 rounded-2xl shadow-2xl opacity-0 invisible group-hover/menu:opacity-100 group-hover/menu:visible transition-all duration-200 z-50 p-2 space-y-1">
                        {Object.entries(OrderStatusLabel).map(([value, label]) => (
                          <button
                            key={value}
                            onClick={() => updateStatusMutation.mutate({ orderId: order.id, status: parseInt(value) })}
                            className={`w-full text-left px-4 py-2 rounded-xl text-xs font-bold transition-colors ${
                              parseInt(value) === order.status 
                              ? 'bg-cyan-500/10 text-cyan-400' 
                              : 'text-slate-400 hover:bg-white/5 hover:text-white'
                            }`}
                          >
                            Set to {label}
                          </button>
                        ))}
                      </div>
                    </div>
                  </div>
                </div>

                {/* Expanded Details */}
                {expandedOrder === order.id && (
                  <div className="border-t border-white/5 bg-black/20 p-6 space-y-6 animate-in slide-in-from-top-2 duration-300">
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                      {/* Items */}
                      <div className="space-y-4">
                        <h4 className="text-xs font-bold text-slate-500 uppercase tracking-widest font-orbitron">Payload Content</h4>
                        <div className="space-y-2">
                          {order.orderItems.map((item, idx) => (
                            <div key={idx} className="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/5">
                              <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-lg bg-slate-800 flex items-center justify-center border border-white/5 overflow-hidden">
                                  {item.productImageUrl ? (
                                    <img src={item.productImageUrl} alt={item.productName} className="w-full h-full object-cover" />
                                  ) : (
                                    <svg className="w-5 h-5 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                                    </svg>
                                  )}
                                </div>
                                <div>
                                  <p className="text-sm font-bold text-white">{item.productName}</p>
                                  <p className="text-[10px] text-slate-500">Qty: {item.quantity} × ${item.price.toLocaleString()}</p>
                                </div>
                              </div>
                              <p className="text-sm font-bold text-slate-300">${(item.quantity * item.price).toLocaleString()}</p>
                            </div>
                          ))}
                        </div>
                      </div>

                      {/* Shipping */}
                      <div className="space-y-4">
                        <h4 className="text-xs font-bold text-slate-500 uppercase tracking-widest font-orbitron">Deployment Coordinates</h4>
                        <div className="p-4 rounded-xl bg-cyan-500/5 border border-cyan-500/10 text-xs leading-relaxed text-slate-400 font-mono">
                          {(() => {
                            try {
                              const shipping: any = JSON.parse(order.shippingDetails);
                              const getVal = (key: string) => {
                                const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
                                return shipping[key] || shipping[pascalKey] || 'N/A';
                              };
                              return (
                                <div className="space-y-1">
                                  <p className="text-cyan-400 font-bold mb-2 uppercase">{getVal('fullName')}</p>
                                  <p>{getVal('addressLine1')}</p>
                                  {getVal('addressLine2') !== 'N/A' && <p>{getVal('addressLine2')}</p>}
                                  <p>{getVal('city')}, {getVal('state')} {getVal('postalCode')}</p>
                                  <p>{getVal('country')}</p>
                                  <p className="mt-2 text-slate-500">Contact: {getVal('phoneNumber')}</p>
                                </div>
                              );
                            } catch {
                              return <p>{order.shippingDetails}</p>;
                            }
                          })()}
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}

        {/* Improved Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-center gap-2 pt-6">
            <button
              onClick={() => setPage(p => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-4 py-2 rounded-xl bg-white/5 border border-white/10 text-slate-400 hover:text-white disabled:opacity-20 transition-all cursor-pointer font-bold text-xs"
            >
              PREV
            </button>
            <div className="flex gap-1">
              {Array.from({ length: totalPages }).map((_, i) => (
                <button
                  key={i}
                  onClick={() => setPage(i + 1)}
                  className={`w-8 h-8 rounded-xl text-xs font-bold transition-all ${
                    page === i + 1 
                    ? 'bg-cyan-500 text-black shadow-lg shadow-cyan-500/20' 
                    : 'bg-white/5 text-slate-500 hover:text-white'
                  }`}
                >
                  {i + 1}
                </button>
              ))}
            </div>
            <button
              onClick={() => setPage(p => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-4 py-2 rounded-xl bg-white/5 border border-white/10 text-slate-400 hover:text-white disabled:opacity-20 transition-all cursor-pointer font-bold text-xs"
            >
              NEXT
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminOrdersPage;
