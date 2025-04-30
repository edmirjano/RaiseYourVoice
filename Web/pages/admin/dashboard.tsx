import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { GetServerSideProps } from 'next';
import MainLayout from '../components/MainLayout';
import { getCurrentUser } from '../services/authService';
import { useRouter } from 'next/router';

// Dashboard Tabs
import NotificationPanel from '../components/admin/NotificationPanel';
import ContentModerationPanel from '../components/admin/ContentModerationPanel';
import OrganizationVerificationPanel from '../components/admin/OrganizationVerificationPanel';
import AnalyticsPanel from '../components/admin/AnalyticsPanel';

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common', 'admin'])),
    },
  };
};

const ModeratorDashboard: React.FC = () => {
  const { t } = useTranslation('admin');
  const router = useRouter();
  const [activeTab, setActiveTab] = useState('content');
  const user = getCurrentUser();
  
  // Redirect if not logged in or not a moderator/admin
  if (!user || (user.role !== 'Admin' && user.role !== 'Moderator')) {
    if (typeof window !== 'undefined') {
      router.push('/auth/login');
    }
    return null;
  }
  
  const renderTabContent = () => {
    switch (activeTab) {
      case 'notifications':
        return <NotificationPanel />;
      case 'content':
        return <ContentModerationPanel />;
      case 'organizations':
        return <OrganizationVerificationPanel />;
      case 'analytics':
        return <AnalyticsPanel />;
      default:
        return <ContentModerationPanel />;
    }
  };
  
  return (
    <MainLayout>
      <div className="bg-white rounded-lg shadow-md mb-6 ios-card">
        <div className="p-6">
          <h1 className="text-2xl font-semibold mb-2">{t('dashboard.title')}</h1>
          <p className="text-gray-600">{t('dashboard.subtitle')}</p>
        </div>
        
        <div className="border-b border-gray-200">
          <nav className="-mb-px flex px-6" aria-label="Tabs">
            <button
              onClick={() => setActiveTab('content')}
              className={`${
                activeTab === 'content'
                  ? 'border-ios-black text-ios-black'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm mr-8`}
            >
              {t('dashboard.tabs.content')}
            </button>
            <button
              onClick={() => setActiveTab('notifications')}
              className={`${
                activeTab === 'notifications'
                  ? 'border-ios-black text-ios-black'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm mr-8`}
            >
              {t('dashboard.tabs.notifications')}
            </button>
            <button
              onClick={() => setActiveTab('organizations')}
              className={`${
                activeTab === 'organizations'
                  ? 'border-ios-black text-ios-black'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm mr-8`}
            >
              {t('dashboard.tabs.organizations')}
            </button>
            <button
              onClick={() => setActiveTab('analytics')}
              className={`${
                activeTab === 'analytics'
                  ? 'border-ios-black text-ios-black'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
            >
              {t('dashboard.tabs.analytics')}
            </button>
          </nav>
        </div>
        
        <div className="p-6">
          {renderTabContent()}
        </div>
      </div>
    </MainLayout>
  );
};

export default ModeratorDashboard;