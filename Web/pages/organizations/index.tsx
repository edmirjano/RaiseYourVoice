import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../../components/MainLayout';
import { OrganizationCard } from '../../components/organizations/OrganizationCard';
import { getOrganizations, getVerifiedOrganizations } from '../../services/organizationService';

const OrganizationsPage: React.FC = () => {
  const { t } = useTranslation('common');
  const [organizations, setOrganizations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filter, setFilter] = useState('all');
  const [searchTerm, setSearchTerm] = useState('');
  
  useEffect(() => {
    const fetchOrganizations = async () => {
      try {
        setLoading(true);
        let data;
        
        if (filter === 'verified') {
          data = await getVerifiedOrganizations();
        } else {
          data = await getOrganizations();
        }
        
        setOrganizations(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch organizations:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchOrganizations();
  }, [filter, t]);
  
  // Filter organizations by search term
  const filteredOrganizations = organizations.filter(org => {
    if (!searchTerm) return true;
    
    const term = searchTerm.toLowerCase();
    return (
      org.name.toLowerCase().includes(term) ||
      org.description.toLowerCase().includes(term) ||
      (org.organizationType && org.organizationType.toLowerCase().includes(term))
    );
  });
  
  return (
    <MainLayout>
      <div className="space-y-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <h1 className="text-3xl font-bold mb-2">{t('organizations.title')}</h1>
          <p className="text-gray-600 mb-6">{t('organizations.description')}</p>
          
          {/* Search and Filters */}
          <div className="flex flex-col md:flex-row md:justify-between md:items-center space-y-4 md:space-y-0 mb-6">
            <div className="flex space-x-2">
              <button
                onClick={() => setFilter('all')}
                className={`px-4 py-2 rounded-full text-sm ${
                  filter === 'all' 
                    ? 'bg-ios-black text-white' 
                    : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                }`}
              >
                {t('organizations.filters.all')}
              </button>
              <button
                onClick={() => setFilter('verified')}
                className={`px-4 py-2 rounded-full text-sm ${
                  filter === 'verified' 
                    ? 'bg-ios-black text-white' 
                    : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                }`}
              >
                {t('organizations.filters.verified')}
              </button>
            </div>
            
            <div className="relative">
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder={t('organizations.searchPlaceholder')}
                className="ios-input pl-10 w-full md:w-64"
              />
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
            </div>
          </div>
        </motion.div>
        
        {loading ? (
          <div className="flex justify-center py-12">
            <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin"></div>
          </div>
        ) : error ? (
          <div className="ios-card p-6 text-center">
            <p className="text-red-600 mb-4">{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="ios-button-secondary"
            >
              {t('common.tryAgain')}
            </button>
          </div>
        ) : filteredOrganizations.length === 0 ? (
          <div className="ios-card p-8 text-center">
            <h3 className="text-xl font-semibold mb-2">{t('organizations.noOrganizations')}</h3>
            <p className="text-gray-600">{t('organizations.noOrganizationsDescription')}</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredOrganizations.map((organization) => (
              <OrganizationCard key={organization.id} organization={organization} />
            ))}
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common'])),
    },
  };
};

export default OrganizationsPage;