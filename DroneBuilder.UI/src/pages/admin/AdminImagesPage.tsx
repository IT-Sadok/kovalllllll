import React, { useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { getProductImages, uploadImage, deleteImage, setPrimaryImage } from '../../api/admin';
import type { Image } from '../../types';
import { ProductCardSkeleton } from '../../components/ui/Skeleton';
import EmptyState from '../../components/ui/EmptyState';

const AdminImagesPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // GET /images/product/{productId} → Image[]
  const { data: images = [], isLoading } = useQuery({
    queryKey: ['product-images-admin', id],
    queryFn: () => getProductImages(id!),
    enabled: !!id,
  });

  // POST /images/upload — multipart/form-data: file, productId
  const uploadMutation = useMutation({
    mutationFn: (file: File) => uploadImage(file, id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-images-admin', id] });
      queryClient.invalidateQueries({ queryKey: ['product-images', id] });
      toast.success('Image uploaded!');
    },
    onError: () => toast.error('Upload failed'),
  });

  // DELETE /images/{imageId}
  const deleteMutation = useMutation({
    mutationFn: deleteImage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-images-admin', id] });
      queryClient.invalidateQueries({ queryKey: ['product-images', id] });
      queryClient.invalidateQueries({ queryKey: ['admin-products'] });
      toast.success('Image deleted');
    },
    onError: () => toast.error('Failed to delete image'),
  });

  // POST /images/{imageId}/set-primary
  const setPrimaryMutation = useMutation({
    mutationFn: setPrimaryImage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product-images-admin', id] });
      queryClient.invalidateQueries({ queryKey: ['product-images', id] });
      queryClient.invalidateQueries({ queryKey: ['admin-products'] });
      toast.success('Primary image updated');
    },
    onError: () => toast.error('Failed to set primary image'),
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      uploadMutation.mutate(file);
      e.target.value = '';
    }
  };

  // Sort images so primary comes first
  const sortedImages = [...images].sort((a, b) => (a.isPrimary === b.isPrimary ? 0 : a.isPrimary ? -1 : 1));

  return (
    <div className="page-enter max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="flex items-center gap-4 mb-8">
        <button
          onClick={() => navigate('/admin/products')}
          id="images-back-btn"
          className="text-slate-400 hover:text-white transition-colors cursor-pointer"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
        </button>
        <div>
          <h1 className="text-2xl font-bold text-white font-orbitron">
            Product <span className="text-violet-400">Images</span>
          </h1>
          <p className="text-slate-400 text-sm">Product ID: {id?.slice(0, 8)}...</p>
        </div>
      </div>

      {/* Upload zone */}
      <div
        className="glass-card border-2 border-dashed border-[rgba(0,212,255,0.2)] hover:border-cyan-400/40 transition-all p-8 text-center mb-8 cursor-pointer"
        onClick={() => fileInputRef.current?.click()}
        id="image-upload-zone"
      >
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          className="hidden"
          id="image-file-input"
          onChange={handleFileChange}
        />
        {uploadMutation.isPending ? (
          <div className="flex flex-col items-center gap-3">
            <svg className="animate-spin w-8 h-8 text-cyan-400" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
            </svg>
            <p className="text-slate-400 text-sm">Uploading...</p>
          </div>
        ) : (
          <div className="flex flex-col items-center gap-3">
            <div className="w-12 h-12 rounded-xl bg-cyan-500/10 border border-cyan-500/20 flex items-center justify-center">
              <svg className="w-6 h-6 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <div>
              <p className="text-slate-300 font-medium">Click to upload image</p>
              <p className="text-slate-500 text-xs mt-1">PNG, JPG, WebP supported</p>
            </div>
          </div>
        )}
      </div>

      {/* Gallery — Image: id, url, fileName, uploadedAt */}
      <h2 className="text-sm font-semibold text-slate-400 mb-4">
        {images.length} image{images.length !== 1 ? 's' : ''}
      </h2>

      {isLoading ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          {Array.from({ length: 4 }).map((_, i) => <ProductCardSkeleton key={i} />)}
        </div>
      ) : images.length === 0 ? (
        <EmptyState
          title="No images yet"
          description="Upload your first product image above"
        />
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
          {sortedImages.map((img) => (
            <div key={img.id} className={`glass-card overflow-hidden group relative border-2 ${img.isPrimary ? 'border-amber-500/50' : 'border-transparent'}`} id={`image-card-${img.id}`}>
              <img src={img.url} alt={img.fileName} className="w-full aspect-square object-cover" />
              
              {/* Primary badge */}
              {img.isPrimary && (
                <div className="absolute top-2 left-2 px-2 py-0.5 rounded bg-amber-500 text-[10px] font-bold text-black uppercase tracking-widest z-10 shadow-lg">
                  Main
                </div>
              )}

              {/* Hover overlay with actions */}
              <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex flex-col items-center justify-center gap-3">
                <div className="flex gap-2">
                  {!img.isPrimary && (
                    <button
                      onClick={() => setPrimaryMutation.mutate(img.id)}
                      id={`set-primary-${img.id}`}
                      title="Set as Main"
                      className="p-2 bg-amber-500/80 hover:bg-amber-500 rounded-lg text-black transition-all cursor-pointer"
                    >
                      <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                      </svg>
                    </button>
                  )}
                  <button
                    onClick={() => deleteMutation.mutate(img.id)}
                    id={`delete-image-${img.id}`}
                    disabled={deleteMutation.isPending}
                    title="Delete"
                    className="p-2 bg-red-500/80 hover:bg-red-500 rounded-lg text-white transition-all cursor-pointer"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
                <span className="text-[10px] text-white/70 text-center px-4 truncate w-full font-mono">
                  {img.fileName}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default AdminImagesPage;
