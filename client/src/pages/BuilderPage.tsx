import React, { useState } from 'react';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { checkBuild } from '../api/builds';
import { getErrorMessage } from '../api/errors';
import PartPicker from '../components/builder/PartPicker';
import PartSlot from '../components/builder/PartSlot';
import { buildItems, useBuilderStore } from '../store/builderStore';
import type { ComponentType, IssueSeverity } from '../types';
import { CATEGORY_LABELS } from '../utils/componentSpecs';
import { formatMoney } from '../utils/money';

const SECTIONS: { title: string; slots: { slot: ComponentType; hint?: string }[] }[] = [
  { title: 'Airframe', slots: [{ slot: 'Frame' }, { slot: 'Motor', hint: 'Four identical motors' }, { slot: 'Propeller' }] },
  {
    title: 'Electronics',
    slots: [
      { slot: 'Stack', hint: 'Flight controller and ESC in one set' },
      { slot: 'FlightController', hint: 'Skip if you chose a stack' },
      { slot: 'Esc', hint: 'Skip if you chose a stack' },
      { slot: 'Receiver' },
    ],
  },
  { title: 'Power', slots: [{ slot: 'Battery' }] },
  { title: 'Video', slots: [{ slot: 'Camera' }, { slot: 'VideoTransmitter' }, { slot: 'Antenna' }] },
];

const QUANTITY_SLOTS: ComponentType[] = ['Motor', 'Propeller', 'Battery'];

const SEVERITY_ORDER: IssueSeverity[] = ['Error', 'Warning', 'Info'];

const SEVERITY_STYLE: Record<IssueSeverity, string> = {
  Error: 'border-red-500/30 bg-red-500/10 text-red-300',
  Warning: 'border-amber-500/30 bg-amber-500/10 text-amber-300',
  Info: 'border-sky-500/30 bg-sky-500/10 text-sky-300',
};

