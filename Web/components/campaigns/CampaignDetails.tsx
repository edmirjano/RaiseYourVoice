import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { formatDistanceToNow } from 'date-fns';
import { DonationForm } from './DonationForm';

interface Campaign {
  id: string;
  title: string;
  description: string;
  organizationId: string;
  organizationName?: string;
  goal: number;
  amountRaised: number;
  startDate: string;
  endDate: string;
  status: string;
  coverImageUrl?: string;
  additionalImagesUrls?: string[];
  videoUrl?: string;
  category: string;
  updates?: Array<{
    id: string;
    title: string;
    content: string;
    postedAt: string;
  }>;
  milestones?: Array<{
    id: string;
    title: string;
    description: string;
    targetAmount: number;
    isCompleted: boolean;
  }>;
}

interface CampaignDetailsProps {
  campaign: Campaign;
}

export const CampaignDetails: React.FC<CampaignDetailsProps> = ({ campaign }) => {
  const { t } = useTranslation('common');
  const [activeTab, setActiveTab] = useState('about');
  
  // Calculate progress percentage
  const progressPercentage = Math.min(100, Math.round((campaign.amountRaised / campaign.goal) * 100));
  
  // Calculate days remaining
  const daysRemaining = Math.max(0, Math.ceil((new Date(campaign.endDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24)));
  
  const formatDate = (dateString: string) => {
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
      {/* Main Content */}
      <div className="lg:col-span-2">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
          className="ios-card overflow-hidden mb-6"
        >
          {/* Campaign Image */}
          <div className="relative h-64 md:h-96">
            <img 
              src={campaign.coverImageUrl || 'https://images.pexels.com/photos/3184418/pexels-photo-3184418.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2'} 
              alt={campaign.title} 
              className="w-full h-full object-cover"
            />
            <div className="absolute top-4 right-4">
              <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${
                campaign.status === 'Active' ? 'bg-green-100 text-green-800' :
                campaign.status === 'Completed' ? 'bg-blue-100 text-blue-800' :
                campaign.status === 'Paused' ? 'bg-yellow-100 text-yellow-800' :
                'bg-gray-100 text-gray-800'
              }`}>
                {campaign.status}
              </span>
            </div>
          </div>
          
          {/* Campaign Header */}
          <div className="p-6 border-b border-gray-100">
            <h1 className="text-2xl font-bold mb-2">{campaign.title}</h1>
            <div className="flex items-center text-sm text-gray-500 mb-4">
              <span className="mr-4">
                <span className="font-medium">{t('campaigns.by')}: </span>
                {campaign.organizationName || 'Organization'}
              </span>
              <span>
                <span className="font-medium">{t('campaigns.category')}: </span>
                {campaign.category}
              </span>
            </div>
            
            {/* Progress Bar */}
            <div className="mb-4">
              <div className="flex justify-between text-sm mb-1">
                <span className="font-medium">{t('campaigns.raised', { amount: campaign.amountRaised })}</span>
                <span>{progressPercentage}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2.5 mb-1">
                <div 
                  className="bg-ios-black h-2.5 rounded-full" 
                  style={{ width: `${progressPercentage}%` }}
                ></div>
              </div>
              <div className="flex justify-between text-sm text-gray-500">
                <div>
                  <span className="font-medium">{t('campaigns.goal')}: </span>
                  {campaign.goal}
                </div>
                <div>
                  <span className="font-medium">{t('campaigns.daysLeft')}: </span>
                  {daysRemaining}
                </div>
              </div>
            </div>
            
            {/* Share Buttons */}
            <div className="flex space-x-2">
              <button className="ios-button-secondary text-sm">
                <svg className="h-4 w-4 mr-1 inline" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
                </svg>
                {t('campaigns.share')}
              </button>
              <button className="ios-button-secondary text-sm">
                <svg className="h-4 w-4 mr-1 inline" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 5a2 2 0 012-2h10a2 2 0 012 2v16l-7-3.5L5 21V5z" />
                </svg>
                {t('campaigns.save')}
              </button>
            </div>
          </div>
          
          {/* Tabs */}
          <div className="border-b border-gray-100">
            <nav className="flex">
              <button
                onClick={() => setActiveTab('about')}
                className={`px-4 py-3 text-sm font-medium ${
                  activeTab === 'about'
                    ? 'border-b-2 border-ios-black text-ios-black'
                    : 'text-gray-500 hover:text-gray-700'
                }`}
              >
                {t('campaigns.tabs.about')}
              </button>
              <button
                onClick={() => setActiveTab('updates')}
                className={`px-4 py-3 text-sm font-medium ${
                  activeTab === 'updates'
                    ? 'border-b-2 border-ios-black text-ios-black'
                    : 'text-gray-500 hover:text-gray-700'
                }`}
              >
                {t('campaigns.tabs.updates')}
                {campaign.updates && campaign.updates.length > 0 && (
                  <span className="ml-1 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100">
                    {campaign.updates.length}
                  </span>
                )}
              </button>
              <button
                onClick={() => setActiveTab('milestones')}
                className={`px-4 py-3 text-sm font-medium ${
                  activeTab === 'milestones'
                    ? 'border-b-2 border-ios-black text-ios-black'
                    : 'text-gray-500 hover:text-gray-700'
                }`}
              >
                {t('campaigns.tabs.milestones')}
              </button>
            </nav>
          </div>
          
          {/* Tab Content */}
          <div className="p-6">
            {activeTab === 'about' && (
              <div className="prose max-w-none">
                <p className="whitespace-pre-line">{campaign.description}</p>
                
                {/* Additional Images */}
                {campaign.additionalImagesUrls && campaign.additionalImagesUrls.length > 0 && (
                  <div className="mt-6 grid grid-cols-2 gap-4">
                    {campaign.additionalImagesUrls.map((url, index) => (
                      <div key={index} className="rounded-lg overflow-hidden">
                        <img 
                          src={url} 
                          alt={`${campaign.title} - Image ${index + 1}`} 
                          className="w-full h-48 object-cover"
                        />
                      </div>
                    ))}
                  </div>
                )}
                
                {/* Video */}
                {campaign.videoUrl && (
                  <div className="mt-6">
                    <h3 className="text-lg font-semibold mb-2">{t('campaigns.watchVideo')}</h3>
                    <div className="aspect-w-16 aspect-h-9 rounded-lg overflow-hidden">
                      <iframe
                        src={campaign.videoUrl}
                        title={campaign.title}
                        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                        allowFullScreen
                        className="w-full h-full"
                      ></iframe>
                    </div>
                  </div>
                )}
              </div>
            )}
            
            {activeTab === 'updates' && (
              <div>
                <h3 className="text-lg font-semibold mb-4">{t('campaigns.latestUpdates')}</h3>
                
                {campaign.updates && campaign.updates.length > 0 ? (
                  <div className="space-y-6">
                    {campaign.updates.map((update) => (
                      <div key={update.id} className="border-b border-gray-100 pb-6 last:border-b-0 last:pb-0">
                        <div className="flex justify-between items-start mb-2">
                          <h4 className="text-md font-medium">{update.title}</h4>
                          <span className="text-xs text-gray-500">{formatDate(update.postedAt)}</span>
                        </div>
                        <p className="text-gray-600 text-sm whitespace-pre-line">{update.content}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    {t('campaigns.noUpdates')}
                  </div>
                )}
              </div>
            )}
            
            {activeTab === 'milestones' && (
              <div>
                <h3 className="text-lg font-semibold mb-4">{t('campaigns.milestones')}</h3>
                
                {campaign.milestones && campaign.milestones.length > 0 ? (
                  <div className="space-y-6">
                    {campaign.milestones.map((milestone) => (
                      <div key={milestone.id} className="relative pl-8 pb-6">
                        {/* Milestone dot and line */}
                        <div className="absolute left-0 top-0 h-full">
                          <div className={`w-4 h-4 rounded-full ${
                            milestone.isCompleted ? 'bg-green-500' : 'bg-gray-300'
                          } z-10`}></div>
                          <div className="absolute top-4 bottom-0 left-2 -ml-px w-0.5 bg-gray-200"></div>
                        </div>
                        
                        <div>
                          <h4 className="text-md font-medium flex items-center">
                            {milestone.title}
                            {milestone.isCompleted && (
                              <svg className="ml-2 h-5 w-5 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                              </svg>
                            )}
                          </h4>
                          <p className="text-gray-600 text-sm mt-1">{milestone.description}</p>
                          <div className="text-sm text-gray-500 mt-1">
                            <span className="font-medium">{t('campaigns.target')}: </span>
                            {milestone.targetAmount}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    {t('campaigns.noMilestones')}
                  </div>
                )}
              </div>
            )}
          </div>
        </motion.div>
      </div>
      
      {/* Sidebar */}
      <div>
        <DonationForm 
          campaignId={campaign.id} 
          campaignTitle={campaign.title}
        />
        
        {/* Organization Info */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3, delay: 0.1 }}
          className="ios-card p-6 mt-6"
        >
          <h3 className="text-lg font-semibold mb-4">{t('campaigns.organizedBy')}</h3>
          <div className="flex items-center">
            <div className="h-12 w-12 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-semibold">
              {campaign.organizationName?.charAt(0) || 'O'}
            </div>
            <div className="ml-3">
              <p className="font-medium">{campaign.organizationName || 'Organization'}</p>
              <p className="text-sm text-gray-500">{t('campaigns.verifiedOrganization')}</p>
            </div>
          </div>
          <div className="mt-4">
            <a href={`/organizations/${campaign.organizationId}`} className="ios-button-secondary w-full text-center text-sm">
              {t('campaigns.viewOrganization')}
            </a>
          </div>
        </motion.div>
        
        {/* Campaign Stats */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3, delay: 0.2 }}
          className="ios-card p-6 mt-6"
        >
          <h3 className="text-lg font-semibold mb-4">{t('campaigns.campaignStats')}</h3>
          <div className="space-y-3">
            <div className="flex justify-between">
              <span className="text-gray-600">{t('campaigns.startDate')}</span>
              <span className="font-medium">{new Date(campaign.startDate).toLocaleDateString()}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">{t('campaigns.endDate')}</span>
              <span className="font-medium">{new Date(campaign.endDate).toLocaleDateString()}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">{t('campaigns.daysLeft')}</span>
              <span className="font-medium">{daysRemaining}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">{t('campaigns.donors')}</span>
              <span className="font-medium">42</span> {/* This would be dynamic in a real implementation */}
            </div>
          </div>
        </motion.div>
      </div>
    </div>
  );
};