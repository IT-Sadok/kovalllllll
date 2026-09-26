import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { deleteBuild, getBuilds } from '../api/builds';
import { getErrorMessage } from '../api/errors';
import Button from '../components/ui/Button';
import EmptyState from '../components/ui/EmptyState';
import Modal from '../components/ui/Modal';
import Skeleton from '../components/ui/Skeleton';
import { buildItems, useBuilderStore } from '../store/builderStore';
import type { SavedBuild } from '../types';
import { CATEGORY_LABELS } from '../utils/componentSpecs';
import { formatMoney } from '../utils/money';

const formatDate = (value: string) =>
  new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(value));

const MyBuildsPage: React.FC = () => {
  const navigate = useNavigate();
  const { parts, saved, loadBuild, clear } = useBuilderStore();
  const [deleteTarget, setDeleteTarget] = useState<SavedBuild | null>(null);

  const { data: builds, isLoading, isError } = useQuery({ queryKey: ['builds'], queryFn: getBuilds });

  const deleteMutation = useMutation({
    mutationFn: (build: SavedBuild) => deleteBuild(build.id),
    onSuccess: (_, build) => {
      if (saved?.id === build.id) clear();
      toast.success(`Deleted "${build.name}"`);
      setDeleteTarget(null);
    },
    onError: (error: unknown) => toast.error(getErrorMessage(error, 'Failed to delete the build')),
  });

  const open = (build: SavedBuild) => {
    const hasUnsavedParts = buildItems(parts).length > 0 && saved?.id !== build.id;
    if (hasUnsavedParts && !window.confirm('Replace the parts currently in the builder?')) return;
    loadBuild(build);
    navigate('/builder');
  };

  return (
    <div className="page-enter max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
        <h1 className="text-4xl font-bold font-orbitron text-white">
          My <span className="text-cyan-400 glow-text">Builds</span>
        </h1>
        <Link to="/builder" className="text-sm text-cyan-400 hover:underline" id="my-builds-new">Open the builder →</Link>
      </div>

      {isError && <div className="glass-card p-6 text-center text-red-400">Failed to load your builds.</div>}

      {isLoading ? (
        <div className="space-y-4">
          <Skeleton className="h-32 w-full" count={3} />
        </div>
      ) : builds?.length === 0 ? (
        <EmptyState
          title="No saved builds yet"
          description="Put a quad together in the builder and save it to find it here."
          action={<Button onClick={() => navigate('/builder')} variant="outline" id="my-builds-empty-cta">Start building</Button>}
        />
      ) : (
        <div className="space-y-4">
          {builds?.map((build) => {
            const unavailable = build.items.filter((i) => !i.isAvailable);
            const cover = build.items.find((i) => i.category === 'Frame')?.imageUrl ?? build.items.find((i) => i.imageUrl)?.imageUrl;
            return (
              <div key={build.id} id={`build-${build.id}`} className="glass-card p-5 flex flex-col sm:flex-row gap-5">
                <div className="w-full sm:w-32 h-32 flex-shrink-0 rounded-xl overflow-hidden bg-slate-800">
                  {cover && <img src={cover} alt="" className="w-full h-full object-cover" />}
                </div>
                <div className="flex-1 min-w-0 space-y-2">
                  <div className="flex flex-wrap items-baseline justify-between gap-2">
                    <h2 className="text-lg font-semibold text-white">{build.name}</h2>
                    <span className="text-lg font-bold text-cyan-400 font-orbitron">{formatMoney(build.totalPrice)}</span>
                  </div>
                  <p className="text-xs text-slate-500">
                    {build.items.length} part{build.items.length === 1 ? '' : 's'} · updated {formatDate(build.updatedAt)}
                  </p>
                  <ul className="text-xs text-slate-400 space-y-0.5">
                    {build.items.map((item) => (
                      <li key={item.productId} className={`truncate ${item.isAvailable ? '' : 'line-through text-slate-600'}`}>
                        <span className="text-slate-500">{CATEGORY_LABELS[item.category]}:</span>{' '}
                        {item.quantity > 1 && `${item.quantity}× `}{item.productName}
                      </li>
                    ))}
                  </ul>
                  {unavailable.length > 0 && (
                    <p className="text-xs text-amber-400">
                      {unavailable.length} part{unavailable.length === 1 ? ' is' : 's are'} no longer sold and left out of the total.
                    </p>
                  )}
                  <div className="flex gap-2 pt-1">
                    <Button size="sm" onClick={() => open(build)} id={`build-open-${build.id}`}>Open in builder</Button>
                    <Button size="sm" variant="danger" onClick={() => setDeleteTarget(build)} id={`build-delete-${build.id}`}>
                      Delete
                    </Button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      <Modal isOpen={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="Delete build" size="sm">
        <p className="text-sm text-slate-300 mb-5">Delete "{deleteTarget?.name}"? This cannot be undone.</p>
        <div className="flex gap-2">
          <Button
            variant="danger"
            size="sm"
            loading={deleteMutation.isPending}
            onClick={() => deleteTarget && deleteMutation.mutate(deleteTarget)}
            id="build-delete-confirm"
          >
            Delete
          </Button>
          <Button variant="ghost" size="sm" onClick={() => setDeleteTarget(null)}>Cancel</Button>
        </div>
      </Modal>
    </div>
  );
};

export default MyBuildsPage;
