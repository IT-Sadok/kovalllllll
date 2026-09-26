import type { ProductCategory, ProductFilters } from '../types';

export type SpecFilter = 'cells' | 'mountPattern' | 'videoSystem' | 'kv' | 'propSize' | 'capacity' | 'batteryConnector' | 'protocol' | 'rfConnector';

export interface SpecFilterForm {
  cells: string;
  mountPattern: string;
  videoSystem: string;
  kvMin: string;
  kvMax: string;
  propSizeInch: string;
  capacityMin: string;
  capacityMax: string;
  batteryConnector: string;
  protocol: string;
  rfConnector: string;
}

export interface CatalogFilterForm extends SpecFilterForm {
  name: string;
  minPrice: string;
  maxPrice: string;
  category: ProductCategory | '';
  manufacturer: string;
  inStock: boolean;
}

export const EMPTY_FILTERS: CatalogFilterForm = {
  name: '', minPrice: '', maxPrice: '', category: '', manufacturer: '', inStock: false,
  cells: '', mountPattern: '', videoSystem: '', kvMin: '', kvMax: '', propSizeInch: '',
  capacityMin: '', capacityMax: '', batteryConnector: '', protocol: '', rfConnector: '',
};

export const CATEGORY_SPEC_FILTERS: Partial<Record<ProductCategory, SpecFilter[]>> = {
  Frame: ['propSize', 'mountPattern'],
  Motor: ['kv', 'cells', 'mountPattern'],
  Propeller: ['propSize'],
  FlightController: ['cells', 'mountPattern'],
  Esc: ['cells', 'mountPattern', 'batteryConnector'],
  Stack: ['cells', 'mountPattern', 'batteryConnector'],
  Battery: ['cells', 'capacity', 'batteryConnector'],
  VideoTransmitter: ['videoSystem', 'rfConnector', 'mountPattern'],
  Camera: ['videoSystem'],
  Receiver: ['protocol'],
  Antenna: ['rfConnector'],
  Radio: ['protocol'],
};

const toNumber = (value: string) => (value === '' ? undefined : Number(value));
const toValue = <T extends string>(value: string) => (value === '' ? undefined : (value as T));

export const toSpecFilters = (category: ProductCategory | '', form: SpecFilterForm): Partial<ProductFilters> => {
  const active = new Set(category ? CATEGORY_SPEC_FILTERS[category] ?? [] : []);
  return {
    cells: active.has('cells') ? toNumber(form.cells) : undefined,
    mountPattern: active.has('mountPattern') ? toValue(form.mountPattern) : undefined,
    videoSystem: active.has('videoSystem') ? toValue(form.videoSystem) : undefined,
    kvMin: active.has('kv') ? toNumber(form.kvMin) : undefined,
    kvMax: active.has('kv') ? toNumber(form.kvMax) : undefined,
    propSizeInch: active.has('propSize') ? toNumber(form.propSizeInch) : undefined,
    capacityMin: active.has('capacity') ? toNumber(form.capacityMin) : undefined,
    capacityMax: active.has('capacity') ? toNumber(form.capacityMax) : undefined,
    batteryConnector: active.has('batteryConnector') ? toValue(form.batteryConnector) : undefined,
    protocol: active.has('protocol') ? toValue(form.protocol) : undefined,
    rfConnector: active.has('rfConnector') ? toValue(form.rfConnector) : undefined,
  };
};
