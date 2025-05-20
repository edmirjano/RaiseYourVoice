import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { formatDistanceToNow } from 'date-fns';
import { Post } from '../../services/postService';
import { ImageOptimizer } from '../common/ImageOptimizer/ImageOptimizer';
import { VideoPlayer } from '../common/VideoPlayer/VideoPlayer';
import { Badge } from '../common/Badge/Badge';
import { Button } from '../common/Button/Button';
import { OpportunityRegistration } from './OpportunityRegistration';
import { MapView } from './MapView';

interface OpportunityDetailsProps {
  opportunity: Post;
  className?: string;
}

export const OpportunityDetails: React.FC<OpportunityDetailsProps> = ({
  opportunity,
  className = '',
}) => {
  const { t } = useTranslation('common');
  const [showRegistration, setShowRegistration] = useState(false);
  
  const formatDate = (dateString?: string) => {
    if (!dateString) return '';
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  // Determine if media is video or image
  const isVideo = (url: string) => {
    return url.match(/\.(mp4|webm|ogg|mov)($|\?)/i);
  };
  
  // Get category from tags
  const getCategory = () => {
    if (!opportunity.tags || opportunity.tags.length === 0) return null;
    
    const categories = ['funding', 'events', 'volunteer', 'mun', 'mobility'];
    const foundCategory = opportunity.tags.find(tag => categories.includes(tag.toLowerCase()));
    
    return foundCategory ? (
      <Badge variant="primary">
        {t(`opportunities.categories.${foundCategory.toLowerCase()}`)}
      </Badge>
    ) : null;
  };
  
  // Add to calendar function
  const addToCalendar = () => {
    if (!opportunity.eventDate) return;
    
    const eventDate = new Date(opportunity.eventDate);
    const endDate = new Date(eventDate);
    endDate.setHours(endDate.getHours() + 2); // Default 2 hour duration
    
    const title = encodeURIComponent(opportunity.title);
    const details = encodeURIComponent(opportunity.content);
    const location = encodeURIComponent(
      `${opportunity.location?.address || ''}, ${opportunity.location?.city || ''}, ${opportunity.location?.country || ''}`
    );
    
    // Format dates for Google Calendar
    const startDate = eventDate.toISOString().replace(/-|:|\.\d+/g, '');
    const endDateStr = endDate.toISOString().replace(/-|:|\.\d+/g, '');
    
    // Create Google Calendar URL
    const googleCalendarUrl = `https://calendar.google.com/calendar/render?action=TEMPLATE&text=${title}&dates=${startDate}/${endDateStr}&details=${details}&location=${location}`;
    
    // Open in new tab
    window.open(googleCalendarUrl, '_blank');
  };
  
  // Share opportunity
  const shareOpportunity = async () => {
    if (navigator.share) {
      try {
        await navigator.share({
          title: opportunity.title,
          text: opportunity.content,
          url: window.location.href,
        });
      } catch (error) {
        console.error('Error sharing:', error);
      }
    } else {
      // Fallback to copy to clipboard
      navigator.clipboard.writeText(window.location.href);
      alert(t('opportunities.copiedToClipboard'));
    }
  };
  
  return (
    <div className={`space-y-6 ${className}`}>
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
        className="ios-card overflow-hidden"
      >
        {/* Opportunity Media */}
        {opportunity.mediaUrls && opportunity.mediaUrls.length > 0 && (
          <div className="relative h-64 md:h-96">
            {isVideo(opportunity.mediaUrls[0]) ? (
              <VideoPlayer 
                src={opportunity.mediaUrls[0]} 
                poster={opportunity.mediaUrls.length > 1 ? opportunity.mediaUrls[1] : undefined}
                controls
                className="w-full h-full object-cover"
              />
            ) : (
              <ImageOptimizer 
                src={opportunity.mediaUrls[0]} 
                alt={opportunity.title} 
                className="w-full h-full object-cover"
              />
            )}
          </div>
        )}
        
        {/* Opportunity Header */}
        <div className="p-6 border-b border-gray-100">
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <div className="flex items-center gap-2 mb-2">
                {getCategory()}
                <span className="text-sm text-gray-500">{formatDate(opportunity.createdAt)}</span>
              </div>
              <h1 className="text-2xl font-bold">{opportunity.title}</h1>
            </div>
            
            <div className="flex flex-wrap gap-2">
              <Button
                variant="secondary"
                size="sm"
                onClick={addToCalendar}
                disabled={!opportunity.eventDate}
              >
                <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                {t('opportunities.saveToCalendar')}
              </Button>
              
              <Button
                variant="secondary"
                size="sm"
                onClick={shareOpportunity}
              >
                <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
                </svg>
                {t('opportunities.share')}
              </Button>
            </div>
          </div>
          
          {/* Key Details */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6 p-4 bg-gray-50 rounded-lg">
            {opportunity.location && (
              <div className="flex items-start">
                <svg className="h-5 w-5 mr-2 text-gray-500 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <div>
                  <div className="text-sm font-medium text-gray-900">{t('opportunities.location')}</div>
                  <div className="text-sm text-gray-600">
                    {opportunity.location.address || `${opportunity.location.city}, ${opportunity.location.country}`}
                  </div>
                </div>
              </div>
            )}
            
            {opportunity.eventDate && (
              <div className="flex items-start">
                <svg className="h-5 w-5 mr-2 text-gray-500 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <div>
                  <div className="text-sm font-medium text-gray-900">{t('opportunities.date')}</div>
                  <div className="text-sm text-gray-600">
                    {new Date(opportunity.eventDate).toLocaleDateString()} {new Date(opportunity.eventDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
        
        {/* Opportunity Content */}
        <div className="p-6">
          <div className="prose max-w-none mb-6">
            <h2 className="text-xl font-semibold mb-4">{t('opportunities.description')}</h2>
            <div className="whitespace-pre-line">{opportunity.content}</div>
          </div>
          
          {/* Tags */}
          {opportunity.tags && opportunity.tags.length > 0 && (
            <div className="mb-6">
              <h3 className="text-lg font-semibold mb-2">{t('opportunities.tags')}</h3>
              <div className="flex flex-wrap gap-2">
                {opportunity.tags.map((tag, index) => (
                  <Badge 
                    key={index} 
                    variant="secondary"
                  >
                    #{tag}
                  </Badge>
                ))}
              </div>
            </div>
          )}
          
          {/* Register Button */}
          <div className="mt-6 flex justify-center">
            <Button
              size="lg"
              onClick={() => setShowRegistration(!showRegistration)}
            >
              {showRegistration ? t('opportunities.hideRegistration') : t('opportunities.register')}
            </Button>
          </div>
        </div>
      </motion.div>
      
      {/* Registration Form */}
      {showRegistration && (
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <OpportunityRegistration 
            opportunityId={opportunity.id!}
            opportunityTitle={opportunity.title}
          />
        </motion.div>
      )}
      
      {/* Map */}
      {opportunity.location?.latitude && opportunity.location?.longitude && (
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3, delay: 0.1 }}
          className="ios-card p-6"
        >
          <h2 className="text-xl font-semibold mb-4">{t('opportunities.locationMap')}</h2>
          <MapView 
            opportunities={[opportunity]} 
            height="400px"
          />
        </motion.div>
      )}
    </div>
  );
};