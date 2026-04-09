import React from 'react';

type Variant = 'primary' | 'secondary' | 'danger' | 'ghost' | 'outline';
type Size = 'sm' | 'md' | 'lg';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
  icon?: React.ReactNode;
  fullWidth?: boolean;
}

const variantClasses: Record<Variant, string> = {
  primary: 'btn-primary rounded-xl text-black font-semibold cursor-pointer',
  secondary:
    'bg-violet-600 hover:bg-violet-500 text-white rounded-xl font-semibold transition-all duration-200 hover:shadow-[0_0_20px_rgba(124,58,237,0.5)] cursor-pointer',
  danger:
    'bg-red-600/20 border border-red-500/40 text-red-400 hover:bg-red-600/30 hover:border-red-500 rounded-xl font-semibold transition-all duration-200 cursor-pointer',
  ghost:
    'text-slate-300 hover:text-white hover:bg-white/5 rounded-xl font-medium transition-all duration-200 cursor-pointer',
  outline:
    'border border-[rgba(0,212,255,0.3)] text-cyan-400 hover:border-cyan-400 hover:bg-[rgba(0,212,255,0.08)] rounded-xl font-semibold transition-all duration-200 cursor-pointer',
};

const sizeClasses: Record<Size, string> = {
  sm: 'px-3 py-1.5 text-sm',
  md: 'px-5 py-2.5 text-sm',
  lg: 'px-7 py-3 text-base',
};

const Button: React.FC<ButtonProps> = ({
  variant = 'primary',
  size = 'md',
  loading = false,
  icon,
  fullWidth = false,
  children,
  disabled,
  className = '',
  ...props
}) => {
  const isDisabled = disabled || loading;

  return (
    <button
      {...props}
      disabled={isDisabled}
      className={`
        inline-flex items-center justify-center gap-2 select-none
        ${variantClasses[variant]}
        ${sizeClasses[size]}
        ${fullWidth ? 'w-full' : ''}
        ${isDisabled ? 'opacity-50 cursor-not-allowed pointer-events-none' : ''}
        ${className}
      `}
    >
      {loading ? (
        <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
        </svg>
      ) : icon ? (
        <span className="flex-shrink-0">{icon}</span>
      ) : null}
      {children}
    </button>
  );
};

export default Button;
