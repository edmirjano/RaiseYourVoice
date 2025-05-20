import React, { useState } from 'react';
import Link from 'next/link';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { formatDistanceToNow } from 'date-fns';
import { Post } from '../../services/postService';
import { Avatar } from '../common/Avatar/Avatar';
import { Badge } from '../common/Badge/Badge';
import { ImageOptimizer } from '../common/ImageOptimizer/ImageOptimizer';
import { VideoPlayer } from '../common/VideoPlayer/VideoPlayer';

interface PostCardProps {
  post: Post;
  onLike?: () => void;
  onComment?: () => void;
  isLiked?: boolean;
  className?: string;
}

export const PostCard: React.FC<PostCardProps> = ({ 
  post, 
  onLike, 
  onComment, 
  isLiked = false,
  className = '' 
}) => {
  const { t } = useTranslation('common');
  const [likeCount, setLikeCount] = useState(post.likeCount || 0);
  
  const handleLike = () => {
    if (onLike) {
      onLike();
      setLikeCount(isLiked ? likeCount - 1 : likeCount + 1);
    }
  };
  
  const handleComment = () => {
    if (onComment) {
      onComment();
    }
  };
  
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
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className={`ios-card overflow-hidden ${className}`}
    >
      {/* Post Header */}
      <div className="p-4 border-b border-gray-100">
        <div className="flex items-center">
          <Avatar 
            src={post.authorProfilePicUrl} 
            name={post.authorName || 'User'} 
            size="md" 
          />
          <div className="ml-3">
            <p className="text-sm font-medium text-gray-900">{post.authorName || 'Anonymous'}</p>
            <p className="text-xs text-gray-500">{formatDate(post.createdAt)}</p>
          </div>
          
          {/* Post Type Badge */}
          <div className="ml-auto">
            <Badge 
              variant={
                post.postType === 'Activism' ? 'primary' : 
                post.postType === 'Opportunity' ? 'success' : 
                'secondary'
              }
            >
              {t(`feed.postTypes.${post.postType.toLowerCase()}`)}
            </Badge>
          </div>
        </div>
      </div>
      
      {/* Post Content */}
      <div className="p-4">
        <Link href={`/posts/${post.id}`}>
          <h3 className="text-lg font-semibold mb-2 hover:underline">{post.title}</h3>
        </Link>
        <p className="text-gray-700 mb-4 whitespace-pre-line line-clamp-3">{post.content}</p>
        
        {/* Post Media */}
        {post.mediaUrls && post.mediaUrls.length > 0 && (
          <div className="mb-4 rounded-lg overflow-hidden">
            {isVideo(post.mediaUrls[0]) ? (
              <VideoPlayer 
                src={post.mediaUrls[0]} 
                poster={post.mediaUrls.length > 1 ? post.mediaUrls[1] : undefined}
                controls
                className="w-full h-64 object-cover"
              />
            ) : (
              <ImageOptimizer 
                src={post.mediaUrls[0]} 
                alt={post.title} 
                className="w-full h-64 object-cover"
              />
            )}
          </div>
        )}
        
        {/* Post Tags */}
        {post.tags && post.tags.length > 0 && (
          <div className="flex flex-wrap gap-2 mb-4">
            {post.tags.map((tag, index) => (
              <span 
                key={index} 
                className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
              >
                #{tag}
              </span>
            ))}
          </div>
        )}
        
        {/* Post Location & Date */}
        {(post.location || post.eventDate) && (
          <div className="flex flex-col space-y-1 mb-4 text-sm text-gray-500">
            {post.location && (
              <div className="flex items-center">
                <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span>
                  {post.location.city}, {post.location.country}
                </span>
              </div>
            )}
            
            {post.eventDate && (
              <div className="flex items-center">
                <svg className="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span>
                  {new Date(post.eventDate).toLocaleDateString()} {new Date(post.eventDate).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
              </div>
            )}
          </div>
        )}
      </div>
      
      {/* Post Footer */}
      <div className="px-4 py-3 border-t border-gray-100 flex justify-between">
        <div className="flex space-x-4">
          <button 
            onClick={handleLike}
            className={`flex items-center text-sm ${isLiked ? 'text-ios-black font-medium' : 'text-gray-500'}`}
          >
            <svg className="h-5 w-5 mr-1" fill={isLiked ? 'currentColor' : 'none'} viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={isLiked ? 0 : 2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
            </svg>
            {likeCount} {t('feed.likes')}
          </button>
          
          <button 
            onClick={handleComment}
            className="flex items-center text-sm text-gray-500"
          >
            <svg className="h-5 w-5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
            {post.commentCount || 0} {t('feed.comments')}
          </button>
        </div>
        
        <button className="text-sm text-gray-500">
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
          </svg>
        </button>
      </div>
    </motion.div>
  );
};