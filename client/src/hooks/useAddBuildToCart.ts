import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { addItemsToCart, getCartItems } from '../api/cart';
import { getErrorMessage } from '../api/errors';
import { useAuthStore } from '../store/authStore';
import { useCartStore } from '../store/cartStore';
import type { BuildItem } from '../types';

export const useAddBuildToCart = () => {
  const user = useAuthStore((state) => state.user);
  const setItemCount = useCartStore((state) => state.setItemCount);
  const navigate = useNavigate();

  const mutation = useMutation({
    mutationFn: (items: BuildItem[]) => addItemsToCart(items),
    onSuccess: async (_, items) => {
      toast.success(`Added ${items.length} part${items.length === 1 ? '' : 's'} to cart`);
      const cartItems = await getCartItems();
      setItemCount(cartItems.length);
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Failed to add the build to cart'), { duration: 6000 }),
  });

  const addBuildToCart = (items: BuildItem[]) => {
    if (!user) {
      toast.error('Please sign in to add items to cart');
      navigate('/login');
      return;
    }
    mutation.mutate(items);
  };

  return { addBuildToCart, isPending: mutation.isPending };
};
