import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getCartItems, removeCartItem, clearCart, updateCartItemQuantity } from '../api/cart';
import { useCartStore } from '../store/cartStore';
import type { CartItem } from '../types';
import Skeleton from '../components/ui/Skeleton';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';

const CartPage: React.FC = () => {
  const navigate = useNavigate();
  const { setItemCount } = useCartStore();
  const queryClient = useQueryClient();

  const { data: items = [], isLoading } = useQuery({
    queryKey: ['cart'],
    queryFn: async () => {
      const data = await getCartItems();
      setItemCount(data.length);
      return data;
    },
  });

  // DELETE /carts/items/{productId} — param is productId!
  const removeMutation = useMutation({
    mutationFn: (productId: string) => removeCartItem(productId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      toast.success('Item removed');
    },
    onError: () => toast.error('Failed to remove item'),
  });

  const updateQuantityMutation = useMutation({
    mutationFn: ({ productId, quantity }: { productId: string; quantity: number }) => 
      updateCartItemQuantity(productId, quantity),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.Message || 'Failed to update quantity');
    },
  });

  const clearMutation = useMutation({
    mutationFn: clearCart,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      setItemCount(0);
      toast.success('Cart cleared');
    },
  });

  // CartItem.price is unit price, totalPrice = price * quantity
  const totalPrice = (items as CartItem[]).reduce(
    (sum, item) => sum + item.price * item.quantity,
    0
  );

  return (
    <div className="page-enter max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold text-white font-orbitron">
          Shopping <span className="text-cyan-400">Cart</span>
        </h1>
        {items.length > 0 && (
          <button
            onClick={() => clearMutation.mutate()}
            id="cart-clear-btn"
            className="text-sm text-red-400 hover:text-red-300 transition-colors cursor-pointer"
            disabled={clearMutation.isPending}
          >
            Clear all
          </button>
        )}
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="glass-card p-4 flex gap-4">
              <Skeleton className="w-20 h-20 rounded-xl" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-3/4" />
                <Skeleton className="h-4 w-1/3" />
              </div>
            </div>
          ))}
        </div>
      ) : items.length === 0 ? (
        <EmptyState
          title="Your cart is empty"
          description="Explore our catalog and add some drones to your cart."
          action={
            <Link to="/" id="cart-browse-btn">
              <Button>Browse Catalog</Button>
            </Link>
          }
        />
      ) : (
        <div className="space-y-4">
          {/* Items — keyed by productId (CartItem has no unique id field) */}
          {(items as CartItem[]).map((item) => (
            <div
              key={item.productId}
              className="glass-card p-4 flex items-center gap-4"
              id={`cart-item-${item.productId}`}
            >
              {/* Product Image */}
              <div className="w-24 h-24 rounded-xl overflow-hidden bg-gradient-to-br from-slate-800 to-slate-900 flex-shrink-0 border border-white/5">
                {item.productImageUrl ? (
                  <img src={item.productImageUrl} alt={item.productName} className="w-full h-full object-cover" />
                ) : (
                  <div className="w-full h-full flex items-center justify-center">
                    <svg className="w-10 h-10 text-slate-700" viewBox="0 0 32 32" fill="none">
                      <circle cx="16" cy="16" r="4" fill="currentColor" />
                      <line x1="16" y1="10" x2="8" y2="4" stroke="currentColor" strokeWidth="1.5" />
                      <line x1="16" y1="10" x2="24" y2="4" stroke="currentColor" strokeWidth="1.5" />
                      <line x1="16" y1="22" x2="8" y2="28" stroke="currentColor" strokeWidth="1.5" />
                      <line x1="16" y1="22" x2="24" y2="28" stroke="currentColor" strokeWidth="1.5" />
                    </svg>
                  </div>
                )}
              </div>

              <div className="flex-1 min-w-0">
                <h3 className="font-semibold text-white truncate text-lg">{item.productName}</h3>
                <p className="text-sm text-slate-500 mb-2">${item.price.toLocaleString()} per unit</p>
                
                {/* Quantity Controls */}
                <div className="flex items-center gap-3">
                  <div className="flex items-center rounded-lg border border-white/10 bg-black/20 overflow-hidden h-8">
                    <button
                      onClick={() => updateQuantityMutation.mutate({ productId: item.productId, quantity: item.quantity - 1 })}
                      disabled={updateQuantityMutation.isPending || item.quantity <= 1}
                      className="w-8 h-full flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/5 transition-all cursor-pointer disabled:opacity-30"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
                      </svg>
                    </button>
                    <span className="w-8 text-center text-white text-xs font-medium">{item.quantity}</span>
                    <button
                      onClick={() => updateQuantityMutation.mutate({ productId: item.productId, quantity: item.quantity + 1 })}
                      disabled={updateQuantityMutation.isPending}
                      className="w-8 h-full flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/5 transition-all cursor-pointer"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                      </svg>
                    </button>
                  </div>
                  <span className="text-[10px] text-slate-500 uppercase tracking-wider">Quantity</span>
                </div>
              </div>

              <div className="text-right">
                <p className="font-bold text-cyan-400 font-orbitron">
                  ${(item.price * item.quantity).toLocaleString()}
                </p>
              </div>

              {/* Remove uses productId — matches DELETE /carts/items/{productId} */}
              <button
                onClick={() => removeMutation.mutate(item.productId)}
                id={`remove-cart-item-${item.productId}`}
                className="p-2 text-slate-500 hover:text-red-400 transition-colors cursor-pointer"
                disabled={removeMutation.isPending}
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </div>
          ))}

          {/* Summary */}
          <div className="glass-card p-5 mt-6">
            <div className="flex justify-between items-center mb-4">
              <span className="text-slate-400">Subtotal ({items.length} items)</span>
              <span className="text-2xl font-bold text-cyan-400 font-orbitron">
                ${totalPrice.toLocaleString()}
              </span>
            </div>
            <Button
              fullWidth
              size="lg"
              onClick={() => navigate('/checkout')}
              id="cart-checkout-btn"
              icon={
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                </svg>
              }
            >
              Proceed to Checkout
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

export default CartPage;
