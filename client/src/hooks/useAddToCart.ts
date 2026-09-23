import { useMutation } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { addToCart, getCartItems } from '../api/cart';
import { getErrorMessage } from '../api/errors';
import { useCartStore } from '../store/cartStore';

interface AddToCartVariables {
  productId: string;
  productName: string;
  quantity: number;
}

export const useAddToCart = () => {
  const setItemCount = useCartStore((state) => state.setItemCount);

  return useMutation({
    mutationFn: ({ productId, quantity }: AddToCartVariables) => addToCart(productId, quantity),
    onSuccess: async (_, { productName, quantity }) => {
      toast.success(`Added ${quantity}x ${productName} to cart!`);
      const items = await getCartItems();
      setItemCount(items.length);
    },
    onError: (error: unknown) => {
      toast.error(getErrorMessage(error, 'Failed to add to cart'));
    },
  });
};
