import React from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { Post } from '../../services/postService';

interface OpportunityCardProps {
  opportunity: Post;
}

export const OpportunityCard: React.FC<OpportunityCardProps> = ({ opportunity }) => {
  const { t } = useTranslation('common');
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="ios-card h-full flex flex-col"
    >
      {/* Opportunity Image */}
      {opportunity.mediaUrls && opportunity.mediaUrls.length > 0 ? (
        <div className="relative h-48">
          <img 
            src={opportunity.mediaUrls[0]} 
            alt={opportunity.title} 
            className="w-full h-full object-cover rounded-t-xl"
          />
        </div>
      ) : (
        <div className="h-48 bg-gray-200 rounded-t-xl flex items-center justify-center">
          <svg className="h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
        </div>
      )}
      
      {/* Opportunity Content */}
      <div className="p-4 flex-1 flex flex-col">
        <Link href={`/opportunities/${opportunity.id}`}>
          <h3 className="text-lg font-semibold mb-2 hover:underline">{opportunity.title}</h3>
        </Link>
        
        <p className="text-gray-600 mb-4 flex-1">
          {opportunity.content.length > 100
            ? `${opportunity.content.substring(0, 100)}...`
            : opportunity.content}
        </p>
        
        {/* Opportunity Details */}
        <div className="mt-auto">
          {/* Location */}
          {opportunity.location && (
            <div className="flex items-center text-sm text-gray-500 mb-2">
              <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span>
                {opportunity.location.city}, {opportunity.location.country}
              </span>
            </div>
          )}
          
          {/* Date */}
          {opportunity.eventDate && (
            <div className="flex items-center text-sm text-gray-500 mb-2">
              <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>
                {new Date(opportunity.eventDate).toLocaleDateString()}
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