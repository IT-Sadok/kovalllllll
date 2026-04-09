import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getProduct, getProductProperties } from '../api/products';
import { getProductImages } from '../api/admin';
import { addToCart } from '../api/cart';
import { useAuthStore } from '../store/authStore';
import { useCartStore } from '../store/cartStore';
import Skeleton from '../components/ui/Skeleton';
import Button from '../components/ui/Button';
import type { Image } from '../types';

const ProductPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const { increment } = useCartStore();
  const [selectedImage, setSelectedImage] = useState(0);
  const [adding, setAdding] = useState(false);
  const [quantity, setQuantity] = useState(1);

  const { data: product, isLoading: loadingProduct, isError } = useQuery({
    queryKey: ['product', id],
    queryFn: () => getProduct(id!),
    enabled: !!id,
  });

  const { data: properties, isLoading: loadingProps } = useQuery({
    queryKey: ['product-properties', id],
    queryFn: () => getProductProperties(id!),
    enabled: !!id,
  });

  const { data: images } = useQuery({
    queryKey: ['product-images', id],
    queryFn: () => getProductImages(id!),
    enabled: !!id,
  });

  const handleAddToCart = async () => {
    if (!user) {
      toast.error('Please sign in to add items to cart');
      navigate('/login');
      return;
    }
    setAdding(true);
    try {
      await addToCart(id!, quantity);
      increment();
      toast.success(`Added ${quantity}x ${product?.name} to cart!`);
    } catch {
      toast.error('Failed to add to cart');
    } finally {
      setAdding(false);
    }
  };

  if (isError) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-20 text-center">
        <p className="text-red-400">Product not found.</p>
        <Button onClick={() => navigate('/')} className="mt-4" variant="ghost">← Back to Catalog</Button>
      </div>
    );
  }

  const allImages = (images && images.length > 0 ? images : (product?.images || [])) as Image[];
  const sortedImages = [...allImages].sort((a, b) => (a.isPrimary === b.isPrimary ? 0 : a.isPrimary ? -1 : 1));

  const nextImage = () => {
    setSelectedImage((prev) => (prev + 1) % sortedImages.length);
  };

  const prevImage = () => {
    setSelectedImage((prev) => (prev - 1 + sortedImages.length) % sortedImages.length);
  };

  return (
    <div className="page-enter max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Back */}
      <button
        onClick={() => navigate(-1)}
        id="product-back-btn"
        className="flex items-center gap-2 text-sm text-slate-400 hover:text-white mb-8 transition-colors cursor-pointer"
      >
        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
        Back to Catalog
      </button>

      <div className="grid lg:grid-cols-2 gap-12">
        {/* Left: Gallery */}
        <div className="space-y-4">
          {/* Main image */}
          <div className="relative aspect-square rounded-2xl overflow-hidden bg-gradient-to-br from-slate-800 to-slate-900 border border-[rgba(0,212,255,0.1)] group">
            {loadingProduct ? (
              <Skeleton className="w-full h-full" />
            ) : sortedImages[selectedImage] ? (
              <>
                <img
                  src={sortedImages[selectedImage]?.url}
                  alt={product?.name}
                  className="w-full h-full object-cover transition-opacity duration-300"
                />
                
                {/* Navigation Arrows */}
                {sortedImages.length > 1 && (
                  <>
                    <button 
                      onClick={(e) => { e.preventDefault(); prevImage(); }}
                      className="absolute left-4 top-1/2 -translate-y-1/2 p-2 rounded-full bg-black/40 backdrop-blur-md text-white border border-white/10 opacity-0 group-hover:opacity-100 transition-all hover:bg-black/60 cursor-pointer"
                    >
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                      </svg>
                    </button>
                    <button 
                      onClick={(e) => { e.preventDefault(); nextImage(); }}
                      className="absolute right-4 top-1/2 -translate-y-1/2 p-2 rounded-full bg-black/40 backdrop-blur-md text-white border border-white/10 opacity-0 group-hover:opacity-100 transition-all hover:bg-black/60 cursor-pointer"
                    >
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                      </svg>
                    </button>
                  </>
                )}
              </>
            ) : (
              <div className="w-full h-full flex items-center justify-center">
                <svg className="w-32 h-32 text-slate-700" viewBox="0 0 32 32" fill="none">
                  <circle cx="16" cy="16" r="4" fill="currentColor" />
                  <line x1="16" y1="10" x2="8" y2="4" stroke="currentColor" strokeWidth="1.5" />
                  <line x1="16" y1="10" x2="24" y2="4" stroke="currentColor" strokeWidth="1.5" />
                  <line x1="16" y1="22" x2="8" y2="28" stroke="currentColor" strokeWidth="1.5" />
                  <line x1="16" y1="22" x2="24" y2="28" stroke="currentColor" strokeWidth="1.5" />
                  <ellipse cx="8" cy="4" rx="6" ry="2" stroke="currentColor" strokeWidth="1.2" fill="none" />
                  <ellipse cx="24" cy="4" rx="6" ry="2" stroke="currentColor" strokeWidth="1.2" fill="none" />
                  <ellipse cx="8" cy="28" rx="6" ry="2" stroke="currentColor" strokeWidth="1.2" fill="none" />
                  <ellipse cx="24" cy="28" rx="6" ry="2" stroke="currentColor" strokeWidth="1.2" fill="none" />
                </svg>
              </div>
            )}
          </div>

          {/* Thumbnails */}
          {sortedImages.length > 1 && (
            <div className="flex gap-3 overflow-x-auto pb-1 no-scrollbar">
              {sortedImages.map((img, idx) => (
                <button
                  key={img.id}
                  onClick={() => setSelectedImage(idx)}
                  id={`product-thumb-${idx}`}
                  className={`flex-shrink-0 w-16 h-16 rounded-xl overflow-hidden border-2 transition-all duration-200 cursor-pointer ${
                    idx === selectedImage ? 'border-cyan-400 ring-2 ring-cyan-500/20' : 'border-white/5 opacity-60 hover:opacity-100'
                  }`}
                >
                  <img src={img.url} alt={`View ${idx + 1}`} className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Right: Details */}
        <div className="space-y-6">
          {loadingProduct ? (
            <div className="space-y-3">
              <Skeleton className="h-8 w-3/4" />
              <Skeleton className="h-4 w-1/3" />
              <Skeleton className="h-10 w-1/2" />
            </div>
          ) : (
            <>
              {product?.category && (
                <span className="text-xs px-2.5 py-1 rounded-full bg-cyan-500/10 border border-cyan-500/20 text-cyan-400 font-medium">
                  {product.category}
                </span>
              )}
              <h1 className="text-3xl font-bold text-white font-orbitron">{product?.name}</h1>
              <div className="flex items-center gap-3">
                <div className="text-4xl font-black text-cyan-400 font-orbitron">
                  ${product?.price.toLocaleString()}
                </div>
                {product?.stockQuantity === 0 ? (
                  <span className="px-3 py-1 rounded-full bg-red-500/10 border border-red-500/20 text-red-400 text-[10px] font-bold uppercase tracking-widest font-orbitron">
                    Out of Stock
                  </span>
                ) : (
                  <span className="px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-xs font-medium">
                    In Stock ({product?.stockQuantity})
                  </span>
                )}
              </div>

              {/* Quantity + Add to cart */}
              <div className="flex items-center gap-4 pt-2">
                <div className={`flex items-center rounded-xl border border-[rgba(0,212,255,0.2)] overflow-hidden ${product?.stockQuantity === 0 ? 'opacity-50 pointer-events-none' : ''}`}>
                  <button
                    onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                    disabled={product?.stockQuantity === 0}
                    id="qty-decrease"
                    className="w-10 h-10 flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/5 transition-all cursor-pointer"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 12H4" />
                    </svg>
                  </button>
                  <span className="w-10 text-center text-white font-medium">{product?.stockQuantity === 0 ? 0 : quantity}</span>
                  <button
                    onClick={() => setQuantity((q) => q + 1)}
                    disabled={product?.stockQuantity === 0}
                    id="qty-increase"
                    className="w-10 h-10 flex items-center justify-center text-slate-400 hover:text-white hover:bg-white/5 transition-all cursor-pointer"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                    </svg>
                  </button>
                </div>
                <Button
                  onClick={handleAddToCart}
                  loading={adding}
                  disabled={product?.stockQuantity === 0}
                  size="lg"
                  variant={product?.stockQuantity === 0 ? 'ghost' : 'primary'}
                  className="flex-1"
                  id="product-add-to-cart"
                  icon={
                    !adding && !product?.stockQuantity === 0 && (
                      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                      </svg>
                    )
                  }
                >
                  {product?.stockQuantity === 0 ? 'Temporarily Unavailable' : 'Add to Cart'}
                </Button>
              </div>
            </>
          )}

          {/* Properties */}
          <div className="glass-card p-5">
            <h2 className="text-sm font-semibold text-white mb-4 flex items-center gap-2">
              <svg className="w-4 h-4 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
              Specifications
            </h2>
            {loadingProps ? (
              <div className="space-y-2">
                <Skeleton className="h-4 w-full" count={4} />
              </div>
            ) : properties && properties.length > 0 ? (
              <div className="divide-y divide-white/5">
                {properties.map((prop) => (
                  <div key={prop.id} className="flex justify-between py-2.5 text-sm">
                    <span className="text-slate-400">{prop.name}</span>
                    <span className="text-white text-right max-w-[60%]">
                      {prop.values?.map((v) => v.text).join(', ') || '—'}
                    </span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-slate-500 text-sm">No specifications available.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductPage;
