import React from 'react';
import { GetServerSideProps } from 'next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import AnalyticsPanel from '../../components/admin/AnalyticsPanel';
import ProtectedRoute from '../../components/auth/ProtectedRoute';

const AnalyticsPage: React.FC = () => {
  return (
    <ProtectedRoute requiredRoles={['Admin', 'Moderator']}>
      <AdminLayout>
        <AnalyticsPanel />
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

export default AnalyticsPage;