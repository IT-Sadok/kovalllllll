import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../../api/errors';
import { getProduct } from '../../api/products';
import { setProductAttributes } from '../../api/admin';
import Button from '../../components/ui/Button';
import Skeleton from '../../components/ui/Skeleton';
import type { ProductAttribute } from '../../types';

const inputClass =
  'w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20';

const AdminProductAttributesPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [draft, setDraft] = useState<ProductAttribute[] | null>(null);

  const { data: product, isLoading } = useQuery({
    queryKey: ['product', id],
    queryFn: () => getProduct(id!),
    enabled: !!id,
  });

  const rows = draft ?? product?.attributes ?? [];

  const saveMutation = useMutation({
    mutationFn: (attributes: ProductAttribute[]) => setProductAttributes(id!, attributes),
    onSuccess: () => {
      setDraft(null);
      queryClient.invalidateQueries({ queryKey: ['product', id] });
      toast.success('Attributes saved');
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Failed to save attributes')),
  });

  const updateRow = (index: number, patch: Partial<ProductAttribute>) =>
    setDraft(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));

  const removeRow = (index: number) => setDraft(rows.filter((_, i) => i !== index));

  const moveRow = (index: number, offset: number) => {
    const target = index + offset;
    if (target < 0 || target >= rows.length) return;
    const next = [...rows];
    [next[index], next[target]] = [next[target], next[index]];
    setDraft(next);
  };

  return (
    <div className="page-enter max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex items-center gap-4 mb-8">
        <button
          onClick={() => navigate('/admin/products')}
          id="attributes-back-btn"
          className="text-slate-400 hover:text-white transition-colors cursor-pointer"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <div>
          <h1 className="text-2xl font-bold text-white font-orbitron">
            Product <span className="text-fuchsia-400">Attributes</span>
          </h1>
          <p className="text-slate-400 text-sm">{product?.name ?? `Product ID: ${id?.slice(0, 8)}...`}</p>
        </div>
      </div>

      {isLoading ? (
        <Skeleton className="h-10 w-full" count={4} />
      ) : (
        <div className="glass-card p-6 space-y-3">
          {rows.length === 0 && (
            <p className="text-slate-500 text-sm">No attributes yet. Add details like wheelbase, material or cable length.</p>
          )}

          {rows.map((row, index) => (
            <div key={index} className="flex items-center gap-2" id={`attribute-row-${index}`}>
              <input
                aria-label="Attribute name"
                value={row.name}
                placeholder="Name"
                onChange={(e) => updateRow(index, { name: e.target.value })}
                className={`${inputClass} sm:w-2/5`}
              />
              <input
                aria-label="Attribute value"
                value={row.value}
                placeholder="Value"
                onChange={(e) => updateRow(index, { value: e.target.value })}
                className={inputClass}
              />
              <div className="flex shrink-0 gap-1">
                <button
                  type="button"
                  title="Move up"
                  disabled={index === 0}
                  onClick={() => moveRow(index, -1)}
                  className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-white/5 disabled:opacity-30 cursor-pointer"
                >
                  ↑
                </button>
                <button
                  type="button"
                  title="Move down"
                  disabled={index === rows.length - 1}
                  onClick={() => moveRow(index, 1)}
                  className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-white/5 disabled:opacity-30 cursor-pointer"
                >
                  ↓
                </button>
                <button
                  type="button"
                  title="Remove"
                  onClick={() => removeRow(index)}
                  className="p-1.5 rounded-lg text-red-400 hover:bg-red-500/10 cursor-pointer"
                >
                  ✕
                </button>
              </div>
            </div>
          ))}

          <div className="flex justify-between pt-3">
            <Button
              variant="outline"
              size="sm"
              id="attribute-add-btn"
              onClick={() => setDraft([...rows, { name: '', value: '' }])}
            >
              Add attribute
            </Button>
            <Button
              size="sm"
              id="attributes-save-btn"
              disabled={!draft}
              loading={saveMutation.isPending}
              onClick={() => saveMutation.mutate(rows)}
            >
              Save
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminProductAttributesPage;
