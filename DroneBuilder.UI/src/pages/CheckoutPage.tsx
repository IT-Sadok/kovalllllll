import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import toast from 'react-hot-toast';
import { createOrder } from '../api/orders';
import { clearCart } from '../api/cart';
import { useCartStore } from '../store/cartStore';
import { useQueryClient } from '@tanstack/react-query';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';

const schema = z.object({
  fullName: z.string().min(2, 'Full name is required'),
  addressLine1: z.string().min(5, 'Address is required'),
  addressLine2: z.string().default(''), // required string in ShippingDetails (can be empty)
  city: z.string().min(2, 'City is required'),
  state: z.string().min(2, 'State is required'),
  postalCode: z.string().min(3, 'Postal code is required'),
  country: z.string().min(2, 'Country is required'),
  phoneNumber: z.string().min(7, 'Phone number is required'),
});

type FormData = z.infer<typeof schema>;

const CheckoutPage: React.FC = () => {
  const navigate = useNavigate();
  const { setItemCount } = useCartStore();
  const queryClient = useQueryClient();

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(schema) as any,
  });

  const onSubmit = async (data: FormData) => {
    try {
      // ShippingDetails has all required fields including addressLine2 as string
      const payload = { ...data, addressLine2: data.addressLine2 ?? '' };
      await createOrder(payload);
      await clearCart().catch(() => {});
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      setItemCount(0);
      toast.success('Order placed successfully! 🚁');
      navigate('/orders');
    } catch (err: any) {
      toast.error(err?.response?.data?.message || 'Failed to place order');
    }
  };

  return (
    <div className="page-enter max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <h1 className="text-3xl font-bold text-white font-orbitron mb-2">
        <span className="text-cyan-400">Checkout</span>
      </h1>
      <p className="text-slate-400 mb-8">Enter your delivery details below</p>

      <div className="glass-card p-6 sm:p-8">
        <h2 className="text-sm font-semibold text-white mb-6 pb-4 border-b border-white/5 flex items-center gap-2">
          <svg className="w-4 h-4 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
          Shipping Information
        </h2>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input
            label="Full Name"
            id="checkout-fullName"
            placeholder="John Doe"
            {...register('fullName')}
            error={errors.fullName?.message}
          />

          <Input
            label="Address Line 1"
            id="checkout-address1"
            placeholder="123 Drone Street"
            {...register('addressLine1')}
            error={errors.addressLine1?.message}
          />

          <Input
            label="Address Line 2 (optional)"
            id="checkout-address2"
            placeholder="Apt, Suite, Unit..."
            {...register('addressLine2')}
          />

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="City"
              id="checkout-city"
              placeholder="Kyiv"
              {...register('city')}
              error={errors.city?.message}
            />
            <Input
              label="State / Region"
              id="checkout-state"
              placeholder="Kyiv Oblast"
              {...register('state')}
              error={errors.state?.message}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Input
              label="Postal Code"
              id="checkout-postalCode"
              placeholder="01001"
              {...register('postalCode')}
              error={errors.postalCode?.message}
            />
            <Input
              label="Country"
              id="checkout-country"
              placeholder="Ukraine"
              {...register('country')}
              error={errors.country?.message}
            />
          </div>

          <Input
            label="Phone Number"
            id="checkout-phone"
            type="tel"
            placeholder="+380 50 000 0000"
            {...register('phoneNumber')}
            error={errors.phoneNumber?.message}
            icon={
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
              </svg>
            }
          />

          <div className="pt-4">
            <Button
              type="submit"
              fullWidth
              size="lg"
              loading={isSubmitting}
              id="checkout-submit"
              icon={
                !isSubmitting && (
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                )
              }
            >
              Place Order
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CheckoutPage;
