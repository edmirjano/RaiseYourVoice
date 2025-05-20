import React from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { Post } from '../../services/postService';
import { formatDistanceToNow } from 'date-fns';

interface SuccessStoryCardProps {
  story: Post;
}

export const SuccessStoryCard: React.FC<SuccessStoryCardProps> = ({ story }) => {
  const { t } = useTranslation('common');
  
  const formatDate = (dateString?: string) => {
    if (!dateString) return '';
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  return (
    <motion.article
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="ios-card overflow-hidden"
    >
      <div className="flex flex-col md:flex-row">
        {/* Story Image */}
        {story.mediaUrls && story.mediaUrls.length > 0 ? (
          <div className="md:w-1/3">
            <img 
              src={story.mediaUrls[0]} 
              alt={story.title} 
              className="w-full h-48 md:h-full object-cover"
            />
          </div>
        ) : null}
        
        {/* Story Content */}
        <div className={`p-6 ${story.mediaUrls && story.mediaUrls.length > 0 ? 'md:w-2/3' : 'w-full'}`}>
          <div className="flex items-center mb-3">
            <div className="h-8 w-8 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-semibold">
              {story.authorId?.charAt(0) || 'U'}
            </div>
            <div className="ml-2">
              <p className="text-sm font-medium text-gray-900">{story.authorId}</p>
              <p className="text-xs text-gray-500">{formatDate(story.createdAt)}</p>
            </div>
          </div>
          
          <Link href={`/success-stories/${story.id}`}>
            <h3 className="text-xl font-bold mb-3 hover:underline">{story.title}</h3>
          </Link>
          
          <p className="text-gray-600 mb-4">
            {story.content.length > 200
              ? `${story.content.substring(0, 200)}...`
              : story.content}
          </p>
          
          {/* Tags */}
          {story.tags && story.tags.length > 0 && (
            <div className="flex flex-wrap gap-1 mb-4">
              {story.tags.map((tag, index) => (
                <span 
                  key={index} 
                  className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                >
                  #{tag}
                </span>
              ))}
            </div>
          )}
          
          <div className="flex justify-between items-center">
            <div className="flex space-x-4 text-sm text-gray-500">
              <span className="flex items-center">
                <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                </svg>
                {story.likeCount || 0}
              </span>
              <span className="flex items-center">
                <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
                {story.commentCount || 0}
              </span>
            </div>
            
            <Link 
              href={`/success-stories/${story.id}`} 
              className="text-ios-black font-medium text-sm hover:underline"
            >
              {t('successStories.readMore')} →
            </Link>
          </div>
        </div>
      </div>
    </motion.article>
  );
};