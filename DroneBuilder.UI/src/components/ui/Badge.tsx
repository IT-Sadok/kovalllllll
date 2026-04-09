import React from 'react';
import { OrderStatusLabel } from '../../types';
import type { OrderStatus } from '../../types';

interface BadgeProps {
  status: OrderStatus;
}

const statusConfig: Record<OrderStatus, { label: string; className: string }> = {
  0: { // New
    label: 'New',
    className: 'bg-blue-500/15 text-blue-400 border-blue-500/30',
  },
  1: { // Paid
    label: 'Paid',
    className: 'bg-emerald-500/15 text-emerald-400 border-emerald-500/30',
  },
  2: { // Sent
    label: 'Sent',
    className: 'bg-violet-500/15 text-violet-400 border-violet-500/30',
  },
  3: { // Completed
    label: 'Completed',
    className: 'bg-cyan-500/15 text-cyan-400 border-cyan-500/30',
  },
  4: { // Cancelled
    label: 'Cancelled',
    className: 'bg-red-500/15 text-red-400 border-red-500/30',
  },
};

const Badge: React.FC<BadgeProps> = ({ status }) => {
  const config = statusConfig[status] ?? {
    label: OrderStatusLabel[status] ?? String(status),
    className: 'bg-slate-500/15 text-slate-400 border-slate-500/30',
  };

  return (
    <span
      className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium border ${config.className}`}
    >
      <span className="w-1.5 h-1.5 rounded-full bg-current mr-1.5 opacity-80" />
      {config.label}
    </span>
  );
};

export default Badge;
