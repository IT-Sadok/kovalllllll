import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import type { CreatePropertyRequest, UpdatePropertyRequest } from '../../types';
import toast from 'react-hot-toast';
import { getProperties, createProperty, updateProperty, deleteProperty, getValues, assignValueToProperty, createValue, removeValueFromProperty } from '../../api/admin';
import type { Property } from '../../types';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import Modal from '../../components/ui/Modal';
import { TableRowSkeleton } from '../../components/ui/Skeleton';
import EmptyState from '../../components/ui/EmptyState';

const schema = z.object({
  name: z.string().min(1, 'Name required'),
});
type FormData = z.infer<typeof schema>;

const AdminPropertiesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [modalType, setModalType] = useState<'create' | 'edit' | null>(null);
  const [editTarget, setEditTarget] = useState<Property | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Property | null>(null);
  const [assignTarget, setAssignTarget] = useState<Property | null>(null);
  const [selectedValId, setSelectedValId] = useState('');
  const [newValueText, setNewValueText] = useState('');

  const { data: allValues = [] } = useQuery({
    queryKey: ['values'],
    queryFn: getValues,
  });

  const { data: properties = [], isLoading } = useQuery({
    queryKey: ['properties'],
    queryFn: getProperties,
  });

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const createMutation = useMutation({
    mutationFn: (data: FormData) => {
      const payload: CreatePropertyRequest = { name: data.name, values: [] };
      return createProperty(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      toast.success('Property created!');
      setModalType(null);
      reset();
    },
    onError: () => toast.error('Failed to create property'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: FormData }) => {
      const payload: UpdatePropertyRequest = { name: data.name };
      return updateProperty(id, payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      toast.success('Property updated!');
      setModalType(null);
      setEditTarget(null);
      reset();
    },
    onError: () => toast.error('Failed to update property'),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteProperty,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      toast.success('Property deleted');
      setDeleteTarget(null);
    },
    onError: () => toast.error('Failed to delete property'),
  });

  const assignMutation = useMutation({
    mutationFn: ({ pId, vId }: { pId: string; vId: string }) => assignValueToProperty(pId, vId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      toast.success('Value assigned!');
      setSelectedValId('');
      setAssignTarget(null);
    },
    onError: () => toast.error('Failed to assign value. It might already be assigned.'),
  });

  const createAndAssignMutation = useMutation({
    mutationFn: ({ pId, text }: { pId: string; text: string }) => createValue(text, pId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      queryClient.invalidateQueries({ queryKey: ['values'] });
      toast.success('New value created & assigned!');
      setNewValueText('');
    },
    onError: () => toast.error('Failed to create and assign value.'),
  });

  const removeValMutation = useMutation({
    mutationFn: ({ pId, vId }: { pId: string; vId: string }) => removeValueFromProperty(pId, vId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      queryClient.invalidateQueries({ queryKey: ['values'] });
      toast.success('Value unlinked & cleaned up!');
    },
    onError: () => toast.error('Failed to remove value.'),
  });

  const openCreate = () => {
    reset({ name: '' });
    setEditTarget(null);
    setModalType('create');
  };

  const openEdit = (prop: Property) => {
    reset({ name: prop.name });
    setEditTarget(prop);
    setModalType('edit');
  };

  const onSubmit = (data: FormData) => {
    if (modalType === 'create') createMutation.mutate(data);
    else if (editTarget) updateMutation.mutate({ id: editTarget.id, data });
  };

  return (
    <div className="page-enter max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-white font-orbitron">
            Properties <span className="text-cyan-400">Management</span>
          </h1>
          <p className="text-slate-400 text-sm mt-1">Drone specification properties</p>
        </div>
        <Button onClick={openCreate} id="create-property-btn" icon={
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
          </svg>
        }>
          Add Property
        </Button>
      </div>

      <div className="glass-card overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-white/5">
              <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase">Name</th>
              <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase">Values</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-slate-400 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
            {isLoading
              ? Array.from({ length: 5 }).map((_, i) => <TableRowSkeleton key={i} cols={3} />)
              : (properties as Property[]).map((prop) => (
                  <tr key={prop.id} className="hover:bg-white/2 transition-colors" id={`prop-row-${prop.id}`}>
                    <td className="px-4 py-3 text-sm font-medium text-white">{prop.name}</td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-1">
                        {prop.values?.map((v) => (
                          <span key={v.id} className="text-xs px-2 py-0.5 rounded bg-slate-700/50 text-slate-300">
                            {v.text}
                          </span>
                        ))}
                        {(!prop.values || prop.values.length === 0) && (
                          <span className="text-xs text-slate-500">No values</span>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => setAssignTarget(prop)}
                          id={`assign-val-${prop.id}`}
                          className="text-xs px-2.5 py-1 rounded-lg border border-fuchsia-500/30 text-fuchsia-400 hover:bg-fuchsia-500/10 transition-all cursor-pointer"
                        >Values</button>
                        <button
                          onClick={() => openEdit(prop)}
                          id={`edit-prop-${prop.id}`}
                          className="text-xs px-2.5 py-1 rounded-lg border border-cyan-500/30 text-cyan-400 hover:bg-cyan-500/10 transition-all cursor-pointer"
                        >Edit</button>
                        <button
                          onClick={() => setDeleteTarget(prop)}
                          id={`delete-prop-${prop.id}`}
                          className="text-xs px-2.5 py-1 rounded-lg border border-red-500/30 text-red-400 hover:bg-red-500/10 transition-all cursor-pointer"
                        >Delete</button>
                      </div>
                    </td>
                  </tr>
                ))}
          </tbody>
        </table>
        {!isLoading && (properties as Property[]).length === 0 && (
          <EmptyState title="No properties" description="Add your first drone property" />
        )}
      </div>

      <Modal isOpen={modalType !== null} onClose={() => { setModalType(null); setEditTarget(null); }} title={modalType === 'create' ? 'Add Property' : 'Edit Property'} size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input label="Property Name" id="property-name-input" placeholder="e.g. Flight Time, Max Speed..." {...register('name')} error={errors.name?.message} />
          <div className="flex gap-3">
            <Button type="button" variant="ghost" onClick={() => setModalType(null)}>Cancel</Button>
            <Button type="submit" fullWidth loading={isSubmitting || createMutation.isPending || updateMutation.isPending} id="property-form-submit">
              {modalType === 'create' ? 'Create' : 'Update'}
            </Button>
          </div>
        </form>
      </Modal>

      <Modal isOpen={!!deleteTarget} onClose={() => setDeleteTarget(null)} title="Delete Property" size="sm">
        <p className="text-slate-300 mb-6">Delete <strong className="text-white">{deleteTarget?.name}</strong>? This cannot be undone.</p>
        <div className="flex gap-3">
          <Button variant="ghost" onClick={() => setDeleteTarget(null)}>Cancel</Button>
          <Button variant="danger" fullWidth loading={deleteMutation.isPending} onClick={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)} id="delete-prop-confirm">
            Delete
          </Button>
        </div>
      </Modal>

      <Modal isOpen={!!assignTarget} onClose={() => { setAssignTarget(null); setNewValueText(''); setSelectedValId(''); }} title={`Values for ${assignTarget?.name}`} size="sm">
        <div className="space-y-6">
          {/* Current Values List */}
          <div className="space-y-3">
             <label className="text-sm font-medium text-slate-300">Currently Assigned</label>
             <div className="flex flex-wrap gap-2 min-h-[40px] p-3 rounded-xl bg-white/2 border border-white/5">
                {assignTarget?.values && assignTarget.values.length > 0 ? (
                  assignTarget.values.map(v => (
                    <span key={v.id} className="group relative text-xs px-2.5 py-1 rounded bg-slate-800 text-slate-300 border border-slate-700 flex items-center gap-2">
                       {v.text}
                       <button 
                         onClick={() => removeValMutation.mutate({ pId: assignTarget.id, vId: v.id })}
                         className="text-slate-500 hover:text-red-400 transition-colors"
                         title="Remove from property"
                       >
                         <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                       </button>
                    </span>
                  ))
                ) : (
                  <span className="text-xs text-slate-500 italic">No values assigned</span>
                )}
             </div>
          </div>

          <div className="space-y-3">
            <label className="text-sm font-medium text-slate-300">Link Existing Value</label>
            <div className="flex gap-2">
              <select
                value={selectedValId}
                onChange={(e) => setSelectedValId(e.target.value)}
                className="flex-1 bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20"
              >
                <option value="">-- Choose --</option>
                {allValues
                  .filter(v => !assignTarget?.values?.some(extV => extV.id === v.id))
                  .map(v => (
                  <option key={v.id} value={v.id}>{v.text}</option>
                ))}
              </select>
              <Button
                size="sm"
                loading={assignMutation.isPending}
                disabled={!selectedValId}
                onClick={() => assignTarget && assignMutation.mutate({ pId: assignTarget.id, vId: selectedValId })}
              >
                Link
              </Button>
            </div>
          </div>

          <div className="relative">
            <div className="absolute inset-0 flex items-center"><span className="w-full border-t border-white/5"></span></div>
            <div className="relative flex justify-center text-xs uppercase"><span className="bg-[#0a0f18] px-2 text-slate-500">Or</span></div>
          </div>

          <div className="space-y-3">
            <label className="text-sm font-medium text-slate-300">Create New Value</label>
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="e.g. 5000 mAh"
                value={newValueText}
                onChange={(e) => setNewValueText(e.target.value)}
                className="flex-1 bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20"
              />
              <Button
                size="sm"
                variant="outline"
                loading={createAndAssignMutation.isPending}
                disabled={!newValueText.trim()}
                onClick={() => assignTarget && createAndAssignMutation.mutate({ pId: assignTarget.id, text: newValueText.trim() })}
              >
                Create
              </Button>
            </div>
          </div>

          <div className="pt-2">
            <Button variant="ghost" fullWidth onClick={() => setAssignTarget(null)}>Close</Button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default AdminPropertiesPage;
