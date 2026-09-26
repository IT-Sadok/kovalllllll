import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { BuildItem, ComponentType, SavedBuild } from '../types';

export const BUILD_SLOTS: ComponentType[] = [
  'Frame', 'Motor', 'Propeller', 'Stack', 'FlightController', 'Esc',
  'Battery', 'VideoTransmitter', 'Camera', 'Receiver', 'Antenna',
];

export const DEFAULT_QUANTITY: Partial<Record<ComponentType, number>> = { Motor: 4 };

interface BuilderState {
  parts: Partial<Record<ComponentType, BuildItem>>;
  saved: { id: string; name: string } | null;
  setPart: (slot: ComponentType, productId: string) => void;
  setQuantity: (slot: ComponentType, quantity: number) => void;
  removePart: (slot: ComponentType) => void;
  clear: () => void;
  setSaved: (build: SavedBuild) => void;
  loadBuild: (build: SavedBuild) => void;
}

export const useBuilderStore = create<BuilderState>()(
  persist(
    (set) => ({
      parts: {},
      saved: null,
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
      clear: () => set({ parts: {}, saved: null }),
      setSaved: (build) => set({ saved: { id: build.id, name: build.name } }),
      loadBuild: (build) =>
        set({
          parts: Object.fromEntries(
            build.items
              .filter((item) => (BUILD_SLOTS as string[]).includes(item.category))
              .map((item) => [item.category, { productId: item.productId, quantity: item.quantity }]),
          ),
          saved: { id: build.id, name: build.name },
        }),
    }),
    { name: 'drone-builder' },
  ),
);

export const buildItems = (parts: Partial<Record<ComponentType, BuildItem>>): BuildItem[] =>
  BUILD_SLOTS.flatMap((slot) => (parts[slot] ? [parts[slot]] : []));
