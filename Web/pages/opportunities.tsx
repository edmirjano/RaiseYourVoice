import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { useState, useEffect } from 'react';
import MainLayout from '../components/MainLayout';
import { getOpportunities, Post } from '../services/postService';
import Link from 'next/link';

export default function Opportunities() {
  const { t } = useTranslation('common');
  const [opportunities, setOpportunities] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchOpportunities = async () => {
      try {
        setLoading(true);
        const data = await getOpportunities();
        setOpportunities(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch opportunities:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };

    fetchOpportunities();
  }, [t]);

  return (
    <MainLayout>
      <div className="space-y-6">
        <h1 className="text-3xl font-bold">{t('opportunities.title')}</h1>
        <p className="text-gray-600">{t('opportunities.description')}</p>

        {/* Create Opportunity Button */}
        <div className="mb-6">
          <button className="ios-button">
            {t('opportunities.createOpportunity')}
          </button>
        </div>

        {loading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-ios-black"></div>
          </div>
        ) : error ? (
          <div className="bg-red-50 p-4 rounded-lg text-red-500">
            {error}
          </div>
        ) : opportunities.length === 0 ? (
          <div className="ios-card p-8 text-center">
            <h3 className="text-xl font-semibold mb-2">{t('opportunities.noPosts.title')}</h3>
            <p className="text-gray-600">{t('opportunities.noPosts.description')}</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {opportunities.map((opportunity) => (
              <Link 
                href={`/opportunities/${opportunity.id}`} 
                key={opportunity.id}
                className="ios-card p-6 hover:shadow-lg transition-shadow ios-fade-in" 
              >
                {opportunity.mediaUrls && opportunity.mediaUrls.length > 0 && (
                  <div className="mb-4">
                    <img 
                      src={opportunity.mediaUrls[0]} 
                      alt={opportunity.title} 
                      className="rounded-lg w-full h-48 object-cover"
                    />
                  </div>
                )}
                
                <h2 className="text-xl font-bold mb-2">{opportunity.title}</h2>
                <p className="text-gray-600 mb-4 line-clamp-3">{opportunity.content}</p>
                
                <div className="flex flex-col space-y-2 text-sm">
                  {opportunity.location && (
                    <div className="flex items-center">
                      <span className="font-semibold mr-2">{t('opportunities.location')}: </span>
                      <span>
                        {opportunity.location.city}, {opportunity.location.country}
                      </span>
                    </div>
                  )}
                  
                  {opportunity.eventDate && (
                    <div className="flex items-center">
                      <span className="font-semibold mr-2">{t('opportunities.date')}: </span>
                      <span>
                        {new Date(opportunity.eventDate).toLocaleDateString()}
                      </span>
                    </div>
                  )}
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </MainLayout>
  );
}

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common'])),
    },
  };
};