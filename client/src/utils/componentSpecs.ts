import {
  BATTERY_CONNECTORS,
  COMPONENT_TYPES,
  MOUNT_PATTERNS,
  RADIO_PROTOCOLS,
  RF_CONNECTORS,
  VIDEO_SYSTEMS,
  type ComponentSpec,
  type ComponentType,
  type ProductCategory,
} from '../types';

export type SpecField =
  | { key: string; label: string; kind: 'number'; unit?: string; step?: number; optional?: boolean }
  | { key: string; label: string; kind: 'text'; placeholder?: string }
  | { key: string; label: string; kind: 'enum'; options: readonly string[]; optional?: boolean }
  | { key: string; label: string; kind: 'enumList'; options: readonly string[] };

export const CATEGORY_LABELS: Record<ProductCategory, string> = {
  Frame: 'Frame',
  Motor: 'Motor',
  Propeller: 'Propeller',
  FlightController: 'Flight controller',
  Esc: 'ESC',
  Stack: 'Stack (FC + ESC)',
  Battery: 'Battery',
  VideoTransmitter: 'Video transmitter',
  Camera: 'Camera',
  Receiver: 'Receiver',
  Antenna: 'Antenna',
  Goggles: 'Goggles',
  Radio: 'Radio',
  Charger: 'Charger',
  Tool: 'Tool',
  ReadyToFly: 'Ready to fly',
  Accessory: 'Accessory',
};

export const toComponentType = (category: ProductCategory): ComponentType | null =>
  (COMPONENT_TYPES as readonly string[]).includes(category) ? (category as ComponentType) : null;

const OPTION_LABELS: Record<string, string> = {
  M9x9: '9×9 mm',
  M12x12: '12×12 mm',
  M16x16: '16×16 mm',
  M19x19: '19×19 mm',
  M20x20: '20×20 mm',
  M25_5x25_5: '25.5×25.5 mm',
  M30_5x30_5: '30.5×30.5 mm',
  DjiO3: 'DJI O3',
  DjiO4: 'DJI O4',
  HdZero: 'HDZero',
  Xt30: 'XT30',
  Xt60: 'XT60',
  Xt90: 'XT90',
  Xt150: 'XT150',
  Ec5: 'EC5',
  Bt20: 'BT2.0',
  Ph20: 'PH2.0',
  Ufl: 'U.FL',
  Mmcx: 'MMCX',
  Sma: 'SMA',
  RpSma: 'RP-SMA',
  Ipex4: 'IPEX 4 (MHF4)',
  Mcx: 'MCX',
  ExpressLrs: 'ExpressLRS',
  FrSky: 'FrSky',
};

export const optionLabel = (option: string) => OPTION_LABELS[option] ?? option;

const mount = (key: string, label: string, optional = false): SpecField =>
  ({ key, label, kind: 'enum', options: MOUNT_PATTERNS, optional });
const cells = (key: string, label: string): SpecField => ({ key, label, kind: 'number', unit: 'S' });

const escFields: SpecField[] = [
  mount('mountPattern', 'Mounting'),
  cells('minCells', 'Min cells'),
  cells('maxCells', 'Max cells'),
  { key: 'continuousCurrentA', label: 'Continuous current', kind: 'number', unit: 'A', step: 0.1, optional: true },
  { key: 'batteryConnector', label: 'Battery connector', kind: 'enum', options: BATTERY_CONNECTORS, optional: true },
];

