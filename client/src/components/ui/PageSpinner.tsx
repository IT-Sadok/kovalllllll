import React from 'react';

const PageSpinner: React.FC = () => (
  <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center">
    <div
      className="w-8 h-8 rounded-full border-2 border-slate-700 border-t-cyan-400 animate-spin"
      role="status"
      aria-label="Loading"
    />
  </div>
);

export default PageSpinner;
