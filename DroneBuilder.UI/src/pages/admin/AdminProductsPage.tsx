import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import toast from 'react-hot-toast';
import { getProducts, getCategories } from '../../api/products';
import { adminCreateProduct, adminUpdateProduct, adminDeleteProduct } from '../../api/admin';
import type { Product, CreateProductRequest, UpdateProductRequest } from '../../types';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import Modal from '../../components/ui/Modal';
import Pagination from '../../components/ui/Pagination';
import EmptyState from '../../components/ui/EmptyState';
import { TableRowSkeleton } from '../../components/ui/Skeleton';

const DEFAULT_CATEGORIES = ['Racing', 'Photography', 'Industrial', 'Military', 'Consumer', 'FPV'];

// Create schema — matches CreateProductRequest (no description field)
const createSchema = z.object({
  name: z.string().min(2, 'Name is required'),
  price: z.coerce.number().positive('Price must be positive'),
  category: z.string().min(1, 'Category is required'),
});

// Update schema — all fields optional
const updateSchema = z.object({
  name: z.string().min(2, 'Name is required').optional(),
  price: z.coerce.number().positive('Price must be positive').optional(),
  category: z.string().min(1, 'Category is required').optional(),
});

type CreateFormData = z.infer<typeof createSchema>;
type UpdateFormData = z.infer<typeof updateSchema>;

const AdminProductsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [modalType, setModalType] = useState<'create' | 'edit' | null>(null);
  const [editTarget, setEditTarget] = useState<Product | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null);
  const [search, setSearch] = useState('');
  
  // Custom category toggle
  const [isCustomCategory, setIsCustomCategory] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ['admin-products', page, search],
    queryFn: () => getProducts({ page, pageSize: 15, name: search || undefined }),
  });

  const { data: serverCategories } = useQuery({
    queryKey: ['categories'],
    queryFn: getCategories,
  });

  // Combine and deduplicate default and server categories
  const categoriesList = Array.from(new Set([...DEFAULT_CATEGORIES, ...(serverCategories || [])]));

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const createForm = useForm<CreateFormData>({ resolver: zodResolver(createSchema) as any });
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const updateForm = useForm<UpdateFormData>({ resolver: zodResolver(updateSchema) as any });

  // POST /products — requires all required fields + properties (empty array for simple product)
  const createMutation = useMutation({
    mutationFn: (data: CreateFormData) => {
      const payload: CreateProductRequest = {
        name: data.name,
        price: data.price,
        category: data.category,
        properties: { name: '', values: [] }, // required by API structure
      };
      return adminCreateProduct(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-products'] });
      toast.success('Product created!');
      setModalType(null);
      createForm.reset();
    },
    onError: () => toast.error('Failed to create product'),
  });

  // PATCH /products/{id} — all fields optional
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductRequest }) =>
      adminUpdateProduct(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-products'] });
      toast.success('Product updated!');
      setModalType(null);
      setEditTarget(null);
      updateForm.reset();
    },
    onError: () => toast.error('Failed to update product'),
  });

  const deleteMutation = useMutation({
    mutationFn: adminDeleteProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-products'] });
      toast.success('Product deleted');
      setDeleteTarget(null);
    },
    onError: () => toast.error('Failed to delete product'),
  });

  const openCreate = () => {
    createForm.reset({ name: '', price: 0, category: '' });
    setIsCustomCategory(false);
    setEditTarget(null);
    setModalType('create');
  };

  const openEdit = (product: Product) => {
    const isStandard = categoriesList.includes(product.category);
    setIsCustomCategory(!isStandard);
    updateForm.reset({
      name: product.name,
      price: product.price,
      category: product.category,
    });
    setEditTarget(product);
    setModalType('edit');
  };

  const onCreateSubmit = (data: CreateFormData) => {
    createMutation.mutate(data);
  };

  const onUpdateSubmit = (data: UpdateFormData) => {
    if (editTarget) {
      updateMutation.mutate({ id: editTarget.id, data });
    }
  };

  return (
    <div className="page-enter max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold text-white font-orbitron">
            Products <span className="text-cyan-400">Management</span>
          </h1>
          <p className="text-slate-400 text-sm mt-1">{data?.totalCount ?? 0} total products</p>
        </div>
        <Button onClick={openCreate} id="admin-create-product-btn" icon={
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
        }>
          Add Product
        </Button>
      </div>

      {/* Search */}
      <div className="mb-4 max-w-sm">
        <Input
          id="admin-product-search"
          placeholder="Search products..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          icon={
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          }
        />
      </div>

      {/* Table */}
      <div className="glass-card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-white/5">
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">Category</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">Price</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-slate-400 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {isLoading
                ? Array.from({ length: 8 }).map((_, i) => <TableRowSkeleton key={i} cols={4} />)
                : data?.items.map((product) => (
                    <tr key={product.id} className="hover:bg-white/2 transition-colors" id={`admin-product-row-${product.id}`}>
                      <td className="px-4 py-3">
                        <span className="text-sm font-medium text-white">{product.name}</span>
                      </td>
                      <td className="px-4 py-3">
                        <span className="text-xs px-2 py-1 rounded-full bg-slate-700/50 text-slate-300">
                          {product.category}
                        </span>
                      </td>
                      <td className="px-4 py-3 font-orbitron text-cyan-400 font-bold text-sm">
                        ${product.price.toLocaleString()}
                      </td>
                          <td className="px-4 py-3">
                        <div className="flex items-center justify-end gap-2">
                          <Link
                            to={`/admin/products/${product.id}/properties`}
                            id={`admin-props-${product.id}`}
                            className="text-xs px-2.5 py-1 rounded-lg border border-fuchsia-500/30 text-fuchsia-400 hover:bg-fuchsia-500/10 transition-all"
                          >
                            Props
                          </Link>
                          <Link
                            to={`/admin/products/${product.id}/images`}
                            id={`admin-images-${product.id}`}
                            className="text-xs px-2.5 py-1 rounded-lg border border-violet-500/30 text-violet-400 hover:bg-violet-500/10 transition-all"
                          >
                            Images
                          </Link>
                          <button
                            onClick={() => openEdit(product)}
                            id={`admin-edit-${product.id}`}
                            className="text-xs px-2.5 py-1 rounded-lg border border-cyan-500/30 text-cyan-400 hover:bg-cyan-500/10 transition-all cursor-pointer"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => setDeleteTarget(product)}
                            id={`admin-delete-${product.id}`}
                            className="text-xs px-2.5 py-1 rounded-lg border border-red-500/30 text-red-400 hover:bg-red-500/10 transition-all cursor-pointer"
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
            </tbody>
          </table>
          {!isLoading && data?.items.length === 0 && (
            <EmptyState title="No products found" description="Add your first product to get started" />
          )}
        </div>
      </div>

      {data && (
        <Pagination page={page} totalCount={data.totalCount} pageSize={15} onPageChange={setPage} />
      )}

      {/* Create Modal */}
      <Modal
        isOpen={modalType === 'create'}
        onClose={() => { setModalType(null); }}
        title="Add New Product"
      >
        <form onSubmit={createForm.handleSubmit(onCreateSubmit)} className="space-y-4">
          <Input
            label="Name"
            id="product-form-name"
            placeholder="DJI Phantom X"
            {...createForm.register('name')}
            error={createForm.formState.errors.name?.message}
          />
          <Input
            label="Price ($)"
            id="product-form-price"
            type="number"
            placeholder="999"
            {...createForm.register('price')}
            error={createForm.formState.errors.price?.message}
          />
          <div className="space-y-1.5">
            <div className="flex justify-between items-center">
              <label className="text-sm font-medium text-slate-300">Category</label>
              <button 
                type="button" 
                onClick={() => {
                  setIsCustomCategory(!isCustomCategory);
                  createForm.setValue('category', ''); // Reset on toggle
                }}
                className="text-xs text-cyan-400 hover:text-cyan-300"
              >
                {isCustomCategory ? 'Use existing category' : '+ Add new category'}
              </button>
            </div>
            
            {isCustomCategory ? (
              <input
                id="product-form-category-custom"
                placeholder="Type new category..."
                {...createForm.register('category')}
                className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2.5 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20"
              />
            ) : (
              <select
                id="product-form-category"
                {...createForm.register('category')}
                className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2.5 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20"
              >
                <option value="" disabled>Select a category</option>
                {categoriesList.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            )}
            
            {createForm.formState.errors.category && (
              <p className="text-xs text-red-400">{createForm.formState.errors.category.message}</p>
            )}
          </div>
          <div className="flex gap-3 pt-2">
            <Button type="button" variant="ghost" onClick={() => setModalType(null)} id="product-form-cancel">
              Cancel
            </Button>
            <Button
              type="submit"
              loading={createMutation.isPending}
              fullWidth
              id="product-form-submit"
            >
              Create Product
            </Button>
          </div>
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal
        isOpen={modalType === 'edit'}
        onClose={() => { setModalType(null); setEditTarget(null); }}
        title="Edit Product"
      >
        <form onSubmit={updateForm.handleSubmit(onUpdateSubmit)} className="space-y-4">
          <Input
            label="Name"
            id="product-edit-name"
            placeholder="DJI Phantom X"
            {...updateForm.register('name')}
            error={updateForm.formState.errors.name?.message}
          />
          <Input
            label="Price ($)"
            id="product-edit-price"
            type="number"
            placeholder="999"
            {...updateForm.register('price')}
            error={updateForm.formState.errors.price?.message}
          />
          <div className="space-y-1.5">
            <div className="flex justify-between items-center">
              <label className="text-sm font-medium text-slate-300">Category</label>
              <button 
                type="button" 
                onClick={() => {
                  setIsCustomCategory(!isCustomCategory);
                  if(!isCustomCategory) updateForm.setValue('category', ''); 
                }}
                className="text-xs text-cyan-400 hover:text-cyan-300"
              >
                {isCustomCategory ? 'Use existing category' : '+ Add new category'}
              </button>
            </div>
            
            {isCustomCategory ? (
              <input
                id="product-form-category-update-custom"
                placeholder="Type new category..."
                {...updateForm.register('category')}
                className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2.5 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20"
              />
            ) : (
              <select
                id="product-form-category-update"
                {...updateForm.register('category')}
                className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2.5 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20"
              >
                <option value="" disabled>Select a category</option>
                {categoriesList.map((c) => <option key={c} value={c}>{c}</option>)}
              </select>
            )}
            
            {updateForm.formState.errors.category && (
              <p className="text-xs text-red-400">{updateForm.formState.errors.category.message}</p>
            )}
          </div>
          <div className="flex gap-3 pt-2">
            <Button type="button" variant="ghost" onClick={() => { setModalType(null); setEditTarget(null); }} id="product-edit-cancel">
              Cancel
            </Button>
            <Button
              type="submit"
              loading={updateMutation.isPending}
              fullWidth
              id="product-edit-submit"
            >
              Update Product
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete confirmation */}
      <Modal isOpen={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="Delete Product" size="sm">
        <p className="text-slate-300 mb-2">
          Are you sure you want to delete <strong className="text-white">{deleteTarget?.name}</strong>?
        </p>
        <p className="text-sm text-slate-500 mb-6">This action cannot be undone.</p>
        <div className="flex gap-3">
          <Button variant="ghost" onClick={() => setDeleteTarget(null)} id="delete-cancel">Cancel</Button>
          <Button
            variant="danger"
            fullWidth
            loading={deleteMutation.isPending}
            onClick={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
            id="delete-confirm"
          >
            Delete
          </Button>
        </div>
      </Modal>
    </div>
  );
};

export default AdminProductsPage;