export const SPEC_FIELDS: Record<ComponentType, SpecField[]> = {
  Frame: [
    { key: 'maxPropSizeInch', label: 'Max prop size', kind: 'number', unit: '"', step: 0.1 },
    { key: 'fcMountPatterns', label: 'FC/ESC mounting', kind: 'enumList', options: MOUNT_PATTERNS },
    { key: 'motorMountPatterns', label: 'Motor mounting', kind: 'enumList', options: MOUNT_PATTERNS },
    { key: 'cameraWidthMm', label: 'Camera width', kind: 'number', unit: 'mm', optional: true },
  ],
  Motor: [
    { key: 'statorSize', label: 'Stator size', kind: 'text', placeholder: '2207' },
    { key: 'kv', label: 'KV', kind: 'number' },
    mount('mountPattern', 'Mounting'),
    cells('minCells', 'Min cells'),
    cells('maxCells', 'Max cells'),
    { key: 'maxCurrentA', label: 'Max current', kind: 'number', unit: 'A', step: 0.1, optional: true },
    { key: 'shaftMm', label: 'Shaft', kind: 'number', unit: 'mm', step: 0.1, optional: true },
    { key: 'maxThrustGrams', label: 'Max thrust', kind: 'number', unit: 'g', optional: true },
  ],
  Propeller: [
    { key: 'diameterInch', label: 'Diameter', kind: 'number', unit: '"', step: 0.1 },
    { key: 'pitchInch', label: 'Pitch', kind: 'number', unit: '"', step: 0.1, optional: true },
    { key: 'bladeCount', label: 'Blades', kind: 'number', optional: true },
    { key: 'hubMm', label: 'Hub', kind: 'number', unit: 'mm', step: 0.1, optional: true },
  ],
  FlightController: [
    mount('mountPattern', 'Mounting'),
    cells('minCells', 'Min cells'),
    cells('maxCells', 'Max cells'),
  ],
  Esc: escFields,
  Stack: escFields,
  Battery: [
    cells('cells', 'Cells'),
    { key: 'capacityMah', label: 'Capacity', kind: 'number', unit: 'mAh' },
    { key: 'cRating', label: 'C rating', kind: 'number', unit: 'C', optional: true },
    { key: 'connector', label: 'Connector', kind: 'enum', options: BATTERY_CONNECTORS },
  ],
  VideoTransmitter: [
    { key: 'videoSystem', label: 'Video system', kind: 'enum', options: VIDEO_SYSTEMS },
    { key: 'antennaConnector', label: 'Antenna connector', kind: 'enum', options: RF_CONNECTORS, optional: true },
    mount('mountPattern', 'Mounting', true),
  ],
  Camera: [
    { key: 'videoSystem', label: 'Video system', kind: 'enum', options: VIDEO_SYSTEMS },
    { key: 'widthMm', label: 'Width', kind: 'number', unit: 'mm', optional: true },
  ],
  Receiver: [
    { key: 'protocol', label: 'Protocol', kind: 'enum', options: RADIO_PROTOCOLS },
  ],
  Antenna: [
    { key: 'connector', label: 'Connector', kind: 'enum', options: RF_CONNECTORS },
  ],
};

export const emptySpec = (type: ComponentType): ComponentSpec => {
  const spec: Record<string, unknown> = { type };
  for (const field of SPEC_FIELDS[type]) {
    spec[field.key] =
      field.kind === 'number' ? (field.optional ? null : 0)
        : field.kind === 'text' ? ''
          : field.kind === 'enum' ? (field.optional ? null : field.options[0])
            : [];
  }
  return spec as ComponentSpec;
};

export const formatSpecValue = (field: SpecField, value: unknown): string => {
  if (value === null || value === undefined) return '—';
  if (field.kind === 'enumList') return (value as string[]).map(optionLabel).join(', ') || '—';
  if (field.kind === 'enum') return optionLabel(value as string);
  if (field.kind === 'number' && field.unit) {
    return ['"', 'S', 'C'].includes(field.unit) ? `${value}${field.unit}` : `${value} ${field.unit}`;
  }
  return String(value);
};

export const specSummary = (spec: ComponentSpec): string => {
  const values = spec as Record<string, unknown>;
  return SPEC_FIELDS[spec.type]
    .filter((field) => field.key !== 'maxCells')
    .map((field) => {
      const value = values[field.key];
      if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) return null;
      if (field.key === 'minCells') return value === values.maxCells ? `${value}S` : `${value}–${values.maxCells}S`;
      if (field.kind === 'enumList' || (field.kind === 'number' && !field.unit)) return `${field.label} ${formatSpecValue(field, value)}`;
      return formatSpecValue(field, value);
    })
    .filter((part) => part !== null)
    .join(' · ');
};
