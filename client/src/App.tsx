import { lazy, Suspense, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider, MutationCache } from '@tanstack/react-query';
import Layout from './components/layout/Layout';
import PrivateRoute from './components/PrivateRoute';
import PageSpinner from './components/ui/PageSpinner';
import { useAuthStore } from './store/authStore';
import { useCartStore } from './store/cartStore';
import { useBuilderStore } from './store/builderStore';

// Pages
import HomePage from './pages/HomePage';
const LoginPage = lazy(() => import('./pages/LoginPage'));
const RegisterPage = lazy(() => import('./pages/RegisterPage'));
const ConfirmEmailPage = lazy(() => import('./pages/ConfirmEmailPage'));
const ProductPage = lazy(() => import('./pages/ProductPage'));
const BuilderPage = lazy(() => import('./pages/BuilderPage'));
const MyBuildsPage = lazy(() => import('./pages/MyBuildsPage'));
const CartPage = lazy(() => import('./pages/CartPage'));
const CheckoutPage = lazy(() => import('./pages/CheckoutPage'));
const OrdersPage = lazy(() => import('./pages/OrdersPage'));
const AdminProductsPage = lazy(() => import('./pages/admin/AdminProductsPage'));
const AdminImagesPage = lazy(() => import('./pages/admin/AdminImagesPage'));
const AdminWarehousePage = lazy(() => import('./pages/admin/AdminWarehousePage'));
const AdminProductAttributesPage = lazy(() => import('./pages/admin/AdminProductAttributesPage'));
const AdminProductSpecPage = lazy(() => import('./pages/admin/AdminProductSpecPage'));
const AdminImportPage = lazy(() => import('./pages/admin/AdminImportPage'));
const AdminOrdersPage = lazy(() => import('./pages/admin/AdminOrdersPage'));

const queryClient = new QueryClient({
  mutationCache: new MutationCache({
    onSuccess: () => {
      // Автоматичне очищення всього кешу після будь-якої успішної мутації (створення/оновлення/видалення)
      queryClient.invalidateQueries();
    },
  }),
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 2, // 2 minutes
      retry: 1,
    },
  },
});

function App() {
  const hydrate = useAuthStore((state) => state.hydrate);

  // Asks the server whether the cookie belongs to a live session, since the browser cannot inspect it.
  useEffect(() => {
    void hydrate();
  }, [hydrate]);

  // Cached cart and orders belong to the session that fetched them. Drop them on sign-out or expiry so the
  // next person on this browser never sees them.
  useEffect(
    () =>
      useAuthStore.subscribe((state, prev) => {
        if (prev.status === 'authenticated' && state.status !== 'authenticated') {
          queryClient.clear();
          useCartStore.getState().setItemCount(0);
          useBuilderStore.setState({ saved: null });
        }
      }),
    [],
  );

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Layout>
          <Suspense fallback={<PageSpinner />}>
            <Routes>
              {/* Public */}
              <Route path="/" element={<HomePage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/confirm-email" element={<ConfirmEmailPage />} />
              <Route path="/products/:id" element={<ProductPage />} />
              <Route path="/builder" element={<BuilderPage />} />

              {/* User (authenticated) */}
              <Route path="/cart" element={
                <PrivateRoute><CartPage /></PrivateRoute>
              } />
              <Route path="/checkout" element={
                <PrivateRoute><CheckoutPage /></PrivateRoute>
              } />
              <Route path="/orders" element={
                <PrivateRoute><OrdersPage /></PrivateRoute>
              } />
              <Route path="/builds" element={
                <PrivateRoute><MyBuildsPage /></PrivateRoute>
              } />

              {/* Admin */}
              <Route path="/admin/products" element={
                <PrivateRoute requireAdmin><AdminProductsPage /></PrivateRoute>
              } />
              <Route path="/admin/products/:id/images" element={
                <PrivateRoute requireAdmin><AdminImagesPage /></PrivateRoute>
              } />
              <Route path="/admin/products/:id/attributes" element={
                <PrivateRoute requireAdmin><AdminProductAttributesPage /></PrivateRoute>
              } />
              <Route path="/admin/products/:id/spec" element={
                <PrivateRoute requireAdmin><AdminProductSpecPage /></PrivateRoute>
              } />
              <Route path="/admin/import" element={
                <PrivateRoute requireAdmin><AdminImportPage /></PrivateRoute>
              } />
              <Route path="/admin/warehouse" element={
                <PrivateRoute requireAdmin><AdminWarehousePage /></PrivateRoute>
              } />
              <Route path="/admin/orders" element={
                <PrivateRoute requireAdmin><AdminOrdersPage /></PrivateRoute>
              } />

              {/* Fallback */}
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </Suspense>
        </Layout>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
