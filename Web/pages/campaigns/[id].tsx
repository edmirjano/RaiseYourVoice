import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../../components/MainLayout';
import { CampaignDetails } from '../../components/campaigns/CampaignDetails';
import { getCampaignById } from '../../services/campaignService';

const CampaignDetailPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { id } = router.query;
  
  const [campaign, setCampaign] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  
  useEffect(() => {
    const fetchCampaign = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const data = await getCampaignById(id as string);
        setCampaign(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch campaign:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchCampaign();
  }, [id, t]);
  
  return (
    <MainLayout>
      {loading ? (
        <div className="flex justify-center py-12">
          <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin"></div>
        </div>
      ) : error ? (
        <div className="ios-card p-6 text-center">
          <p className="text-red-600 mb-4">{error}</p>
          <button
            onClick={() => router.back()}
            className="ios-button-secondary"
          >
            {t('common.goBack')}
          </button>
        </div>
      ) : campaign ? (
        <CampaignDetails campaign={campaign} />
      ) : (
        <div className="ios-card p-6 text-center">
          <p className="text-gray-600 mb-4">{t('campaigns.campaignNotFound')}</p>
          <button
            onClick={() => router.back()}
            className="ios-button-secondary"
          >
            {t('common.goBack')}
          </button>
        </div>
      )}
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

export default CampaignDetailPage;