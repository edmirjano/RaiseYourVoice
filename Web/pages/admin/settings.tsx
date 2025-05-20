import React, { useState } from 'react';
import { GetServerSideProps } from 'next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { useTranslation } from 'next-i18next';
import AdminLayout from '../../components/admin/layout/AdminLayout';
import ProtectedRoute from '../../components/auth/ProtectedRoute';

const SettingsPage: React.FC = () => {
  const { t } = useTranslation('admin');
  const [activeTab, setActiveTab] = useState('general');
  
  return (
    <ProtectedRoute requiredRoles={['Admin']}>
      <AdminLayout>
        <div className="bg-white rounded-lg shadow-md mb-6 ios-card">
          <div className="p-6">
            <h1 className="text-2xl font-semibold mb-2">{t('settings.title')}</h1>
            <p className="text-gray-600">{t('settings.subtitle')}</p>
          </div>
          
          <div className="border-b border-gray-200">
            <nav className="-mb-px flex px-6" aria-label="Tabs">
              <button
                onClick={() => setActiveTab('general')}
                className={`${
                  activeTab === 'general'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm mr-8`}
              >
                {t('settings.tabs.general')}
              </button>
              <button
                onClick={() => setActiveTab('security')}
                className={`${
                  activeTab === 'security'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm mr-8`}
              >
                {t('settings.tabs.security')}
              </button>
              <button
                onClick={() => setActiveTab('api')}
                className={`${
                  activeTab === 'api'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm mr-8`}
              >
                {t('settings.tabs.api')}
              </button>
              <button
                onClick={() => setActiveTab('email')}
                className={`${
                  activeTab === 'email'
                    ? 'border-ios-black text-ios-black'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm`}
              >
                {t('settings.tabs.email')}
              </button>
            </nav>
          </div>
          
          <div className="p-6">
            {activeTab === 'general' && (
              <div>
                <h2 className="text-lg font-medium mb-4">{t('settings.general.title')}</h2>
                
                <div className="space-y-6">
                  <div>
                    <label htmlFor="site-name" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.general.siteName')}
                    </label>
                    <input
                      type="text"
                      id="site-name"
                      className="ios-input w-full"
                      defaultValue="RaiseYourVoice"
                    />
                  </div>
                  
                  <div>
                    <label htmlFor="site-description" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.general.siteDescription')}
                    </label>
                    <textarea
                      id="site-description"
                      rows={3}
                      className="ios-input w-full"
                      defaultValue="A platform for activists to connect and make a difference."
                    />
                  </div>
                  
                  <div>
                    <label htmlFor="default-language" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.general.defaultLanguage')}
                    </label>
                    <select
                      id="default-language"
                      className="ios-input w-full"
                      defaultValue="en"
                    >
                      <option value="en">English</option>
                      <option value="sq">Albanian</option>
                    </select>
                  </div>
                  
                  <div className="flex justify-end">
                    <button className="ios-button">
                      {t('settings.saveChanges')}
                    </button>
                  </div>
                </div>
              </div>
            )}
            
            {activeTab === 'security' && (
              <div>
                <h2 className="text-lg font-medium mb-4">{t('settings.security.title')}</h2>
                
                <div className="space-y-6">
                  <div>
                    <label htmlFor="password-policy" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.security.passwordPolicy')}
                    </label>
                    <select
                      id="password-policy"
                      className="ios-input w-full"
                      defaultValue="strong"
                    >
                      <option value="basic">{t('settings.security.passwordPolicies.basic')}</option>
                      <option value="medium">{t('settings.security.passwordPolicies.medium')}</option>
                      <option value="strong">{t('settings.security.passwordPolicies.strong')}</option>
                    </select>
                  </div>
                  
                  <div>
                    <label htmlFor="session-timeout" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.security.sessionTimeout')}
                    </label>
                    <select
                      id="session-timeout"
                      className="ios-input w-full"
                      defaultValue="60"
                    >
                      <option value="15">15 {t('settings.security.minutes')}</option>
                      <option value="30">30 {t('settings.security.minutes')}</option>
                      <option value="60">60 {t('settings.security.minutes')}</option>
                      <option value="120">120 {t('settings.security.minutes')}</option>
                    </select>
                  </div>
                  
                  <div className="flex items-center">
                    <input
                      id="two-factor-auth"
                      type="checkbox"
                      className="h-4 w-4 text-ios-black focus:ring-ios-black border-gray-300 rounded"
                      defaultChecked
                    />
                    <label htmlFor="two-factor-auth" className="ml-2 block text-sm text-gray-900">
                      {t('settings.security.requireTwoFactor')}
                    </label>
                  </div>
                  
                  <div className="flex justify-end">
                    <button className="ios-button">
                      {t('settings.saveChanges')}
                    </button>
                  </div>
                </div>
              </div>
            )}
            
            {activeTab === 'api' && (
              <div>
                <h2 className="text-lg font-medium mb-4">{t('settings.api.title')}</h2>
                
                <div className="space-y-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.api.apiKeys')}
                    </label>
                    <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 mb-2">
                      <div className="flex justify-between items-center">
                        <div>
                          <div className="font-medium">Mobile Client</div>
                          <div className="text-sm text-gray-500">Created: 2025-04-01</div>
                        </div>
                        <div className="flex space-x-2">
                          <button className="ios-button-secondary text-sm">
                            {t('settings.api.view')}
                          </button>
                          <button className="ios-button-secondary text-sm">
                            {t('settings.api.revoke')}
                          </button>
                        </div>
                      </div>
                    </div>
                    
                    <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                      <div className="flex justify-between items-center">
                        <div>
                          <div className="font-medium">Analytics Service</div>
                          <div className="text-sm text-gray-500">Created: 2025-04-15</div>
                        </div>
                        <div className="flex space-x-2">
                          <button className="ios-button-secondary text-sm">
                            {t('settings.api.view')}
                          </button>
                          <button className="ios-button-secondary text-sm">
                            {t('settings.api.revoke')}
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                  
                  <div>
                    <button className="ios-button-secondary">
                      {t('settings.api.generateKey')}
                    </button>
                  </div>
                  
                  <div>
                    <label htmlFor="rate-limit" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.api.rateLimit')}
                    </label>
                    <select
                      id="rate-limit"
                      className="ios-input w-full"
                      defaultValue="60"
                    >
                      <option value="30">30 {t('settings.api.requestsPerMinute')}</option>
                      <option value="60">60 {t('settings.api.requestsPerMinute')}</option>
                      <option value="120">120 {t('settings.api.requestsPerMinute')}</option>
                      <option value="300">300 {t('settings.api.requestsPerMinute')}</option>
                    </select>
                  </div>
                  
                  <div className="flex justify-end">
                    <button className="ios-button">
                      {t('settings.saveChanges')}
                    </button>
                  </div>
                </div>
              </div>
            )}
            
            {activeTab === 'email' && (
              <div>
                <h2 className="text-lg font-medium mb-4">{t('settings.email.title')}</h2>
                
                <div className="space-y-6">
                  <div>
                    <label htmlFor="smtp-host" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.email.smtpHost')}
                    </label>
                    <input
                      type="text"
                      id="smtp-host"
                      className="ios-input w-full"
                      defaultValue="smtp.example.com"
                    />
                  </div>
                  
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label htmlFor="smtp-port" className="block text-sm font-medium text-gray-700 mb-1">
                        {t('settings.email.smtpPort')}
                      </label>
                      <input
                        type="text"
                        id="smtp-port"
                        className="ios-input w-full"
                        defaultValue="587"
                      />
                    </div>
                    
                    <div>
                      <label htmlFor="smtp-security" className="block text-sm font-medium text-gray-700 mb-1">
                        {t('settings.email.smtpSecurity')}
                      </label>
                      <select
                        id="smtp-security"
                        className="ios-input w-full"
                        defaultValue="tls"
                      >
                        <option value="none">{t('settings.email.securityOptions.none')}</option>
                        <option value="ssl">{t('settings.email.securityOptions.ssl')}</option>
                        <option value="tls">{t('settings.email.securityOptions.tls')}</option>
                      </select>
                    </div>
                  </div>
                  
                  <div>
                    <label htmlFor="smtp-username" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.email.smtpUsername')}
                    </label>
                    <input
                      type="text"
                      id="smtp-username"
                      className="ios-input w-full"
                      defaultValue="notifications@raiseyourvoice.al"
                    />
                  </div>
                  
                  <div>
                    <label htmlFor="smtp-password" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.email.smtpPassword')}
                    </label>
                    <input
                      type="password"
                      id="smtp-password"
                      className="ios-input w-full"
                      defaultValue="••••••••••••"
                    />
                  </div>
                  
                  <div>
                    <label htmlFor="from-email" className="block text-sm font-medium text-gray-700 mb-1">
                      {t('settings.email.fromEmail')}
                    </label>
                    <input
                      type="email"
                      id="from-email"
                      className="ios-input w-full"
                      defaultValue="no-reply@raiseyourvoice.al"
                    />
                  </div>
                  
                  <div className="flex justify-end">
                    <button className="ios-button-secondary mr-2">
                      {t('settings.email.testConnection')}
                    </button>
                    <button className="ios-button">
                      {t('settings.saveChanges')}
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
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

export default SettingsPage;