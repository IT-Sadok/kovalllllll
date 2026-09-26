import React from 'react';
import type { UseFormRegister } from 'react-hook-form';
import {
  BATTERY_CONNECTORS,
  MOUNT_PATTERNS,
  RADIO_PROTOCOLS,
  RF_CONNECTORS,
  VIDEO_SYSTEMS,
  type ProductCategory,
} from '../../types';
import { CATEGORY_SPEC_FILTERS, type CatalogFilterForm, type SpecFilterForm } from '../../utils/catalogFilters';
import { optionLabel } from '../../utils/componentSpecs';

const CELL_OPTIONS = [1, 2, 3, 4, 5, 6, 8];
const PROP_SIZES = [2, 3, 4, 5, 6, 7, 8, 10];

const fieldClass =
  'w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-cyan-500/20 focus:border-cyan-500/50 transition-all';

const Field: React.FC<{ label: string; children: React.ReactNode }> = ({ label, children }) => (
  <div className="space-y-1.5">
    <label className="text-sm font-medium text-slate-300">{label}</label>
    {children}
  </div>
);

interface SpecFiltersProps {
  category: ProductCategory | '';
  register: UseFormRegister<CatalogFilterForm>;
}

const SpecFilters: React.FC<SpecFiltersProps> = ({ category, register }) => {
  const filters = category ? CATEGORY_SPEC_FILTERS[category] ?? [] : [];

  const select = (name: keyof SpecFilterForm, options: readonly (string | number)[], format: (o: string | number) => string) => (
    <select id={`filter-${name}`} {...register(name)} className={`${fieldClass} cursor-pointer`}>
      <option value="">Any</option>
      {options.map((option) => (
        <option key={option} value={option}>{format(option)}</option>
      ))}
    </select>
  );

  const range = (min: keyof SpecFilterForm, max: keyof SpecFilterForm) => (
    <div className="flex gap-2">
      <input type="number" placeholder="Min" id={`filter-${min}`} {...register(min)} className={fieldClass} />
      <input type="number" placeholder="Max" id={`filter-${max}`} {...register(max)} className={fieldClass} />
    </div>
  );

  return (
    <>
      {filters.map((filter) => {
        switch (filter) {
          case 'cells':
            return <Field key={filter} label="Battery cells">{select('cells', CELL_OPTIONS, (o) => `${o}S`)}</Field>;
          case 'mountPattern':
            return <Field key={filter} label="Mounting">{select('mountPattern', MOUNT_PATTERNS, (o) => optionLabel(String(o)))}</Field>;
          case 'videoSystem':
            return <Field key={filter} label="Video system">{select('videoSystem', VIDEO_SYSTEMS, (o) => optionLabel(String(o)))}</Field>;
          case 'kv':
            return <Field key={filter} label="KV">{range('kvMin', 'kvMax')}</Field>;
          case 'propSize':
            return <Field key={filter} label="Prop size">{select('propSizeInch', PROP_SIZES, (o) => `${o}"`)}</Field>;
          case 'capacity':
            return <Field key={filter} label="Capacity (mAh)">{range('capacityMin', 'capacityMax')}</Field>;
          case 'batteryConnector':
            return <Field key={filter} label="Battery connector">{select('batteryConnector', BATTERY_CONNECTORS, (o) => optionLabel(String(o)))}</Field>;
          case 'protocol':
            return <Field key={filter} label="Protocol">{select('protocol', RADIO_PROTOCOLS, (o) => optionLabel(String(o)))}</Field>;
          case 'rfConnector':
            return <Field key={filter} label="Antenna connector">{select('rfConnector', RF_CONNECTORS, (o) => optionLabel(String(o)))}</Field>;
        }
      })}
    </>
  );
};

export default SpecFilters;
