import React from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getErrorMessage } from '../../api/errors';
import { getImportRuns, startRaceDayQuadsImport } from '../../api/admin';
import Button from '../../components/ui/Button';
import EmptyState from '../../components/ui/EmptyState';
import { TableRowSkeleton } from '../../components/ui/Skeleton';
import type { ImportRunStatus } from '../../types';

const STATUS_CLASSES: Record<ImportRunStatus, string> = {
  Queued: 'bg-slate-500/15 text-slate-300 border-slate-500/30',
  Running: 'bg-cyan-500/15 text-cyan-300 border-cyan-500/30',
  Succeeded: 'bg-emerald-500/15 text-emerald-300 border-emerald-500/30',
  Failed: 'bg-red-500/15 text-red-300 border-red-500/30',
};

const formatTime = (value?: string | null) => (value ? new Date(value).toLocaleString() : '—');

const AdminImportPage: React.FC = () => {
  const queryClient = useQueryClient();

  const { data: runs = [], isLoading } = useQuery({
    queryKey: ['import-runs'],
    queryFn: getImportRuns,
    refetchInterval: (query) =>
      query.state.data?.some((r) => r.status === 'Queued' || r.status === 'Running') ? 3000 : false,
  });

  const active = runs.some((r) => r.status === 'Queued' || r.status === 'Running');

  const startMutation = useMutation({
    mutationFn: startRaceDayQuadsImport,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['import-runs'] });
      toast.success('Import started');
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Failed to start import')),
  });

  return (
    <div className="page-enter max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold text-white font-orbitron">
            Catalog <span className="text-cyan-400">Import</span>
          </h1>
          <p className="text-slate-400 text-sm mt-1">
            Pulls drone parts from{' '}
            <a href="https://www.racedayquads.com" target="_blank" rel="noreferrer" className="text-cyan-400 hover:underline">
              RaceDayQuads
            </a>
            . Prices come from the source; stock starts at 0 and is managed in the warehouse.
          </p>
        </div>
        <Button
          id="import-start-btn"
          loading={startMutation.isPending}
          disabled={active}
          onClick={() => startMutation.mutate()}
        >
          {active ? 'Import running…' : 'Import from RaceDayQuads'}
        </Button>
      </div>

      <div className="glass-card overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="border-b border-white/5">
            <tr>
              {['Status', 'Started', 'Finished', 'Added', 'Updated', 'Skipped', 'Needs review', 'Failed'].map((h) => (
                <th key={h} className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase tracking-wider">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
            {isLoading
              ? Array.from({ length: 3 }).map((_, i) => <TableRowSkeleton key={i} cols={8} />)
              : runs.map((run) => (
                  <tr key={run.id} id={`import-run-${run.id}`}>
                    <td className="px-4 py-3">
                      <span className={`text-xs px-2 py-1 rounded-full border ${STATUS_CLASSES[run.status]}`}>{run.status}</span>
                      {run.error && <p className="text-xs text-red-400 mt-1 max-w-xs">{run.error}</p>}
                    </td>
                    <td className="px-4 py-3 text-slate-300">{formatTime(run.startedAt ?? run.createdAt)}</td>
                    <td className="px-4 py-3 text-slate-300">{formatTime(run.finishedAt)}</td>
                    <td className="px-4 py-3 text-emerald-400 font-medium">{run.added}</td>
                    <td className="px-4 py-3 text-slate-200">{run.updated}</td>
                    <td className="px-4 py-3 text-slate-400">{run.skipped}</td>
                    <td className="px-4 py-3 text-amber-400">{run.needsReview}</td>
                    <td className="px-4 py-3 text-red-400">{run.failed}</td>
                  </tr>
                ))}
          </tbody>
        </table>
        {!isLoading && runs.length === 0 && (
          <EmptyState title="No imports yet" description="Start the first import to fill the catalog" />
        )}
      </div>

      <p className="text-slate-500 text-xs mt-4">
        Products marked “Needs review” were imported without a component spec — open them from{' '}
        <Link to="/admin/products" className="text-cyan-400 hover:underline">Products</Link> and fill the spec by hand.
      </p>
    </div>
  );
};

export default AdminImportPage;
