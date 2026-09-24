import { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider, MutationCache } from '@tanstack/react-query';
import Layout from './components/layout/Layout';
import PrivateRoute from './components/PrivateRoute';
import { useAuthStore } from './store/authStore';
import { useCartStore } from './store/cartStore';

// Pages
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ConfirmEmailPage from './pages/ConfirmEmailPage';
import ProductPage from './pages/ProductPage';
import CartPage from './pages/CartPage';
import CheckoutPage from './pages/CheckoutPage';
import OrdersPage from './pages/OrdersPage';
import AdminProductsPage from './pages/admin/AdminProductsPage';
import AdminImagesPage from './pages/admin/AdminImagesPage';
import AdminWarehousePage from './pages/admin/AdminWarehousePage';
import AdminProductAttributesPage from './pages/admin/AdminProductAttributesPage';
import AdminProductSpecPage from './pages/admin/AdminProductSpecPage';
import AdminOrdersPage from './pages/admin/AdminOrdersPage';

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
        }
      }),
    [],
  );

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Layout>
          <Routes>
            {/* Public */}
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/confirm-email" element={<ConfirmEmailPage />} />
            <Route path="/products/:id" element={<ProductPage />} />

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
            <Route path="/admin/warehouse" element={
              <PrivateRoute requireAdmin><AdminWarehousePage /></PrivateRoute>
            } />
            <Route path="/admin/orders" element={
              <PrivateRoute requireAdmin><AdminOrdersPage /></PrivateRoute>
            } />

            {/* Fallback */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Layout>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
