import React, { useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useForm, useWatch } from 'react-hook-form';
import toast from 'react-hot-toast';
import { getManufacturers, getProducts } from '../api/products';
import { useAddToCart } from '../hooks/useAddToCart';
import { useAuthStore } from '../store/authStore';
import type { Product, ProductFilters, ProductSort } from '../types';
import SpecFilters from '../components/catalog/SpecFilters';
import { EMPTY_FILTERS, toSpecFilters, type CatalogFilterForm } from '../utils/catalogFilters';
import { ProductCardSkeleton } from '../components/ui/Skeleton';
import Pagination from '../components/ui/Pagination';
import EmptyState from '../components/ui/EmptyState';
import Button from '../components/ui/Button';
import Input from '../components/ui/Input';
import CategoryOptions from '../components/CategoryOptions';
import { CATEGORY_LABELS } from '../utils/componentSpecs';

const SORT_OPTIONS: { value: ProductSort; label: string }[] = [
  { value: 'Name', label: 'Name' },
  { value: 'PriceAsc', label: 'Price: low to high' },
  { value: 'PriceDesc', label: 'Price: high to low' },
];

const ProductCard: React.FC<{ product: Product }> = ({ product }) => {
  const { user } = useAuthStore();
  const addToCartMutation = useAddToCart();
  const adding = addToCartMutation.isPending;

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault();
    if (!user) {
      toast.error('Please sign in to add items to cart');
      return;
    }
    addToCartMutation.mutate({ productId: product.id, productName: product.name, quantity: 1 });
  };

  const group = product.group;
  const title = group?.name ?? product.name;
  const outOfStock = (group ? group.totalStock : product.stockQuantity) === 0;

  const primaryImage = product.images?.find(img => img.isPrimary) || product.images?.[0];
  const imageUrl = primaryImage?.url;

  return (
    <Link to={`/products/${product.id}`} id={`product-card-${product.id}`} className="block group">
      <div className={`glass-card overflow-hidden h-full flex flex-col transition-all duration-300 ${outOfStock ? 'opacity-60 grayscale' : ''}`}>
        {/* Image */}
        <div className="relative h-48 bg-gradient-to-br from-slate-800 to-slate-900 overflow-hidden">
          {imageUrl ? (
            <img
              src={imageUrl}
              alt={title}
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
              {CATEGORY_LABELS[product.category]}
            </span>
          )}
          {/* Out of Stock Badge */}
          {outOfStock && (
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
            {title}
          </h3>
          {group && (
            <span className="text-xs text-slate-400" id={`product-variants-${product.id}`}>
              {group.variantCount} variants
            </span>
          )}

          <div className="mt-auto flex items-center justify-between">
            <span className="text-xl font-bold text-cyan-400 font-orbitron">
              {group && group.minPrice !== group.maxPrice && <span className="text-xs text-slate-400 font-sans mr-1">from</span>}
              ${(group ? group.minPrice : product.price).toLocaleString()}
            </span>
            {group ? (
              <span
                id={`choose-variant-${product.id}`}
                className="text-xs px-3 py-1.5 rounded-lg border bg-cyan-500/10 border-cyan-500/20 text-cyan-400"
              >
                Choose
              </span>
            ) : (
              <button
                onClick={handleAddToCart}
                disabled={adding || outOfStock}
                id={`add-to-cart-${product.id}`}
                className={`flex items-center gap-1.5 text-xs px-3 py-1.5 rounded-lg border transition-all duration-200 ${
                  outOfStock 
                  ? 'bg-slate-800/50 border-slate-700 text-slate-500 cursor-not-allowed'
                  : 'bg-cyan-500/10 border-cyan-500/20 text-cyan-400 hover:bg-cyan-500/20 hover:border-cyan-500/40 cursor-pointer'
                } disabled:opacity-50`}
              >
                {adding ? (
                  <svg className="animate-spin w-3 h-3" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
                  </svg>
                ) : outOfStock ? (
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
            )}
          </div>
        </div>
      </div>
    </Link>
  );
};

const HomePage: React.FC = () => {
  const [filters, setFilters] = useState<ProductFilters>({ page: 1, pageSize: 12 });

  const { register, handleSubmit, reset, control, setValue } = useForm<CatalogFilterForm>({
    defaultValues: EMPTY_FILTERS,
  });
  const category = useWatch({ control, name: 'category' });

  const { data, isLoading, isError } = useQuery({
    queryKey: ['products', filters],
    queryFn: () => getProducts({ ...filters, collapseVariants: true }),
  });

  const { data: manufacturers } = useQuery({
    queryKey: ['manufacturers', category],
    queryFn: () => getManufacturers(category || undefined),
  });

  const onFilter = useCallback((form: CatalogFilterForm) => {
    setFilters((prev) => ({
      page: 1,
      pageSize: prev.pageSize,
      sort: prev.sort,
      name: form.name || undefined,
      minPrice: form.minPrice ? Number(form.minPrice) : '',
      maxPrice: form.maxPrice ? Number(form.maxPrice) : '',
      category: form.category || undefined,
      manufacturer: form.manufacturer || undefined,
      inStock: form.inStock || undefined,
      ...toSpecFilters(form.category, form),
    }));
  }, []);

  const onReset = () => {
    reset();
    setFilters((prev) => ({ page: 1, pageSize: 12, sort: prev.sort }));
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
          Build your <span className="text-cyan-400 glow-text">FPV quad</span>
        </h1>
        <p className="text-slate-400 text-lg max-w-2xl">
          Browse real parts with full specs, then put them together in the builder. Fit, weight and thrust to weight
          are checked as you go.
        </p>
        <Link
          to="/builder"
          id="hero-open-builder"
          className="relative inline-block mt-5 text-sm px-5 py-2.5 rounded-xl bg-cyan-500/10 border border-cyan-500/30 text-cyan-400 hover:bg-cyan-500/20 transition-all"
        >
          Open the builder →
        </Link>
      </div>

      <div className="flex flex-col lg:flex-row gap-8">
        {/* Filters sidebar */}
        <aside className="w-full lg:w-64 flex-shrink-0">
          <div className="glass-card p-5 lg:sticky lg:top-24 lg:max-h-[calc(100vh-7rem)] lg:overflow-y-auto">
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
                placeholder="Part name..."
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
                  {...register('category', { onChange: () => setValue('manufacturer', '') })}
                  className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2.5 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20 focus:border-cyan-500/50 transition-all appearance-none cursor-pointer"
                >
                  <option value="">All categories</option>
                  <CategoryOptions />
                </select>
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium text-slate-300">Brand</label>
                <select
                  id="filter-manufacturer"
                  {...register('manufacturer')}
                  className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2.5 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20 focus:border-cyan-500/50 transition-all appearance-none cursor-pointer"
                >
                  <option value="">All brands</option>
                  {manufacturers?.map((manufacturer) => (
                    <option key={manufacturer} value={manufacturer}>{manufacturer}</option>
                  ))}
                </select>
              </div>
              <SpecFilters category={category} register={register} />
              <label className="flex items-center gap-2 text-sm text-slate-300 cursor-pointer">
                <input type="checkbox" id="filter-in-stock" {...register('inStock')} className="accent-cyan-400" />
                In stock only
              </label>
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
          <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
            <p className="text-sm text-slate-500">
              {data && (
                <>Found <span className="text-white font-medium">{data.totalCount}</span> product{data.totalCount !== 1 ? 's' : ''}</>
              )}
            </p>
            <label className="flex items-center gap-2 text-sm text-slate-400">
              Sort by
              <select
                id="catalog-sort"
                value={filters.sort ?? 'Name'}
                onChange={(e) => setFilters((prev) => ({ ...prev, page: 1, sort: e.target.value as ProductSort }))}
                className="bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-1.5 text-sm text-slate-100 cursor-pointer"
              >
                {SORT_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
            </label>
          </div>

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
              title="No parts found"
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
