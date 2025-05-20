import React from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { Organization } from '../../services/organizationService';

interface OrganizationCardProps {
  organization: Organization;
}

export const OrganizationCard: React.FC<OrganizationCardProps> = ({ organization }) => {
  const { t } = useTranslation('common');
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="ios-card overflow-hidden h-full flex flex-col"
    >
      {/* Organization Logo */}
      <div className="p-6 flex justify-center border-b border-gray-100">
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
      
      {/* Organization Content */}
      <div className="p-6 flex-1 flex flex-col">
        <Link href={`/organizations/${organization.id}`}>
          <h3 className="text-lg font-semibold text-center mb-2 hover:underline">{organization.name}</h3>
        </Link>
        
        {organization.verificationStatus === 'Verified' && (
          <div className="flex justify-center mb-4">
            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
              <svg className="h-3 w-3 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
              {t('organizations.verified')}
            </span>
          </div>
        )}
        
        <p className="text-sm text-gray-600 mb-4 flex-1">
          {organization.description.length > 120
            ? `${organization.description.substring(0, 120)}...`
            : organization.description}
        </p>
        
        {/* Organization Details */}
        <div className="mt-auto space-y-2 text-sm">
          {organization.location && (
            <div className="flex items-center text-gray-500">
              <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              <span>
                {organization.location.city}, {organization.location.country}
              </span>
            </div>
          )}
          
          <div className="flex items-center text-gray-500">
            <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            </svg>
            <span>{organization.organizationType}</span>
          </div>
          
          {organization.website && (
            <div className="flex items-center text-gray-500">
              <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
              </svg>
              <a 
                href={organization.website} 
                target="_blank" 
                rel="noopener noreferrer"
                className="text-ios-black hover:underline"
              >
                {organization.website.replace(/^https?:\/\//, '')}
              </a>
            </div>
          )}
        </div>
        
        {/* View Details Button */}
        <Link 
          href={`/organizations/${organization.id}`}
          className="mt-4 ios-button w-full text-center text-sm"
        >
          {t('organizations.viewProfile')}
        </Link>
      </div>
    </motion.div>
  );
};