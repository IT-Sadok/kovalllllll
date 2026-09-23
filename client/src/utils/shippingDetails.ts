import type { ShippingDetails } from '../types';

type ShippingField = keyof ShippingDetails;
type ShippingDetailsRecord = Record<string, unknown>;

export const parseShippingDetails = (value: string): ShippingDetailsRecord | null => {
  try {
    const parsed: unknown = JSON.parse(value);
    return typeof parsed === 'object' && parsed !== null
      ? parsed as ShippingDetailsRecord
      : null;
  } catch {
    return null;
  }
};

export const getShippingValue = (
  shipping: ShippingDetailsRecord | null,
  key: ShippingField,
): string => {
  if (!shipping) {
    return 'N/A';
  }

  const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
  const value = shipping[key] ?? shipping[pascalKey];

  return typeof value === 'string' && value.trim() ? value : 'N/A';
};
