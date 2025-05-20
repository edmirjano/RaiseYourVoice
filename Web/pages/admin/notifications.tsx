import React from 'react';
import { GetServerSideProps } from 'next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import NotificationPanel from '../../components/admin/NotificationPanel';
import ProtectedRoute from '../../components/auth/ProtectedRoute';

const NotificationsPage: React.FC = () => {
  return (
    <ProtectedRoute requiredRoles={['Admin', 'Moderator']}>
      <AdminLayout>
        <NotificationPanel />
      </AdminLayout>
    </ProtectedRoute>
  );
};

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common', 'admin'])),
    },
  };
};

export default NotificationsPage;