import React, { useState } from 'react';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { getProducts } from '../../api/products';
import type { ComponentType, Product } from '../../types';
import { CATEGORY_LABELS, specSummary } from '../../utils/componentSpecs';
import Modal from '../ui/Modal';
import Pagination from '../ui/Pagination';
import Skeleton from '../ui/Skeleton';
import { formatMoney } from '../../utils/money';

const PAGE_SIZE = 8;

interface PartPickerProps {
  slot: ComponentType | null;
  selectedId?: string;
  otherPartIds: string[];
  onPick: (product: Product) => void;
  onClose: () => void;
}

const PartPicker: React.FC<PartPickerProps> = ({ slot, selectedId, otherPartIds, onPick, onClose }) => {
  const [name, setName] = useState('');
  const [page, setPage] = useState(1);
  const [onlyCompatible, setOnlyCompatible] = useState(true);
  const compatibleWith = onlyCompatible && otherPartIds.length > 0 ? otherPartIds : undefined;

  const { data, isLoading, isError } = useQuery({
    queryKey: ['builder-parts', slot, name, page, compatibleWith],
    queryFn: () => getProducts({ category: slot!, name, page, pageSize: PAGE_SIZE, collapseVariants: true, compatibleWith }),
    enabled: slot !== null,
    placeholderData: keepPreviousData,
  });

  const close = () => {
    setName('');
    setPage(1);
    onClose();
  };

  return (
    <Modal isOpen={slot !== null} onClose={close} title={slot ? `Choose ${CATEGORY_LABELS[slot].toLowerCase()}` : ''} size="xl">
      <input
        autoFocus
        id="part-picker-search"
        value={name}
        onChange={(e) => {
          setName(e.target.value);
          setPage(1);
        }}
        placeholder="Search by name..."
        className="w-full mb-3 bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-cyan-500/20 focus:border-cyan-500/50"
      />
      {otherPartIds.length > 0 && (
        <label className="flex items-center gap-2 mb-4 text-sm text-slate-300 cursor-pointer">
          <input
            type="checkbox"
            id="part-picker-compatible"
            checked={onlyCompatible}
            onChange={(e) => {
              setOnlyCompatible(e.target.checked);
              setPage(1);
            }}
            className="accent-cyan-400"
          />
          Only parts that fit my build
          <span className="text-xs text-slate-500">(parts without specs are hidden)</span>
        </label>
      )}

      <div className="max-h-[60vh] overflow-y-auto space-y-2 pr-1">
        {isError && <p className="text-sm text-red-400">Failed to load parts.</p>}
        {isLoading && <Skeleton className="h-16 w-full" count={4} />}
        {data?.items.length === 0 && (
          <p className="text-sm text-slate-500 py-6 text-center">
            {compatibleWith ? 'No parts fit the rest of your build. Untick the filter to see everything.' : 'No parts found.'}
          </p>
        )}
        {data?.items.map((product) => {
          const image = product.images?.find((img) => img.isPrimary) ?? product.images?.[0];
          const group = product.group;
          const stock = group ? group.totalStock : product.stockQuantity;
          const selected = product.id === selectedId;
          return (
            <button
              key={product.id}
              type="button"
              id={`pick-${product.id}`}
              onClick={() => {
                onPick(product);
                close();
              }}
              className={`w-full flex items-center gap-3 p-2 rounded-xl border text-left transition-all cursor-pointer ${
                selected ? 'border-cyan-400 bg-cyan-500/10' : 'border-white/5 hover:border-cyan-500/30 hover:bg-white/5'
              }`}
            >
              <div className="w-14 h-14 flex-shrink-0 rounded-lg overflow-hidden bg-slate-800">
                {image && <img src={image.url} alt="" className="w-full h-full object-cover" />}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm text-white truncate">{group?.name ?? product.name}</p>
                <p className="text-xs text-slate-400 truncate">
                  {product.spec ? specSummary(product.spec) : 'No spec — compatibility cannot be checked'}
                </p>
                {group && <p className="text-xs text-slate-500">{group.variantCount} variants</p>}
              </div>
              <div className="text-right flex-shrink-0">
                <p className="text-sm font-semibold text-cyan-400">
                  {group && group.minPrice !== group.maxPrice && <span className="text-xs text-slate-400 mr-1">from</span>}
                  {formatMoney(group ? group.minPrice : product.price)}
                </p>
                {stock === 0 && <p className="text-[10px] uppercase tracking-wider text-red-400">Out of stock</p>}
              </div>
            </button>
          );
        })}
      </div>

      {data && (
        <Pagination page={page} totalCount={data.totalCount} pageSize={PAGE_SIZE} onPageChange={setPage} />
      )}
    </Modal>
  );
};

export default PartPicker;
