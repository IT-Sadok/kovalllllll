import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../../api/errors';
import { getProduct } from '../../api/products';
import { setProductSpec, removeProductSpec } from '../../api/admin';
import Button from '../../components/ui/Button';
import Skeleton from '../../components/ui/Skeleton';
import type { ComponentSpec } from '../../types';
import { CATEGORY_LABELS, SPEC_FIELDS, emptySpec, optionLabel, toComponentType, type SpecField } from '../../utils/componentSpecs';

const inputClass =
  'w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20';

const AdminProductSpecPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [draft, setDraft] = useState<Record<string, unknown> | null>(null);

  const { data: product, isLoading } = useQuery({
    queryKey: ['product', id],
    queryFn: () => getProduct(id!),
    enabled: !!id,
  });

  const type = product ? toComponentType(product.category) : null;
  const spec: Record<string, unknown> | null =
    draft ?? (product?.spec as Record<string, unknown> | null | undefined) ?? (type ? { ...emptySpec(type) } : null);

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['product', id] });
    queryClient.invalidateQueries({ queryKey: ['admin-products'] });
  };

  const saveMutation = useMutation({
    mutationFn: (value: ComponentSpec) => setProductSpec(id!, value),
    onSuccess: () => {
      setDraft(null);
      invalidate();
      toast.success('Spec saved');
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Failed to save spec')),
  });

  const removeMutation = useMutation({
    mutationFn: () => removeProductSpec(id!),
    onSuccess: () => {
      setDraft(null);
      invalidate();
      toast.success('Spec removed');
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Failed to remove spec')),
  });

  const setField = (key: string, value: unknown) => setDraft({ ...(spec ?? {}), [key]: value });

  const toggleListOption = (key: string, option: string) => {
    const current = (spec?.[key] as string[]) ?? [];
    setField(key, current.includes(option) ? current.filter((o) => o !== option) : [...current, option]);
  };

  const renderField = (field: SpecField) => {
    const value = spec?.[field.key];
    const fieldId = `spec-${field.key}`;

    if (field.kind === 'enumList') {
      return (
        <div className="flex flex-wrap gap-2">
          {field.options.map((option) => {
            const selected = (value as string[]).includes(option);
            return (
              <button
                key={option}
                type="button"
                id={`${fieldId}-${option}`}
                onClick={() => toggleListOption(field.key, option)}
                className={`text-xs px-2.5 py-1 rounded-lg border transition-all cursor-pointer ${
                  selected
                    ? 'border-cyan-400 bg-cyan-500/15 text-cyan-300'
                    : 'border-white/10 text-slate-400 hover:border-white/30'
                }`}
              >
                {optionLabel(option)}
              </button>
            );
          })}
        </div>
      );
    }

    if (field.kind === 'enum') {
      return (
        <select
          id={fieldId}
          value={(value as string | null) ?? ''}
          onChange={(e) => setField(field.key, e.target.value === '' ? null : e.target.value)}
          className={inputClass}
        >
          {field.optional && <option value="">Unknown</option>}
          {field.options.map((option) => (
            <option key={option} value={option}>{optionLabel(option)}</option>
          ))}
        </select>
      );
    }

    if (field.kind === 'text') {
      return (
        <input
          id={fieldId}
          value={value as string}
          placeholder={field.placeholder}
          onChange={(e) => setField(field.key, e.target.value)}
          className={inputClass}
        />
      );
    }

    return (
      <div className="flex items-center gap-2">
        <input
          id={fieldId}
          type="number"
          step={field.step ?? 1}
          value={(value as number | null) ?? ''}
          placeholder={field.optional ? 'Optional' : undefined}
          onChange={(e) => setField(field.key, e.target.value === '' ? (field.optional ? null : 0) : Number(e.target.value))}
          className={inputClass}
        />
        {field.unit && <span className="text-slate-500 text-sm w-10">{field.unit}</span>}
      </div>
    );
  };

  return (
    <div className="page-enter max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex items-center gap-4 mb-8">
        <button
          onClick={() => navigate('/admin/products')}
          id="spec-back-btn"
          className="text-slate-400 hover:text-white transition-colors cursor-pointer"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <div>
          <h1 className="text-2xl font-bold text-white font-orbitron">
            Component <span className="text-violet-400">Spec</span>
          </h1>
          <p className="text-slate-400 text-sm">{product?.name ?? `Product ID: ${id?.slice(0, 8)}...`}</p>
        </div>
      </div>

      {isLoading ? (
        <Skeleton className="h-10 w-full" count={5} />
      ) : (
        <div className="glass-card p-6 space-y-5">
          <p className="text-sm text-slate-400">
            Category: <span className="text-cyan-400 font-medium">{product && CATEGORY_LABELS[product.category]}</span>
          </p>

          {!type && (
            <p className="text-slate-500 text-sm">Products in this category have no component spec.</p>
          )}

          {type && SPEC_FIELDS[type].map((field) => (
            <div key={field.key} className="flex flex-col gap-1.5">
              <label htmlFor={`spec-${field.key}`} className="text-sm font-medium text-slate-300">{field.label}</label>
              {renderField(field)}
            </div>
          ))}

          {type && (
            <div className="flex justify-between pt-2">
              <Button
                variant="danger"
                size="sm"
                id="spec-remove-btn"
                disabled={!product?.spec}
                loading={removeMutation.isPending}
                onClick={() => removeMutation.mutate()}
              >
                Remove spec
              </Button>
              <Button
                size="sm"
                id="spec-save-btn"
                disabled={!spec || !draft}
                loading={saveMutation.isPending}
                onClick={() => spec && saveMutation.mutate(spec as ComponentSpec)}
              >
                Save
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default AdminProductSpecPage;
