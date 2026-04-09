import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getWarehouseItems, addWarehouseQuantity, removeWarehouseQuantity } from '../../api/admin';
import type { WarehouseItem } from '../../types';
import Button from '../../components/ui/Button';
import Input from '../../components/ui/Input';
import Modal from '../../components/ui/Modal';
import Pagination from '../../components/ui/Pagination';
import { TableRowSkeleton } from '../../components/ui/Skeleton';
import EmptyState from '../../components/ui/EmptyState';

const AdminWarehousePage: React.FC = () => {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [modalItem, setModalItem] = useState<{ item: WarehouseItem; type: 'add' | 'remove' } | null>(null);
  const [qty, setQty] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['warehouse', page],
    queryFn: () => getWarehouseItems(page, 15),
  });

  // POST /warehouse/items/{id} body: { quantityToAdd } → returns updated WarehouseItem
  const addMutation = useMutation({
    mutationFn: ({ id, qty }: { id: string; qty: number }) =>
      addWarehouseQuantity(id, qty),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['warehouse'] });
      toast.success('Quantity added');
      setModalItem(null);
      setQty('');
    },
    onError: () => toast.error('Failed to add quantity'),
  });

  // DELETE /warehouse/items/{id} body: { quantityToRemove } → returns updated WarehouseItem
  const removeMutation = useMutation({
    mutationFn: ({ id, qty }: { id: string; qty: number }) =>
      removeWarehouseQuantity(id, qty),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['warehouse'] });
      toast.success('Quantity removed');
      setModalItem(null);
      setQty('');
    },
    onError: () => toast.error('Failed to remove quantity'),
  });

  const handleSubmit = () => {
    const n = parseInt(qty);
    if (!n || n <= 0 || !modalItem) return;
    if (modalItem.type === 'add') {
      addMutation.mutate({ id: modalItem.item.id, qty: n });
    } else {
      removeMutation.mutate({ id: modalItem.item.id, qty: n });
    }
  };

  const getStockStatus = (quantity: number) => {
    if (quantity === 0) return { label: 'Out of Stock', className: 'text-red-400' };
    if (quantity < 5) return { label: 'Low Stock', className: 'text-amber-400' };
    return { label: 'In Stock', className: 'text-emerald-400' };
  };

  return (
    <div className="page-enter max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-white font-orbitron">
          Warehouse <span className="text-cyan-400">Inventory</span>
        </h1>
        <p className="text-slate-400 text-sm mt-1">Manage stock levels — track inventory and adjust quantities</p>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
        {[
          { label: 'Total Items', value: data?.totalCount ?? '—', color: 'text-cyan-400' },
          { label: 'In Stock', value: data?.items.filter((i) => i.quantity > 0).length ?? '—', color: 'text-emerald-400' },
          { label: 'Out of Stock', value: data?.items.filter((i) => i.quantity === 0).length ?? '—', color: 'text-red-400' },
        ].map((stat) => (
          <div key={stat.label} className="glass-card p-4">
            <p className="text-sm text-slate-400">{stat.label}</p>
            <p className={`text-2xl font-bold font-orbitron mt-1 ${stat.color}`}>{stat.value}</p>
          </div>
        ))}
      </div>

      {/* Table — WarehouseItem: id, warehouseId, productId, quantity */}
      <div className="glass-card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-white/5">
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase">Product</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase">Quantity</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-slate-400 uppercase">Status</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-slate-400 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {isLoading
                ? Array.from({ length: 8 }).map((_, i) => <TableRowSkeleton key={i} cols={5} />)
                : data?.items.map((item: WarehouseItem) => {
                    const status = getStockStatus(item.quantity);
                    return (
                      <tr key={item.id} className="hover:bg-white/2 transition-colors" id={`warehouse-row-${item.id}`}>
                        <td className="px-4 py-3">
                          <div className="flex flex-col">
                            <span className="text-sm font-medium text-white">{item.productName}</span>
                            <span className="text-[10px] font-mono text-slate-500 uppercase tracking-tighter">ID: {item.productId.slice(0, 8)}</span>
                          </div>
                        </td>
                        <td className="px-4 py-3 text-sm">
                          <span className="font-orbitron font-bold text-white">{item.quantity}</span>
                          <span className="text-slate-500 ml-1">units</span>
                        </td>
                        <td className="px-4 py-3">
                          <span className={`text-xs font-medium ${status.className}`}>{status.label}</span>
                        </td>
                        <td className="px-4 py-3">
                          <div className="flex justify-end gap-2">
                            <button
                              onClick={() => { setModalItem({ item, type: 'add' }); setQty(''); }}
                              id={`warehouse-add-${item.id}`}
                              className="text-xs px-2.5 py-1 rounded-lg border border-emerald-500/30 text-emerald-400 hover:bg-emerald-500/10 transition-all cursor-pointer"
                            >
                              + Add
                            </button>
                            <button
                              onClick={() => { setModalItem({ item, type: 'remove' }); setQty(''); }}
                              id={`warehouse-remove-${item.id}`}
                              className="text-xs px-2.5 py-1 rounded-lg border border-red-500/30 text-red-400 hover:bg-red-500/10 transition-all cursor-pointer"
                            >
                              − Remove
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
            </tbody>
          </table>
          {!isLoading && data?.items.length === 0 && (
            <EmptyState title="No warehouse items" description="No inventory data available" />
          )}
        </div>
      </div>

      {data && (
        <Pagination page={page} totalCount={data.totalCount} pageSize={15} onPageChange={setPage} />
      )}

      {/* Add/Remove Modal */}
      <Modal
        isOpen={!!modalItem}
        onClose={() => { setModalItem(null); setQty(''); }}
        title={modalItem?.type === 'add' ? 'Add Stock' : 'Remove Stock'}
        size="sm"
      >
        <p className="text-slate-300 text-sm mb-1">
          {modalItem?.type === 'add' ? 'Adding stock for' : 'Removing stock from'}:
        </p>
        <p className="text-lg font-bold text-white mb-4">
          {modalItem?.item.productName}
        </p>
        <div className="flex justify-between items-center mb-4 p-3 rounded-xl bg-white/2 border border-white/5">
          <span className="text-xs text-slate-400 uppercase">Current Stock</span>
          <span className="text-xl font-bold font-orbitron text-cyan-400">{modalItem?.item.quantity} units</span>
        </div>
        <Input
          label="Quantity"
          id="warehouse-qty-input"
          type="number"
          min="1"
          placeholder="Enter quantity..."
          value={qty}
          onChange={(e) => setQty(e.target.value)}
        />
        <div className="flex gap-3 mt-4">
          <Button variant="ghost" onClick={() => setModalItem(null)} id="warehouse-cancel">Cancel</Button>
          <Button
            fullWidth
            variant={modalItem?.type === 'add' ? 'primary' : 'danger'}
            loading={addMutation.isPending || removeMutation.isPending}
            onClick={handleSubmit}
            disabled={!qty || parseInt(qty) <= 0}
            id="warehouse-confirm"
          >
            {modalItem?.type === 'add' ? 'Add Stock' : 'Remove Stock'}
          </Button>
        </div>
      </Modal>
    </div>
  );
};

export default AdminWarehousePage;
