import React from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { ImageOptimizer } from '../common/ImageOptimizer/ImageOptimizer';
import { Button } from '../common/Button/Button';
import { Badge } from '../common/Badge/Badge';
import { SocialShareButtons } from './SocialShareButtons';

export type Activist = {
  id: string;
  name: string;
  profilePicture?: string;
  bio: string;
  achievements: string[];
  causes: string[];
  impactMetrics?: {
    peopleHelped?: number;
    projectsCompleted?: number;
    volunteersCoordinated?: number;
    [key: string]: number | undefined;
  };
  socialLinks?: {
    twitter?: string;
    facebook?: string;
    instagram?: string;
    linkedin?: string;
    website?: string;
  };
  featuredStoryId?: string;
  featuredStoryTitle?: string;
};

interface ActivistOfTheMonthCardProps {
  activist: Activist;
  className?: string;
}

export const ActivistOfTheMonthCard: React.FC<ActivistOfTheMonthCardProps> = ({
  activist,
  className = '',
}) => {
  const { t } = useTranslation('common');
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className={`ios-card overflow-hidden ${className}`}
    >
      <div className="p-6 border-b border-gray-100">
        <h2 className="text-xl font-semibold flex items-center">
          <svg className="h-5 w-5 text-yellow-500 mr-2" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm1-13a1 1 0 10-2 0v.092a4.535 4.535 0 00-1.676.662C6.602 6.234 6 7.009 6 8c0 .99.602 1.765 1.324 2.246.48.32 1.054.545 1.676.662v1.941c-.391-.127-.68-.317-.843-.504a1 1 0 10-1.51 1.31c.562.649 1.413 1.076 2.353 1.253V15a1 1 0 102 0v-.092a4.535 4.535 0 001.676-.662C13.398 13.766 14 12.991 14 12c0-.99-.602-1.765-1.324-2.246A4.535 4.535 0 0011 9.092V7.151c.391.127.68.317.843.504a1 1 0 101.511-1.31c-.563-.649-1.413-1.076-2.354-1.253V5z" clipRule="evenodd" />
          </svg>
          {t('successStories.featuredActivist')}
        </h2>
      </div>
      
      <div className="flex flex-col md:flex-row">
        {/* Activist Image */}
        <div className="md:w-1/3">
          <div className="aspect-w-1 aspect-h-1">
            <ImageOptimizer 
              src={activist.profilePicture || 'https://images.pexels.com/photos/7148384/pexels-photo-7148384.jpeg'} 
              alt={activist.name} 
              className="w-full h-full object-cover"
            />
          </div>
        </div>
        
        {/* Activist Info */}
        <div className="p-6 md:w-2/3">
          <h3 className="text-2xl font-bold mb-2">{activist.name}</h3>
          
          {/* Causes */}
          <div className="flex flex-wrap gap-2 mb-4">
            {activist.causes.map((cause, index) => (
              <Badge key={index} variant="primary">
                {cause}
              </Badge>
            ))}
          </div>
          
          {/* Bio */}
          <p className="text-gray-700 mb-4">{activist.bio}</p>
          
          {/* Achievements */}
          <div className="mb-4">
            <h4 className="text-lg font-semibold mb-2">{t('successStories.keyAchievements')}</h4>
            <ul className="list-disc list-inside space-y-1 text-gray-700">
              {activist.achievements.map((achievement, index) => (
                <li key={index}>{achievement}</li>
              ))}
            </ul>
          </div>
          
          {/* Impact Metrics */}
          {activist.impactMetrics && (
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-6">
              {Object.entries(activist.impactMetrics).map(([key, value]) => (
                <div key={key} className="text-center p-3 bg-gray-50 rounded-lg">
                  <div className="text-2xl font-bold text-ios-black">{value}</div>
                  <div className="text-sm text-gray-600">{t(`successStories.impactMetrics.${key}`)}</div>
                </div>
              ))}
            </div>
          )}
          
          {/* Featured Story Link */}
          {activist.featuredStoryId && (
            <div className="mb-4">
              <Link 
                href={`/success-stories/${activist.featuredStoryId}`}
                className="ios-button inline-block"
              >
                {t('successStories.readFeaturedStory')}
              </Link>
            </div>
          )}
          
          {/* Social Share */}
          <div className="mt-4">
            <SocialShareButtons 
              url={`${typeof window !== 'undefined' ? window.location.origin : ''}/activists/${activist.id}`}
              title={`${t('successStories.featuredActivist')}: ${activist.name}`}
              description={activist.bio}
            />
          </div>
        </div>
      </div>
    </motion.div>
  );
};