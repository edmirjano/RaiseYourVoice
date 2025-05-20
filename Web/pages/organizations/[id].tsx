import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../../components/MainLayout';
import { CampaignCard } from '../../components/campaigns/CampaignCard';
import { getOrganizationById } from '../../services/organizationService';
import { getCampaignsByOrganization } from '../../services/campaignService';

const OrganizationDetailPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { id } = router.query;
  
  const [organization, setOrganization] = useState(null);
  const [campaigns, setCampaigns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState('about');
  
  useEffect(() => {
    const fetchOrganizationAndCampaigns = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const [orgData, campaignsData] = await Promise.all([
          getOrganizationById(id as string),
          getCampaignsByOrganization(id as string)
        ]);
        
        setOrganization(orgData);
        setCampaigns(campaignsData);
        setError('');
      } catch (err) {
        console.error('Failed to fetch organization data:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchOrganizationAndCampaigns();
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
      ) : organization ? (
        <div className="space-y-6">
          {/* Organization Header */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
            className="ios-card overflow-hidden"
          >
            <div className="p-6 flex flex-col md:flex-row items-center">
              {/* Organization Logo */}
              <div className="mb-4 md:mb-0 md:mr-6">
                <div className="h-24 w-24 rounded-full overflow-hidden bg-gray-100 flex items-center justify-center">
                  {organization.logo ? (
                    <img 
                      src={organization.logo} 
                      alt={organization.name} 
                      className="w-full h-full object-cover"
                    />
                  ) : (
                    <span className="text-3xl font-bold text-gray-400">
                      {organization.name.charAt(0)}
                    </span>
                  )}
                </div>
              </div>
              
              {/* Organization Info */}
              <div className="text-center md:text-left md:flex-1">
                <div className="flex flex-col md:flex-row md:items-center md:justify-between">
                  <div>
                    <h1 className="text-2xl font-bold mb-1">{organization.name}</h1>
                    <div className="flex items-center justify-center md:justify-start mb-2">
                      {organization.verificationStatus === 'Verified' && (
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 mr-2">
                          <svg className="h-3 w-3 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                          </svg>
                          {t('organizations.verified')}
                        </span>
                      )}
                      <span className="text-sm text-gray-500">{organization.organizationType}</span>
                    </div>
                  </div>
                  
                  <div className="flex space-x-2 mt-2 md:mt-0">
                    {organization.website && (
                      <a 
                        href={organization.website} 
                        target="_blank" 
                        rel="noopener noreferrer"
                        className="ios-button-secondary text-sm"
                      >
                        {t('organizations.visitWebsite')}
                      </a>
                    )}
                    <button className="ios-button-secondary text-sm">
                      {t('organizations.contact')}
                    </button>
                  </div>
                </div>
              </div>
            </div>
            
            {/* Tabs */}
            <div className="border-t border-gray-100">
              <nav className="flex">
                <button
                  onClick={() => setActiveTab('about')}
                  className={`px-4 py-3 text-sm font-medium ${
                    activeTab === 'about'
                      ? 'border-b-2 border-ios-black text-ios-black'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {t('organizations.tabs.about')}
                </button>
                <button
                  onClick={() => setActiveTab('campaigns')}
                  className={`px-4 py-3 text-sm font-medium ${
                    activeTab === 'campaigns'
                      ? 'border-b-2 border-ios-black text-ios-black'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {t('organizations.tabs.campaigns')}
                  {campaigns.length > 0 && (
                    <span className="ml-1 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100">
                      {campaigns.length}
                    </span>
                  )}
                </button>
                <button
                  onClick={() => setActiveTab('team')}
                  className={`px-4 py-3 text-sm font-medium ${
                    activeTab === 'team'
                      ? 'border-b-2 border-ios-black text-ios-black'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {t('organizations.tabs.team')}
                </button>
                <button
                  onClick={() => setActiveTab('impact')}
                  className={`px-4 py-3 text-sm font-medium ${
                    activeTab === 'impact'
                      ? 'border-b-2 border-ios-black text-ios-black'
                      : 'text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {t('organizations.tabs.impact')}
                </button>
              </nav>
            </div>
          </motion.div>
          
          {/* Tab Content */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3, delay: 0.1 }}
            className="ios-card p-6"
          >
            {activeTab === 'about' && (
              <div>
                <h2 className="text-xl font-semibold mb-4">{t('organizations.aboutUs')}</h2>
                <div className="prose max-w-none">
                  <p className="whitespace-pre-line">{organization.description}</p>
                  
                  {organization.missionStatement && (
                    <div className="mt-6">
                      <h3 className="text-lg font-medium mb-2">{t('organizations.mission')}</h3>
                      <p>{organization.missionStatement}</p>
                    </div>
                  )}
                  
                  {organization.visionStatement && (
                    <div className="mt-6">
                      <h3 className="text-lg font-medium mb-2">{t('organizations.vision')}</h3>
                      <p>{organization.visionStatement}</p>
                    </div>
                  )}
                  
                  <div className="mt-6">
                    <h3 className="text-lg font-medium mb-2">{t('organizations.details')}</h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <p className="text-sm">
                          <span className="font-medium">{t('organizations.founded')}: </span>
                          {new Date(organization.foundingDate).toLocaleDateString()}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm">
                          <span className="font-medium">{t('organizations.type')}: </span>
                          {organization.organizationType}
                        </p>
                      </div>
                      {organization.location && (
                        <div>
                          <p className="text-sm">
                            <span className="font-medium">{t('organizations.headquarters')}: </span>
                            {organization.location.city}, {organization.location.country}
                          </p>
                        </div>
                      )}
                      {organization.operatingRegions && organization.operatingRegions.length > 0 && (
                        <div>
                          <p className="text-sm">
                            <span className="font-medium">{t('organizations.regions')}: </span>
                            {organization.operatingRegions.join(', ')}
                          </p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )}
            
            {activeTab === 'campaigns' && (
              <div>
                <h2 className="text-xl font-semibold mb-4">{t('organizations.campaigns')}</h2>
                
                {campaigns.length === 0 ? (
                  <div className="text-center py-8 text-gray-500">
                    {t('organizations.noCampaigns')}
                  </div>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {campaigns.map((campaign) => (
                      <CampaignCard key={campaign.id} campaign={campaign} />
                    ))}
                  </div>
                )}
              </div>
            )}
            
            {activeTab === 'team' && (
              <div>
                <h2 className="text-xl font-semibold mb-4">{t('organizations.team')}</h2>
                
                {organization.teamMembers && organization.teamMembers.length > 0 ? (
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {organization.teamMembers.map((member) => (
                      <div key={member.id} className="text-center">
                        <div className="mb-2 mx-auto">
                          {member.photoUrl ? (
                            <img 
                              src={member.photoUrl} 
                              alt={member.name} 
                              className="h-24 w-24 rounded-full object-cover mx-auto"
                            />
                          ) : (
                            <div className="h-24 w-24 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-semibold mx-auto">
                              {member.name.charAt(0)}
                            </div>
                          )}
                        </div>
                        <h3 className="font-medium">{member.name}</h3>
                        <p className="text-sm text-gray-500">{member.title}</p>
                        {member.bio && (
                          <p className="text-sm text-gray-600 mt-2">{member.bio}</p>
                        )}
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    {t('organizations.noTeamMembers')}
                  </div>
                )}
              </div>
            )}
            
            {activeTab === 'impact' && (
              <div>
                <h2 className="text-xl font-semibold mb-4">{t('organizations.impact')}</h2>
                
                {organization.impactMetrics ? (
                  <div className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <div className="ios-card p-4 text-center">
                        <div className="text-3xl font-bold text-ios-black mb-1">
                          {organization.impactMetrics.peopleHelped || 0}
                        </div>
                        <div className="text-sm text-gray-500">{t('organizations.peopleHelped')}</div>
                      </div>
                      <div className="ios-card p-4 text-center">
                        <div className="text-3xl font-bold text-ios-black mb-1">
                          {organization.impactMetrics.areasCovered || 0}
                        </div>
                        <div className="text-sm text-gray-500">{t('organizations.areasCovered')}</div>
                      </div>
                      <div className="ios-card p-4 text-center">
                        <div className="text-3xl font-bold text-ios-black mb-1">
                          {organization.impactMetrics.volunteerHours || 0}
                        </div>
                        <div className="text-sm text-gray-500">{t('organizations.volunteerHours')}</div>
                      </div>
                    </div>
                    
                    {organization.pastProjects && organization.pastProjects.length > 0 && (
                      <div className="mt-6">
                        <h3 className="text-lg font-medium mb-4">{t('organizations.pastProjects')}</h3>
                        <div className="space-y-4">
                          {organization.pastProjects.map((project) => (
                            <div key={project.id} className="ios-card p-4">
                              <h4 className="font-medium mb-1">{project.title}</h4>
                              <p className="text-sm text-gray-600 mb-2">{project.description}</p>
                              <div className="flex flex-wrap gap-2 text-xs text-gray-500">
                                <span>
                                  {new Date(project.startDate).toLocaleDateString()} - 
                                  {project.endDate ? new Date(project.endDate).toLocaleDateString() : t('organizations.present')}
                                </span>
                                {project.impactDescription && (
                                  <span className="text-green-600">{project.impactDescription}</span>
                                )}
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    {t('organizations.noImpactData')}
                  </div>
                )}
              </div>
            )}
          </motion.div>
        </div>
      ) : (
        <div className="ios-card p-6 text-center">
          <p className="text-gray-600 mb-4">{t('organizations.organizationNotFound')}</p>
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

export default OrganizationDetailPage;