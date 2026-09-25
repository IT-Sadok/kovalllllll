import React from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { getProduct } from '../../api/products';
import type { BuildItem, ComponentType, IssueSeverity } from '../../types';
import { CATEGORY_LABELS, specSummary } from '../../utils/componentSpecs';
import { formatMoney } from '../../utils/money';
import Skeleton from '../ui/Skeleton';

const SEVERITY_BORDER: Record<IssueSeverity, string> = {
  Error: 'border-red-500/50',
  Warning: 'border-amber-500/50',
  Info: 'border-sky-500/40',
};

interface PartSlotProps {
  slot: ComponentType;
  hint?: string;
  part?: BuildItem;
  severity?: IssueSeverity;
  quantityEditable: boolean;
  onChoose: () => void;
  onSelectVariant: (productId: string) => void;
  onQuantityChange: (quantity: number) => void;
  onRemove: () => void;
}

const PartSlot: React.FC<PartSlotProps> = ({
  slot, hint, part, severity, quantityEditable, onChoose, onSelectVariant, onQuantityChange, onRemove,
}) => {
  const { data: product, isLoading, isError } = useQuery({
    queryKey: ['product', part?.productId],
    queryFn: () => getProduct(part!.productId),
    enabled: !!part,
  });

  const border = severity ? SEVERITY_BORDER[severity] : 'border-white/5';
  const variants = product?.group?.variants ?? [];
  const image = product?.images?.find((img) => img.isPrimary) ?? product?.images?.[0];

  return (
    <div id={`slot-${slot}`} className={`glass-card p-4 border ${border}`}>
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className="text-xs uppercase tracking-wider text-slate-400">{CATEGORY_LABELS[slot]}</p>
          {!part && hint && <p className="text-xs text-slate-500 mt-0.5">{hint}</p>}
        </div>
        <div className="flex items-center gap-3">
          {part && (
            <button type="button" onClick={onRemove} id={`slot-remove-${slot}`} className="text-xs text-slate-500 hover:text-red-400 cursor-pointer">
              Remove
            </button>
          )}
          <button
            type="button"
            onClick={onChoose}
            id={`slot-choose-${slot}`}
            className="text-xs px-3 py-1.5 rounded-lg border bg-cyan-500/10 border-cyan-500/20 text-cyan-400 hover:bg-cyan-500/20 cursor-pointer"
          >
            {part ? 'Change' : 'Choose'}
          </button>
        </div>
      </div>

      {part && isLoading && <Skeleton className="h-14 w-full mt-3" />}
      {part && isError && (
        <p className="text-sm text-red-400 mt-3">This part is no longer available. Remove it or choose another one.</p>
      )}
      {product && (
        <div className="flex items-start gap-3 mt-3">
          <div className="w-16 h-16 flex-shrink-0 rounded-lg overflow-hidden bg-slate-800">
            {image && <img src={image.url} alt="" className="w-full h-full object-cover" />}
          </div>
          <div className="flex-1 min-w-0 space-y-1">
            <Link to={`/products/${product.id}`} className="block text-sm text-white hover:text-cyan-400 truncate">
              {product.group?.name ?? product.name}
            </Link>
            <p className="text-xs text-slate-400">
              {product.spec ? specSummary(product.spec) : 'No spec — compatibility cannot be checked'}
            </p>
            {variants.length > 1 && (
              <select
                id={`slot-variant-${slot}`}
                value={product.id}
                onChange={(e) => onSelectVariant(e.target.value)}
                className="max-w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-lg px-2 py-1 text-xs text-slate-100 cursor-pointer"
              >
                {variants.map((variant) => (
                  <option key={variant.id} value={variant.id}>
                    {variant.variantName ?? 'Default'} — {formatMoney(variant.price)}
                    {variant.stockQuantity === 0 ? ' (out of stock)' : ''}
                  </option>
                ))}
              </select>
            )}
          </div>
          <div className="text-right flex-shrink-0 space-y-1">
            <p className="text-sm font-semibold text-cyan-400">{formatMoney(product.price * part!.quantity)}</p>
            {quantityEditable ? (
              <label className="flex items-center gap-1 text-xs text-slate-400">
                Qty
                <input
                  type="number"
                  min={1}
                  max={20}
                  id={`slot-quantity-${slot}`}
                  value={part!.quantity}
                  onChange={(e) => onQuantityChange(Math.min(20, Math.max(1, Number(e.target.value) || 1)))}
                  className="w-12 bg-[#111827] border border-white/10 rounded px-1 py-0.5 text-slate-100"
                />
              </label>
            ) : null}
            {product.weightGrams != null && <p className="text-xs text-slate-500">{product.weightGrams} g{part!.quantity > 1 ? ' each' : ''}</p>}
            {product.stockQuantity === 0 && <p className="text-[10px] uppercase tracking-wider text-red-400">Out of stock</p>}
          </div>
        </div>
      )}
    </div>
  );
};

export default PartSlot;