const BuilderPage: React.FC = () => {
  const { parts, setPart, setQuantity, removePart, clear } = useBuilderStore();
  const [pickerSlot, setPickerSlot] = useState<ComponentType | null>(null);
  const items = buildItems(parts);

  const { data: check, isFetching, isError, error } = useQuery({
    queryKey: ['build-check', items],
    queryFn: () => checkBuild(items),
    enabled: items.length > 0,
    placeholderData: keepPreviousData,
  });

  const slotByProduct = new Map(
    (Object.entries(parts) as [ComponentType, { productId: string }][]).map(([slot, part]) => [part.productId, slot]),
  );
  const issues = items.length > 0 && check
    ? [...check.issues].sort((a, b) => SEVERITY_ORDER.indexOf(a.severity) - SEVERITY_ORDER.indexOf(b.severity))
    : [];

  const slotSeverity = (slot: ComponentType): IssueSeverity | undefined => {
    const productId = parts[slot]?.productId;
    if (!productId) return undefined;
    return SEVERITY_ORDER.find((severity) =>
      issues.some((issue) => issue.severity === severity && issue.productIds.includes(productId)));
  };

  const errorCount = issues.filter((i) => i.severity === 'Error').length;
  const warningCount = issues.filter((i) => i.severity === 'Warning').length;
  const weight = items.length > 0 ? check?.weight : undefined;
  const missingWeight = (weight?.missingWeightProductIds ?? [])
    .map((id) => slotByProduct.get(id))
    .filter((slot): slot is ComponentType => !!slot)
    .map((slot) => CATEGORY_LABELS[slot]);

  return (
    <div className="page-enter max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-4xl font-bold font-orbitron text-white mb-2">
            Drone <span className="text-cyan-400 glow-text">Builder</span>
          </h1>
          <p className="text-slate-400">Pick the parts of your FPV quad. Compatibility is checked as you go.</p>
        </div>
        {items.length > 0 && (
          <button type="button" onClick={clear} id="builder-clear" className="text-sm text-slate-500 hover:text-red-400 cursor-pointer">
            Clear build
          </button>
        )}
      </div>

      <div className="flex flex-col lg:flex-row gap-8">
        <div className="flex-1 space-y-8">
          {SECTIONS.map((section) => (
            <section key={section.title} className="space-y-3">
              <h2 className="text-sm font-semibold text-white uppercase tracking-wider">{section.title}</h2>
              {section.slots.map(({ slot, hint }) => (
                <PartSlot
                  key={slot}
                  slot={slot}
                  hint={hint}
                  part={parts[slot]}
                  severity={slotSeverity(slot)}
                  quantityEditable={QUANTITY_SLOTS.includes(slot)}
                  onChoose={() => setPickerSlot(slot)}
                  onSelectVariant={(productId) => setPart(slot, productId)}
                  onQuantityChange={(quantity) => setQuantity(slot, quantity)}
                  onRemove={() => removePart(slot)}
                />
              ))}
            </section>
          ))}
        </div>

        <aside className="w-full lg:w-96 flex-shrink-0">
          <div className="glass-card p-5 sticky top-24 space-y-5" id="builder-summary">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold text-white">Summary</h2>
              {isFetching && <span className="text-xs text-slate-500">Checking…</span>}
            </div>

            {items.length === 0 ? (
              <p className="text-sm text-slate-500">Choose a part to start your build.</p>
            ) : isError ? (
              <p className="text-sm text-red-400">{getErrorMessage(error, 'Failed to check the build.')}</p>
            ) : check ? (
              <>
                <div
                  id="builder-status"
                  className={`rounded-xl border px-3 py-2 text-sm font-medium ${
                    errorCount > 0 ? SEVERITY_STYLE.Error : warningCount > 0 ? SEVERITY_STYLE.Warning : 'border-emerald-500/30 bg-emerald-500/10 text-emerald-300'
                  }`}
                >
                  {errorCount > 0
                    ? `${errorCount} problem${errorCount === 1 ? '' : 's'} to fix`
                    : warningCount > 0
                      ? `Compatible, ${warningCount} warning${warningCount === 1 ? '' : 's'}`
                      : 'All parts are compatible'}
                </div>

                <dl className="divide-y divide-white/5 text-sm">
                  <div className="flex justify-between py-2">
                    <dt className="text-slate-400">Total price</dt>
                    <dd className="text-cyan-400 font-semibold" id="builder-total">{formatMoney(check.totalPrice)}</dd>
                  </div>
                  <div className="flex justify-between py-2">
                    <dt className="text-slate-400">Dry weight</dt>
                    <dd className="text-white">{check.weight.dryGrams} g</dd>
                  </div>
                  <div className="flex justify-between py-2">
                    <dt className="text-slate-400">Take-off weight</dt>
                    <dd className="text-white" id="builder-auw">{check.weight.allUpGrams != null ? `${check.weight.allUpGrams} g` : '—'}</dd>
                  </div>
                  <div className="flex justify-between py-2">
                    <dt className="text-slate-400">Thrust to weight</dt>
                    <dd className="text-white" id="builder-twr">{check.weight.thrustToWeight != null ? `${check.weight.thrustToWeight} : 1` : '—'}</dd>
                  </div>
                </dl>
                {missingWeight.length > 0 && (
                  <p className="text-xs text-slate-500">No weight data for: {missingWeight.join(', ')}.</p>
                )}

                {issues.length > 0 && (
                  <ul className="space-y-2" id="builder-issues">
                    {issues.map((issue, index) => (
                      <li key={`${issue.code}-${index}`} className={`rounded-xl border px-3 py-2 text-sm ${SEVERITY_STYLE[issue.severity]}`}>
                        <p>{issue.message}</p>
                        {issue.productIds.length > 0 && (
                          <p className="text-xs opacity-70 mt-0.5">
                            {[...new Set(issue.productIds.map((id) => slotByProduct.get(id)).filter((slot) => !!slot))]
                              .map((slot) => CATEGORY_LABELS[slot!])
                              .join(', ')}
                          </p>
                        )}
                      </li>
                    ))}
                  </ul>
                )}
              </>
            ) : null}
          </div>
        </aside>
      </div>

      <PartPicker
        slot={pickerSlot}
        selectedId={pickerSlot ? parts[pickerSlot]?.productId : undefined}
        onPick={(product) => setPart(pickerSlot!, product.id)}
        onClose={() => setPickerSlot(null)}
      />
    </div>
  );
};

export default BuilderPage;
