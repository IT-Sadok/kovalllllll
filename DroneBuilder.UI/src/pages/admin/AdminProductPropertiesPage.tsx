import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import {
  getProperties,
  createProperty,
  getValues,
  createValue,
  assignValueToProperty,
  assignValueToProductProperty,
  removePropertyFromProduct,
  removeValueFromProductProperty
} from '../../api/admin';
import { getProductProperties } from '../../api/products';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';

const AdminProductPropertiesPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // --- State for linking existing ---
  const [selectedPropId, setSelectedPropId] = useState('');

  // --- State for creating new ---
  const [newPropName, setNewPropName] = useState('');
  const [newPropInitialValue, setNewPropInitialValue] = useState('');
  const [isCreatingProp, setIsCreatingProp] = useState(false);

  // --- State for inline value creation ---
  const [newValueTexts, setNewValueTexts] = useState<Record<string, string>>({});

  // 1) Endpoints fetching
  const { data: productProps = [], isLoading: loadingProdProps } = useQuery({
    queryKey: ['product-properties', id],
    queryFn: () => getProductProperties(id!),
    enabled: !!id,
  });

  const { data: allProps = [], isLoading: loadingAllProps } = useQuery({
    queryKey: ['properties'],
    queryFn: getProperties,
  });

  const { data: allValues = [] } = useQuery({
    queryKey: ['values'],
    queryFn: getValues,
  });


  const createAndAssignPropMutation = useMutation({
    mutationFn: async ({ name, initialValue }: { name: string; initialValue: string }) => {
      // Create global property
      const newProp = await createProperty({ name, values: [] });
      // We must assign an initial value when creating a property on a product to avoid empty ghost properties.
      const firstVal = await createValue(initialValue, newProp.id);
      await assignValueToProductProperty(id!, newProp.id, firstVal.id);
      return newProp;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      queryClient.invalidateQueries({ queryKey: ['product-properties', id] });
      toast.success('New property created & assigned!');
      setNewPropName('');
      setNewPropInitialValue('');
      setIsCreatingProp(false);
    },
    onError: () => toast.error('Failed to create property.'),
  });

  const createAndAssignValueMutation = useMutation({
    mutationFn: async ({ propertyId, text }: { propertyId: string; text: string }) => {
      // Create global value and bind to property in one call
      const newVal = await createValue(text, propertyId);
      // Assign it to the product!
      await assignValueToProductProperty(id!, propertyId, newVal.id);
      return newVal;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['values'] });
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      queryClient.invalidateQueries({ queryKey: ['product-properties', id] });
      toast.success('Value added!');
      setNewValueTexts((prev) => ({ ...prev, [variables.propertyId]: '' }));
    },
    onError: () => toast.error('Failed to add value.'),
  });

  const assignExistingValueMutation = useMutation({
    mutationFn: async ({ pId, vId }: { pId: string; vId: string }) => {
      try {
        await assignValueToProperty(pId, vId);
      } catch (e: any) {
        // Ignore "already assigned globally" errors
      }
      await assignValueToProductProperty(id!, pId, vId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      queryClient.invalidateQueries({ queryKey: ['product-properties', id] });
      toast.success('Existing value assigned!');
    },
    onError: (e: any) => {
      console.error(e);
      toast.error('Error assigning: ' + (e.response?.data?.message || e.message));
    },
  });

  const removePropMutation = useMutation({
    mutationFn: async (propertyId: string) => {
      await removePropertyFromProduct(id!, propertyId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-properties', id] });
      toast.success('Property removed from product');
    },
    onError: () => toast.error('Failed to remove property'),
  });

  const removeValueMutation = useMutation({
    mutationFn: async ({ propertyId, valueId }: { propertyId: string; valueId: string }) => {
      await removeValueFromProductProperty(id!, propertyId, valueId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-properties', id] });
      toast.success('Value removed from property');
    },
    onError: () => toast.error('Failed to remove value'),
  });

  // Handlers
  const handleCreateProp = () => {
    if (newPropName.trim() && newPropInitialValue.trim()) {
      createAndAssignPropMutation.mutate({ 
        name: newPropName.trim(), 
        initialValue: newPropInitialValue.trim() 
      });
    }
  };

  const handleCreateValue = (propertyId: string) => {
    const text = newValueTexts[propertyId]?.trim();
    if (text) createAndAssignValueMutation.mutate({ propertyId, text });
  };

  const isLoading = loadingProdProps || loadingAllProps;

  const availableProps = allProps.filter(
    (gProp: any) => !productProps.some((pProp: any) => pProp.id === gProp.id)
  );

  return (
    <div className="page-enter max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex items-center gap-4 mb-8">
        <button
          onClick={() => navigate('/admin/products')}
          className="text-slate-400 hover:text-white transition-colors cursor-pointer"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
             <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <div>
          <h1 className="text-2xl font-bold text-white font-orbitron">
            Manage Product <span className="text-cyan-400">Options</span>
          </h1>
          <p className="text-slate-400 text-sm">Create and assign properties locally.</p>
        </div>
      </div>

      {/* Top Actions: Assign Existing or Create New Property */}
      <div className="glass-card p-6 mb-8 border border-[rgba(0,212,255,0.1)]">
        <div className="flex flex-col sm:flex-row gap-8">
          
          <div className="flex-1">
            <h2 className="text-sm font-medium text-slate-300 mb-3">Assign Existing Property</h2>
            <div className="flex flex-col gap-3">
              <select
                value={selectedPropId}
                onChange={(e) => setSelectedPropId(e.target.value)}
                className="w-full bg-[#111827] border border-[rgba(0,212,255,0.12)] rounded-xl px-3 py-2 text-sm text-slate-100 focus:outline-none focus:ring-2 focus:ring-cyan-500/20"
              >
                <option value="">-- Choose from catalogue --</option>
                {availableProps.map((p: any) => (
                  <option key={p.id} value={p.id}>{p.name}</option>
                ))}
              </select>
              
              {selectedPropId && (
                <div className="flex gap-2 animate-fade-in-up">
                  <select
                    className="flex-1 bg-[#0a0f18] border border-[rgba(0,212,255,0.3)] rounded-lg px-2 py-1.5 text-sm text-slate-300 focus:outline-none"
                    onChange={(e) => {
                      if (e.target.value) {
                        assignExistingValueMutation.mutate({ pId: selectedPropId, vId: e.target.value });
                        setSelectedPropId('');
                        e.target.value = '';
                      }
                    }}
                    defaultValue=""
                  >
                    <option value="" disabled>Pick value...</option>
                    {allValues.map((v: any) => (
                        <option key={v.id} value={v.id}>{v.text}</option>
                    ))}
                  </select>

                  <div className="flex flex-1">
                    <input
                      type="text"
                      placeholder="Or type new..."
                      id="new-val-temp-input"
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                           const v = e.currentTarget.value;
                           if (v.trim()) {
                             createAndAssignValueMutation.mutate({ propertyId: selectedPropId, text: v.trim() });
                             setSelectedPropId('');
                             e.currentTarget.value = '';
                           }
                        }
                      }}
                      className="w-full bg-[#0a0f18] border border-[rgba(0,212,255,0.3)] rounded-l-lg px-2 py-1.5 text-sm text-slate-100 focus:outline-none"
                    />
                    <button
                      onClick={() => {
                        const v = (document.getElementById('new-val-temp-input') as HTMLInputElement).value;
                        if (v.trim()) {
                          createAndAssignValueMutation.mutate({ propertyId: selectedPropId, text: v.trim() });
                          setSelectedPropId('');
                        }
                      }}
                      className="px-2 py-1.5 bg-cyan-500/20 text-cyan-400 hover:bg-cyan-500/30 rounded-r-lg"
                    >
                      +
                    </button>
                  </div>
                </div>
              )}
            </div>
          </div>

          <div className="hidden sm:block w-px bg-white/10"></div>

          <div className="flex-1">
            <h2 className="text-sm font-medium text-slate-300 mb-3">Or Create Fresh Property</h2>
            {isCreatingProp ? (
              <div className="flex flex-col gap-3">
                <div className="flex gap-3">
                  <Input
                    id="new-prop-input"
                    placeholder="Prop Name (e.g. Battery)"
                    value={newPropName}
                    onChange={(e) => setNewPropName(e.target.value)}
                    className="flex-1"
                  />
                  <Input
                    id="new-prop-val-input"
                    placeholder="Value (e.g. 5000mAh)"
                    value={newPropInitialValue}
                    onChange={(e) => setNewPropInitialValue(e.target.value)}
                    className="flex-1"
                  />
                </div>
                <div className="flex gap-2">
                  <Button onClick={handleCreateProp} loading={createAndAssignPropMutation.isPending} disabled={!newPropName.trim() || !newPropInitialValue.trim()}>
                    Create & Assign
                  </Button>
                  <Button variant="ghost" onClick={() => { setIsCreatingProp(false); setNewPropName(''); setNewPropInitialValue(''); }}>Cancel</Button>
                </div>
              </div>
            ) : (
              <Button variant="outline" fullWidth onClick={() => setIsCreatingProp(true)} icon={
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4"/></svg>
              }>
                Create New Property +
              </Button>
            )}
          </div>
        </div>
      </div>

      {/* Property List with Inline Values */}
      <div className="space-y-4">
        <h2 className="text-lg font-medium text-white mb-2">Assigned Properties & Options</h2>
        {isLoading ? (
          <p className="text-slate-400">Loading...</p>
        ) : productProps.length === 0 ? (
          <div className="text-slate-400 py-12 text-center border border-dashed border-white/10 rounded-xl">
            This product has no properties yet. Add one above.
          </div>
        ) : (
          productProps.map((prop: any) => (
            <div key={prop.id} className="glass-card p-5 border-l-4 border-l-cyan-500/50 hover:bg-white/[0.02] transition-colors group">
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                <div className="min-w-[200px]">
                  <h3 className="font-bold text-white text-lg tracking-wide flex items-center gap-2">
                    {prop.name}
                    <button 
                      onClick={() => { if(window.confirm('Remove this entire property from the product?')) removePropMutation.mutate(prop.id) }} 
                      className="text-slate-600 hover:text-red-400 opacity-0 group-hover:opacity-100 transition-all" 
                      title="Remove property from product"
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                    </button>
                  </h3>
                  <p className="text-xs text-slate-500 mt-0.5">Property ID: {prop.id.slice(0, 8)}</p>
                </div>
                
                <div className="flex-1">
                  <div className="flex flex-wrap gap-2 mb-3">
                    {prop.values && prop.values.length > 0 ? (
                      prop.values.map((val: any) => (
                         <span key={val.id} className="group/val relative text-sm px-3 py-1 rounded bg-slate-800 text-slate-200 border border-slate-700 shadow-sm flex items-center gap-1.5">
                           {val.text}
                           <button 
                             onClick={() => removeValueMutation.mutate({ propertyId: prop.id, valueId: val.id })} 
                             className="text-slate-500 hover:text-red-400 opacity-0 group-hover/val:opacity-100 transition-opacity ml-1"
                           >
                             <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                           </button>
                         </span>
                      ))
                    ) : (
                      <span className="text-xs text-amber-500/70 py-1">No options yet</span>
                    )}
                  </div>
                  
                  {/* Inline value adder */}
                  <div className="flex items-center gap-2 max-w-sm mt-3 opacity-60 group-hover:opacity-100 transition-opacity">
                    <input
                      type="text"
                      placeholder="Add an option (e.g. Red)..."
                      value={newValueTexts[prop.id] || ''}
                      onChange={(e) => setNewValueTexts({ ...newValueTexts, [prop.id]: e.target.value })}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') handleCreateValue(prop.id);
                      }}
                      className="flex-1 bg-[#0a0f18] border border-[rgba(0,212,255,0.12)] rounded-lg px-3 py-1.5 text-sm text-slate-100 focus:outline-none focus:border-cyan-500/50"
                    />
                    <button
                      onClick={() => handleCreateValue(prop.id)}
                      disabled={!newValueTexts[prop.id]?.trim() || createAndAssignValueMutation.isPending}
                      className="px-3 py-1.5 text-xs font-semibold bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 rounded-lg transition-colors whitespace-nowrap"
                    >
                      + Add
                    </button>
                    {/* Add existing value dropdown inline */}
                    <select
                      className="bg-[#0a0f18] border border-[rgba(0,212,255,0.12)] rounded-lg px-2 py-1.5 text-xs text-slate-300 focus:outline-none max-w-[120px]"
                      onChange={(e) => {
                        if (e.target.value) {
                          assignExistingValueMutation.mutate({ pId: prop.id, vId: e.target.value });
                          e.target.value = '';
                        }
                      }}
                      defaultValue=""
                    >
                      <option value="" disabled>Link existing...</option>
                      {allValues.filter((v: any) => !prop.values?.some((pv: any) => pv.id === v.id)).map((v: any) => (
                          <option key={v.id} value={v.id}>{v.text}</option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>
            </div>
          ))
        )}
      </div>

    </div>
  );
};

export default AdminProductPropertiesPage;
