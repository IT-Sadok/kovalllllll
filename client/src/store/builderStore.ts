import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { BuildItem, ComponentType } from '../types';

export const BUILD_SLOTS: ComponentType[] = [
  'Frame', 'Motor', 'Propeller', 'Stack', 'FlightController', 'Esc',
  'Battery', 'VideoTransmitter', 'Camera', 'Receiver', 'Antenna',
];

export const DEFAULT_QUANTITY: Partial<Record<ComponentType, number>> = { Motor: 4 };

interface BuilderState {
  parts: Partial<Record<ComponentType, BuildItem>>;
  setPart: (slot: ComponentType, productId: string) => void;
  setQuantity: (slot: ComponentType, quantity: number) => void;
  removePart: (slot: ComponentType) => void;
  clear: () => void;
}

export const useBuilderStore = create<BuilderState>()(
  persist(
    (set) => ({
      parts: {},
      setPart: (slot, productId) =>
        set((state) => ({
          parts: {
            ...state.parts,
            [slot]: { productId, quantity: state.parts[slot]?.quantity ?? DEFAULT_QUANTITY[slot] ?? 1 },
          },
        })),
      setQuantity: (slot, quantity) =>
        set((state) => {
          const part = state.parts[slot];
          return part ? { parts: { ...state.parts, [slot]: { ...part, quantity } } } : state;
        }),
      removePart: (slot) =>
        set((state) => {
          const parts = { ...state.parts };
          delete parts[slot];
          return { parts };
        }),
      clear: () => set({ parts: {} }),
    }),
    { name: 'drone-builder' },
  ),
);

export const buildItems = (parts: Partial<Record<ComponentType, BuildItem>>): BuildItem[] =>
  BUILD_SLOTS.flatMap((slot) => (parts[slot] ? [parts[slot]] : []));
