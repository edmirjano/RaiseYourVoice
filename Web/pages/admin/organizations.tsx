import React from 'react';
import { GetServerSideProps } from 'next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import OrganizationVerificationPanel from '../../components/admin/OrganizationVerificationPanel';
import ProtectedRoute from '../../components/auth/ProtectedRoute';

const OrganizationsPage: React.FC = () => {
  return (
    <ProtectedRoute requiredRoles={['Admin', 'Moderator']}>
      <AdminLayout>
        <OrganizationVerificationPanel />
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

export default OrganizationsPage;