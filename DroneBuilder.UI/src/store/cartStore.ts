import { create } from 'zustand';

interface CartStore {
  itemCount: number;
  setItemCount: (count: number) => void;
  increment: () => void;
  decrement: () => void;
}

export const useCartStore = create<CartStore>((set) => ({
  itemCount: 0,
  setItemCount: (count) => set({ itemCount: count }),
  increment: () => set((s) => ({ itemCount: s.itemCount + 1 })),
  decrement: () => set((s) => ({ itemCount: Math.max(0, s.itemCount - 1) })),
}));
