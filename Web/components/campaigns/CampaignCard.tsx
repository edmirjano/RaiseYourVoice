import React from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';

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
  category: string;
}

interface CampaignCardProps {
  campaign: Campaign;
}

export const CampaignCard: React.FC<CampaignCardProps> = ({ campaign }) => {
  const { t } = useTranslation('common');
  
  // Calculate progress percentage
  const progressPercentage = Math.min(100, Math.round((campaign.amountRaised / campaign.goal) * 100));
  
  // Calculate days remaining
  const daysRemaining = Math.max(0, Math.ceil((new Date(campaign.endDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24)));
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="ios-card overflow-hidden h-full flex flex-col"
    >
      {/* Campaign Image */}
      <div className="relative h-48">
        <img 
          src={campaign.coverImageUrl || 'https://images.pexels.com/photos/3184418/pexels-photo-3184418.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2'} 
          alt={campaign.title} 
          className="w-full h-full object-cover"
        />
        <div className="absolute top-3 right-3">
          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
            campaign.status === 'Active' ? 'bg-green-100 text-green-800' :
            campaign.status === 'Completed' ? 'bg-blue-100 text-blue-800' :
            campaign.status === 'Paused' ? 'bg-yellow-100 text-yellow-800' :
            'bg-gray-100 text-gray-800'
          }`}>
            {campaign.status}
          </span>
        </div>
      </div>
      
      {/* Campaign Content */}
      <div className="p-4 flex-1 flex flex-col">
        <Link href={`/campaigns/${campaign.id}`}>
          <h3 className="text-lg font-semibold mb-2 hover:underline">{campaign.title}</h3>
        </Link>
        
        <p className="text-sm text-gray-600 mb-4 flex-1">
          {campaign.description.length > 120
            ? `${campaign.description.substring(0, 120)}...`
            : campaign.description}
        </p>
        
        {/* Progress Bar */}
        <div className="mt-auto">
          <div className="flex justify-between text-sm mb-1">
            <span className="font-medium">{t('campaigns.raised', { amount: campaign.amountRaised })}</span>
            <span>{progressPercentage}%</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2.5 mb-4">
            <div 
              className="bg-ios-black h-2.5 rounded-full" 
              style={{ width: `${progressPercentage}%` }}
            ></div>
          </div>
          
          {/* Campaign Details */}
          <div className="flex justify-between text-sm text-gray-500 mb-4">
            <div>
              <span className="font-medium">{t('campaigns.goal')}: </span>
              {campaign.goal}
            </div>
            <div>
              <span className="font-medium">{t('campaigns.daysLeft')}: </span>
              {daysRemaining}
            </div>
          </div>
          
          {/* Organization */}
          <div className="text-sm text-gray-500 mb-4">
            <span className="font-medium">{t('campaigns.by')}: </span>
            {campaign.organizationName || 'Organization'}
          </div>
          
          {/* Action Button */}
          <Link 
            href={`/campaigns/${campaign.id}`}
            className="ios-button w-full text-center text-sm"
          >
            {t('campaigns.viewCampaign')}
          </Link>
        </div>
      </div>
    </motion.div>
  );
};