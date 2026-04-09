import React, { useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { getProducts, getCategories } from '../api/products';
import { addToCart } from '../api/cart';
import { useCartStore } from '../store/cartStore';
import { useAuthStore } from '../store/authStore';
import type { Product, ProductFilters } from '../types';
import { ProductCardSkeleton } from '../components/ui/Skeleton';
import Pagination from '../components/ui/Pagination';
import EmptyState from '../components/ui/EmptyState';
import Button from '../components/ui/Button';
import Input from '../components/ui/Input';

const DEFAULT_CATEGORIES = ['Racing', 'Photography', 'Industrial', 'Military', 'Consumer', 'FPV'];

interface FilterForm {
  name: string;
  minPrice: string;
  maxPrice: string;
  category: string;
}

const ProductCard: React.FC<{ product: Product }> = ({ product }) => {
  const { user } = useAuthStore();
  const { increment } = useCartStore();
  const [adding, setAdding] = useState(false);

  const handleAddToCart = async (e: React.MouseEvent) => {
    e.preventDefault();
    if (!user) {
      toast.error('Please sign in to add items to cart');
      return;
    }
    setAdding(true);
    try {
      await addToCart(product.id, 1);
      increment();
      toast.success(`${product.name} added to cart!`);
    } catch {
      toast.error('Failed to add to cart');
    } finally {
      setAdding(false);
    }
  };

  const primaryImage = product.images?.find(img => img.isPrimary) || product.images?.[0];
  const imageUrl = primaryImage?.url;

  return (
    <Link to={`/products/${product.id}`} id={`product-card-${product.id}`} className={`block group ${product.stockQuantity === 0 ? 'pointer-events-none' : ''}`}>
      <div className={`glass-card overflow-hidden h-full flex flex-col transition-all duration-300 ${product.stockQuantity === 0 ? 'opacity-60 grayscale' : ''}`}>
        {/* Image */}
        <div className="relative h-48 bg-gradient-to-br from-slate-800 to-slate-900 overflow-hidden">
          {imageUrl ? (
            <img
              src={imageUrl}
              alt={product.name}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center">
              <svg className="w-20 h-20 text-slate-700" viewBox="0 0 32 32" fill="none">
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
          {/* Category tag */}
          {product.category && (
            <span className="absolute top-2 left-2 text-xs px-2 py-1 rounded-md bg-black/60 backdrop-blur-sm text-cyan-400 border border-cyan-500/20 font-medium">
              {product.category}
            </span>
          )}
          {/* Out of Stock Badge */}
          {product.stockQuantity === 0 && (
            <div className="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[2px]">
              <span className="px-4 py-1.5 rounded-full bg-red-500/20 border border-red-500/40 text-red-400 text-[10px] font-bold uppercase tracking-widest font-orbitron shadow-[0_0_15px_rgba(239,68,68,0.2)]">
                Out of Stock
              </span>
            </div>
          )}
        </div>

        {/* Content */}
        <div className="p-4 flex flex-col flex-1 gap-3">
          <h3 className="font-semibold text-white text-sm leading-tight group-hover:text-cyan-400 transition-colors line-clamp-2">
            {product.name}
          </h3>

          <div className="mt-auto flex items-center justify-between">
            <span className="text-xl font-bold text-cyan-400 font-orbitron">
              ${product.price.toLocaleString()}
            </span>
            <button
              onClick={handleAddToCart}
              disabled={adding || product.stockQuantity === 0}
              id={`add-to-cart-${product.id}`}
              className={`flex items-center gap-1.5 text-xs px-3 py-1.5 rounded-lg border transition-all duration-200 ${
                product.stockQuantity === 0 
                ? 'bg-slate-800/50 border-slate-700 text-slate-500 cursor-not-allowed'
                : 'bg-cyan-500/10 border-cyan-500/20 text-cyan-400 hover:bg-cyan-500/20 hover:border-cyan-500/40 cursor-pointer'
              } disabled:opacity-50`}
            >
              {adding ? (
                <svg className="animate-spin w-3 h-3" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
                </svg>
              ) : product.stockQuantity === 0 ? (
                <span>Sold Out</span>
              ) : (
                <>
                  <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  <span>Add</span>
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </Link>
  );
};

const HomePage: React.FC = () => {
  const [filters, setFilters] = useState<ProductFilters>({ page: 1, pageSize: 12 });

  const { register, handleSubmit, reset } = useForm<FilterForm>({
    defaultValues: { name: '', minPrice: '', maxPrice: '', category: '' },
  });

  const { data, isLoading, isError } = useQuery({
    queryKey: ['products', filters],
    queryFn: () => getProducts(filters),
  });

  const { data: serverCategories } = useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });

  // Combine and deduplicate default and server categories
  const categoriesList = Array.from(new Set([...DEFAULT_CATEGORIES, ...(serverCategories || [])]));

  const onFilter = useCallback((form: FilterForm) => {
    setFilters((prev) => ({
      ...prev,
      page: 1,
      name: form.name || undefined,
      minPrice: form.minPrice ? Number(form.minPrice) : '',
      maxPrice: form.maxPrice ? Number(form.maxPrice) : '',
      category: form.category || undefined,
    }));
  }, []);

  const onReset = () => {
    reset();
    setFilters({ page: 1, pageSize: 12 });
  };

  return (
    <div className="page-enter max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Hero */}
      <div className="mb-12 relative">
        <div className="absolute inset-0 pointer-events-none">
          <div className="absolute -top-10 left-1/4 w-72 h-72 rounded-full bg-cyan-500/5 blur-3xl" />
          <div className="absolute -top-10 right-1/4 w-72 h-72 rounded-full bg-violet-500/5 blur-3xl" />
        </div>
        <h1 className="text-4xl sm:text-5xl font-bold font-orbitron text-white mb-3">
          Professional <span className="text-cyan-400 glow-text">Drones</span>
        </h1>
        <p className="text-slate-400 text-lg">Configure and order professional UAVs for any mission</p>
      </div>

      <div className="flex flex-col lg:flex-row gap-8">
        {/* Filters sidebar */}
        <aside className="w-full lg:w-64 flex-shrink-0">
          <div className="glass-card p-5 sticky top-24">
            <h2 className="text-sm font-semibold text-white mb-4 flex items-center gap-2">
              <svg className="w-4 h-4 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2a1 1 0 01-.293.707L13 13.414V19a1 1 0 01-.553.894l-4 2A1 1 0 017 21v-7.586L3.293 6.707A1 1 0 013 6V4z" />
              </svg>
              Filters
            </h2>
            <form onSubmit={handleSubmit(onFilter)} className="space-y-4">
              <Input
                label="Search"
                id="filter-name"
                placeholder="Drone name..."
                {...register('name')}
              />
              <div className="space-y-2">
                <label className="text-sm font-medium text-slate-300">Price Range</label>
                <div className="flex gap-2">
                  <input
                    type="number"
                    placeholder="Min"
                    id="filter-min-price"
                    {...register('minPrice')}
                    className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-cyan-500/20 focus:border-cyan-500/50 transition-all"
                  />
                  <input
                    type="number"
                    placeholder="Max"
                    id="filter-max-price"
                    {...register('maxPrice')}
                    className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-cyan-500/20 focus:border-cyan-500/50 transition-all"
                  />
                </div>
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-300">Category</label>
                <select
                  id="filter-category"
                  {...register('category')}
                  className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2.5 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20 focus:border-cyan-500/50 transition-all appearance-none cursor-pointer"
                >
                  <option value="">All categories</option>
                  {categoriesList.map((c) => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
              </div>
              <Button type="submit" fullWidth size="sm" id="filter-submit">Apply Filters</Button>
              <button
                type="button"
                onClick={onReset}
                id="filter-reset"
                className="w-full text-xs text-slate-500 hover:text-slate-300 transition-colors cursor-pointer py-1"
              >
                Reset filters
              </button>
            </form>
          </div>
        </aside>

        {/* Products grid */}
        <div className="flex-1">
          {/* Count */}
          {data && (
            <p className="text-sm text-slate-500 mb-4">
              Found <span className="text-white font-medium">{data.totalCount}</span> product{data.totalCount !== 1 ? 's' : ''}
            </p>
          )}

          {isError && (
            <div className="glass-card p-6 text-center text-red-400">
              Failed to load products. Make sure the API is running.
            </div>
          )}

          {isLoading ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-5">
              {Array.from({ length: 6 }).map((_, i) => (
                <ProductCardSkeleton key={i} />
              ))}
            </div>
          ) : data?.items.length === 0 ? (
            <EmptyState
              title="No drones found"
              description="Try adjusting your search filters or browse all products."
              action={
                <Button onClick={onReset} variant="outline" id="empty-reset-btn">Clear Filters</Button>
              }
            />
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-5">
              {data?.items.map((product) => (
                <ProductCard key={product.id} product={product} />
              ))}
            </div>
          )}

          {data && data.totalCount > 0 && (
            <Pagination
              page={filters.page || 1}
              totalCount={data.totalCount}
              pageSize={filters.pageSize || 12}
              onPageChange={(p) => setFilters((prev) => ({ ...prev, page: p }))}
            />
          )}
        </div>
      </div>
    </div>
  );
};

export default HomePage;
