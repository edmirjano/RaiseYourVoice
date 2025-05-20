import React from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { formatDistanceToNow } from 'date-fns';
import { Post } from '../../services/postService';
import { ImageOptimizer } from '../common/ImageOptimizer/ImageOptimizer';
import { Badge } from '../common/Badge/Badge';

interface OpportunityCardProps {
  opportunity: Post;
  className?: string;
}

export const OpportunityCard: React.FC<OpportunityCardProps> = ({ 
  opportunity,
  className = ''
}) => {
  const { t } = useTranslation('common');
  
  const formatDate = (dateString?: string) => {
    if (!dateString) return '';
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  // Get category from tags
  const getCategory = () => {
    if (!opportunity.tags || opportunity.tags.length === 0) return null;
    
    const categories = ['funding', 'events', 'volunteer', 'mun', 'mobility'];
    const foundCategory = opportunity.tags.find(tag => categories.includes(tag.toLowerCase()));
    
    return foundCategory ? (
      <Badge variant="primary" className="absolute top-3 left-3">
        {t(`opportunities.categories.${foundCategory.toLowerCase()}`)}
      </Badge>
    ) : null;
  };
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className={`ios-card h-full flex flex-col overflow-hidden ${className}`}
    >
      {/* Opportunity Image */}
      <div className="relative h-48">
        {opportunity.mediaUrls && opportunity.mediaUrls.length > 0 ? (
          <ImageOptimizer 
            src={opportunity.mediaUrls[0]} 
            alt={opportunity.title} 
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full bg-gray-200 flex items-center justify-center">
            <svg className="h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        )}
        
        {/* Category Badge */}
        {getCategory()}
      </div>
      
      {/* Opportunity Content */}
      <div className="p-4 flex-1 flex flex-col">
        <Link href={`/opportunities/${opportunity.id}`}>
          <h3 className="text-lg font-semibold mb-2 hover:underline">{opportunity.title}</h3>
        </Link>
        
        <p className="text-gray-600 mb-4 flex-1 line-clamp-3">
          {opportunity.content}
        </p>
        
        {/* Opportunity Details */}
        <div className="mt-auto space-y-2 text-sm">
          {opportunity.location && (
            <div className="flex items-center text-gray-500">
              <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span>
                {opportunity.location.city}, {opportunity.location.country}
              </span>
            </div>
          )}
          
          {opportunity.eventDate && (
            <div className="flex items-center text-gray-500">
              <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>
                {new Date(opportunity.eventDate).toLocaleDateString()} {new Date(opportunity.eventDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </span>
            </div>
          )}
          
          {/* Tags */}
          {opportunity.tags && opportunity.tags.length > 0 && (
            <div className="flex flex-wrap gap-1 mt-2">
              {opportunity.tags.map((tag, index) => (
                <span 
                  key={index} 
                  className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                >
                  #{tag}
                </span>
              ))}
            </div>
          )}
        </div>
        
        {/* View Details Button */}
        <Link 
          href={`/opportunities/${opportunity.id}`}
          className="mt-4 ios-button-secondary text-center text-sm"
        >
          {t('opportunities.viewDetails')}
        </Link>
      </div>
    </motion.div>
  );
};