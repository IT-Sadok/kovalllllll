import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getOrders } from '../api/orders';
import type { Order, OrderStatus } from '../types';
import EmptyState from '../components/ui/EmptyState';
import Skeleton from '../components/ui/Skeleton';
import { Link } from 'react-router-dom';

const OrderItemView: React.FC<{ item: any }> = ({ item }) => (
  <div className="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/5 group hover:bg-white/[0.08] transition-all">
    <div className="flex items-center gap-4">
      <div className="w-12 h-12 rounded-lg overflow-hidden bg-slate-900 border border-white/10 flex-shrink-0">
        {item.productImageUrl ? (
          <img src={item.productImageUrl} alt={item.productName} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <svg className="w-6 h-6 text-slate-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
            </svg>
          </div>
        )}
      </div>
      <div>
        <p className="text-sm font-bold text-white group-hover:text-cyan-400 transition-colors">{item.productName}</p>
        <p className="text-[10px] text-slate-500 uppercase tracking-wider font-medium">Quantity: {item.quantity} × ${item.price.toLocaleString()}</p>
      </div>
    </div>
    <p className="text-sm font-bold text-slate-300 font-orbitron">${(item.quantity * item.price).toLocaleString()}</p>
  </div>
);

const OrderCard: React.FC<{ order: Order }> = ({ order }) => {
  const [expanded, setExpanded] = useState(false);
  
  const shipping: any = (() => {
    try {
      return JSON.parse(order.shippingDetails);
    } catch {
      return null;
    }
  })();

  const getShippingValue = (key: string) => {
    if (!shipping) return 'N/A';
    // Handle both camelCase and PascalCase
    const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    return shipping[key] || shipping[pascalKey] || 'N/A';
  };

  const getStatusStyle = (status: OrderStatus) => {
    switch (status) {
      case 0: return 'bg-blue-500/10 text-blue-400 border-blue-500/20';
      case 1: return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20';
      case 2: return 'bg-purple-500/10 text-purple-400 border-purple-500/20';
      case 3: return 'bg-cyan-500/10 text-cyan-400 border-cyan-500/20';
      case 4: return 'bg-red-500/10 text-red-400 border-red-500/20';
      default: return 'bg-slate-500/10 text-slate-400 border-slate-500/20';
    }
  };

  const OrderStatusLabel: Record<number, string> = {
    0: 'New', 1: 'Paid', 2: 'Sent', 3: 'Completed', 4: 'Cancelled'
  };

  return (
    <div className="glass-card overflow-hidden transition-all duration-300 hover:border-white/20">
      {/* Header */}
      <div className="p-5 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 border-b border-white/5 bg-white/[0.02]">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <span className="text-[10px] font-black text-slate-500 uppercase font-orbitron tracking-widest">Order ID</span>
            <span className="text-sm font-bold text-white truncate max-w-[120px]" title={order.id}>
              #{order.id?.split('-')[0].toUpperCase() || 'N/A'}
            </span>
          </div>
          <p className="text-[10px] text-slate-500 font-medium">
            {new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(order.createdAt))}
          </p>
        </div>

        <div className="flex items-center gap-4 w-full sm:w-auto justify-between sm:justify-end">
          <div className={`px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider border ${getStatusStyle(order.status)}`}>
            {OrderStatusLabel[order.status]}
          </div>
          <div className="text-right">
            <p className="text-[10px] text-slate-500 uppercase font-bold tracking-wider">Total Value</p>
            <p className="text-lg font-bold text-cyan-400 font-orbitron">${order.totalPrice.toLocaleString()}</p>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="p-5 space-y-4">
        {/* Item list */}
        <div className="space-y-2">
          {order.orderItems?.slice(0, expanded ? undefined : 2).map((item, idx) => (
            <OrderItemView key={idx} item={item} />
          ))}
        </div>

        {/* Expand Toggle */}
        {(order.orderItems?.length || 0) > 2 && (
          <button
            onClick={() => setExpanded(!expanded)}
            className="w-full py-2 rounded-xl bg-white/5 border border-white/5 text-[10px] font-bold text-slate-400 uppercase tracking-widest hover:text-white hover:bg-white/10 transition-all cursor-pointer"
          >
            {expanded ? 'Collapse Cargo Details' : `Expand +${order.orderItems.length - 2} More Items`}
          </button>
        )}

        {/* Full Shipping Details */}
        {shipping && (
          <div className="pt-4 border-t border-white/5">
            <div className="flex items-start gap-4 p-5 rounded-2xl bg-cyan-500/5 border border-cyan-500/10">
              <div className="p-2.5 rounded-xl bg-cyan-500/10 text-cyan-400 flex-shrink-0">
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </div>
              <div className="space-y-1">
                <p className="text-[10px] text-slate-500 uppercase font-black tracking-widest mb-1 font-orbitron">Deployment Destination</p>
                <div className="text-sm text-slate-300 space-y-0.5 leading-relaxed">
                  <p className="font-bold text-white mb-1">{getShippingValue('fullName')}</p>
                  <p>{getShippingValue('addressLine1')}</p>
                  {getShippingValue('addressLine2') !== 'N/A' && <p>{getShippingValue('addressLine2')}</p>}
                  <p>{getShippingValue('city')}, {getShippingValue('state')} {getShippingValue('postalCode')}</p>
                  <p className="font-bold text-slate-400 uppercase text-[10px] tracking-widest mt-1">{getShippingValue('country')}</p>
                  <p className="text-[10px] text-slate-500 pt-1">Contact: {getShippingValue('phoneNumber')}</p>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

const OrdersPage: React.FC = () => {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['orders', page],
    queryFn: () => getOrders(page, 10),
  });

  return (
    <div className="page-enter max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="mb-10 text-center sm:text-left space-y-2">
        <h1 className="text-4xl font-black text-white font-orbitron tracking-tight">
          Deployment <span className="text-cyan-400">History</span>
        </h1>
        <p className="text-slate-400">Review your past mission logs and order status.</p>
      </div>

      {isLoading ? (
        <div className="space-y-6">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-48 w-full rounded-2xl" />
          ))}
        </div>
      ) : (data?.items.length || 0) === 0 ? (
        <EmptyState
          title="No Mission Logs Found"
          description="Your deployment registry is empty. Ready to build your first drone?"
          action={
            <Link to="/">
              <button className="px-6 py-3 rounded-xl bg-cyan-500 text-black font-bold uppercase tracking-widest hover:bg-cyan-400 transition-all cursor-pointer shadow-lg shadow-cyan-500/20">
                Launch Catalog
              </button>
            </Link>
          }
        />
      ) : (
        <div className="space-y-6">
          {data?.items.map((order, idx) => (
            <OrderCard key={order.id || idx} order={order} />
          ))}
          
          {/* Pagination */}
          {(data?.totalCount || 0) > 10 && (
            <div className="flex justify-center gap-2 pt-8">
              {Array.from({ length: Math.ceil((data?.totalCount || 0) / 10) }).map((_, i) => (
                <button
                  key={i}
                  onClick={() => setPage(i + 1)}
                  className={`w-10 h-10 rounded-xl font-bold transition-all ${
                    page === i + 1 
                    ? 'bg-cyan-500 text-black' 
                    : 'bg-white/5 text-slate-500 hover:text-white hover:bg-white/10'
                  }`}
                >
                  {i + 1}
                </button>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default OrdersPage;
