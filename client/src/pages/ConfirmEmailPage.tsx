import React from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import toast from 'react-hot-toast';
import { confirmEmail, resendEmailConfirmation } from '../api/auth';
import { getErrorMessage } from '../api/errors';
import Input from '../components/ui/Input';
import Button from '../components/ui/Button';

const schema = z.object({
  email: z.string().email('Invalid email address'),
});
type FormData = z.infer<typeof schema>;

const ConfirmEmailPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const userId = searchParams.get('userId');
  const token = searchParams.get('token');
  const hasLink = Boolean(userId && token);

  const confirmation = useQuery({
    queryKey: ['confirm-email', userId, token],
    queryFn: () => confirmEmail(userId!, token!),
    enabled: hasLink,
    retry: false,
    staleTime: Infinity,
  });

  const resendMutation = useMutation({
    mutationFn: (data: FormData) => resendEmailConfirmation(data.email),
    onSuccess: () => toast.success('If that account is awaiting confirmation, a new link is on its way.'),
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Could not send the email')),
  });

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const showResendForm = !hasLink || confirmation.isError;

  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center px-4 py-16">
      <div className="w-full max-w-md relative">
        <div className="glass-card p-8 shadow-2xl space-y-6">
          <div className="text-center">
            <h1 className="text-2xl font-bold text-white">Email confirmation</h1>
          </div>

          {hasLink && confirmation.isLoading && (
            <div className="flex justify-center">
              <div
                className="w-8 h-8 rounded-full border-2 border-slate-700 border-t-cyan-400 animate-spin"
                role="status"
                aria-label="Confirming"
              />
            </div>
          )}

          {confirmation.isSuccess && (
            <div className="text-center space-y-4">
              <p className="text-slate-300">Your email is confirmed. You can sign in now.</p>
              <Link to="/login" id="confirm-email-login-link">
                <Button fullWidth size="lg">Sign In</Button>
              </Link>
            </div>
          )}

          {confirmation.isError && (
            <p className="text-center text-red-400 text-sm">
              {getErrorMessage(confirmation.error, 'This confirmation link is invalid or has expired.')}
            </p>
          )}

          {showResendForm && (
            <form onSubmit={handleSubmit((data) => resendMutation.mutate(data))} className="space-y-4">
              <p className="text-slate-400 text-sm">
                Enter your email and we will send you a new confirmation link.
              </p>
              <Input
                label="Email"
                type="email"
                id="confirm-email-address"
                placeholder="pilot@example.com"
                {...register('email')}
                error={errors.email?.message}
              />
              <Button type="submit" fullWidth size="lg" loading={resendMutation.isPending} id="confirm-email-resend">
                Send confirmation link
              </Button>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};

export default ConfirmEmailPage;
