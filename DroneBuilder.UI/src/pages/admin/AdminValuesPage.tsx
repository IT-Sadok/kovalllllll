import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import toast from 'react-hot-toast';
import { getValues, createValue, updateValue, deleteValue } from '../../api/admin';
import type { Value } from '../../types';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import Modal from '../../components/ui/Modal';
import { TableRowSkeleton } from '../../components/ui/Skeleton';
import EmptyState from '../../components/ui/EmptyState';

const schema = z.object({ text: z.string().min(1, 'Text required') });
type FormData = z.infer<typeof schema>;

const AdminValuesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [modalType, setModalType] = useState<'create' | 'edit' | null>(null);
  const [editTarget, setEditTarget] = useState<Value | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Value | null>(null);
  const [search, setSearch] = useState('');

  const { data: values = [], isLoading } = useQuery({
    queryKey: ['values'],
    queryFn: getValues,
  });

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const createMutation = useMutation({
    mutationFn: (data: FormData) => createValue(data.text),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['values'] });
      toast.success('Value created!');
      setModalType(null);
      reset();
    },
    onError: () => toast.error('Failed to create value'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, text }: { id: string; text: string }) => updateValue(id, text),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['values'] });
      toast.success('Value updated!');
      setModalType(null);
      setEditTarget(null);
      reset();
    },
    onError: () => toast.error('Failed to update value'),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteValue,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['values'] });
      toast.success('Value deleted');
      setDeleteTarget(null);
    },
    onError: () => toast.error('Failed to delete value'),
  });


  const openEdit = (v: Value) => {
    reset({ text: v.text });
    setEditTarget(v);
    setModalType('edit');
  };

  const onSubmit = (data: FormData) => {
    if (modalType === 'create') createMutation.mutate(data);
    else if (editTarget) updateMutation.mutate({ id: editTarget.id, text: data.text });
  };

  const filtered = (values as Value[]).filter((v) =>
    v.text.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="page-enter max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-white font-orbitron">
            Values <span className="text-cyan-400">Management</span>
          </h1>
          <p className="text-slate-400 text-sm mt-1">Property value definitions</p>
        </div>
        <div className="flex items-center gap-2">
           <span className="text-xs text-slate-500 bg-slate-800/50 px-3 py-1 rounded-full border border-slate-700">
             Manage via Properties page
           </span>
        </div>
      </div>

      <div className="mb-4 max-w-sm">
        <Input
          id="values-search"
          placeholder="Search values..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          icon={
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          }
        />
      </div>

      <div className="glass-card overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-white/5">
              <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase">#</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase">Text</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-slate-400 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
            {isLoading
              ? Array.from({ length: 5 }).map((_, i) => <TableRowSkeleton key={i} cols={3} />)
              : filtered.map((v, idx) => (
                  <tr key={v.id} className="hover:bg-white/2 transition-colors" id={`value-row-${v.id}`}>
                    <td className="px-4 py-3 text-sm text-slate-500 font-mono">{idx + 1}</td>
                    <td className="px-4 py-3 text-sm text-white">{v.text}</td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        <button onClick={() => openEdit(v)} id={`edit-value-${v.id}`}
                          className="text-xs px-2.5 py-1 rounded-lg border border-cyan-500/30 text-cyan-400 hover:bg-cyan-500/10 transition-all cursor-pointer">
                          Edit
                        </button>
                        <button onClick={() => setDeleteTarget(v)} id={`delete-value-${v.id}`}
                          className="text-xs px-2.5 py-1 rounded-lg border border-red-500/30 text-red-400 hover:bg-red-500/10 transition-all cursor-pointer">
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
          </tbody>
        </table>
        {!isLoading && filtered.length === 0 && (
          <EmptyState title="No values found" description="Add your first property value" />
        )}
      </div>

      <Modal isOpen={modalType !== null} onClose={() => { setModalType(null); setEditTarget(null); }} title={modalType === 'create' ? 'Add Value' : 'Edit Value'} size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input label="Value Text" id="value-text-input" placeholder="e.g. 30 min / 4K / 120 km/h..." {...register('text')} error={errors.text?.message} />
          <div className="flex gap-3">
            <Button type="button" variant="ghost" onClick={() => setModalType(null)}>Cancel</Button>
            <Button type="submit" fullWidth loading={isSubmitting || createMutation.isPending || updateMutation.isPending} id="value-form-submit">
              {modalType === 'create' ? 'Create' : 'Update'}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal isOpen={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="Delete Value" size="sm">
        <p className="text-slate-300 mb-6">Delete value <strong className="text-white">"{deleteTarget?.text}"</strong>?</p>
        <div className="flex gap-3">
          <Button variant="ghost" onClick={() => setDeleteTarget(null)}>Cancel</Button>
          <Button variant="danger" fullWidth loading={deleteMutation.isPending} onClick={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)} id="delete-value-confirm">
            Delete
          </Button>
        </div>
      </Modal>
    </div>
  );
};

export default AdminValuesPage;
