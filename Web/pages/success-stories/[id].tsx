import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../../components/MainLayout';
import { CommentSection } from '../../components/feed/CommentSection';
import { getPostById, likePost, Post } from '../../services/postService';
import { formatDistanceToNow } from 'date-fns';

const SuccessStoryDetailPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { id } = router.query;
  
  const [story, setStory] = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isLiked, setIsLiked] = useState(false);
  const [likeCount, setLikeCount] = useState(0);
  
  useEffect(() => {
    const fetchStory = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const data = await getPostById(id as string);
        
        // Verify this is a success story post
        if (data.postType !== 'SuccessStory') {
          router.push('/success-stories');
          return;
        }
        
        setStory(data);
        setLikeCount(data.likeCount || 0);
        setError('');
      } catch (err) {
        console.error('Failed to fetch success story:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchStory();
  }, [id, router, t]);
  
  const handleLike = async () => {
    if (!story?.id) return;
    
    try {
      await likePost(story.id);
      setIsLiked(!isLiked);
      setLikeCount(isLiked ? likeCount - 1 : likeCount + 1);
    } catch (err) {
      console.error('Failed to like story:', err);
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
  
  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
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
        ) : story ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            {/* Story Header */}
            <div className="ios-card mb-6 overflow-hidden">
              {/* Cover Image */}
              {story.mediaUrls && story.mediaUrls.length > 0 && (
                <div className="relative h-64 md:h-96">
                  <img 
                    src={story.mediaUrls[0]} 
                    alt={story.title} 
                    className="w-full h-full object-cover"
                  />
                </div>
              )}
              
              <div className="p-6">
                <div className="flex items-center mb-4">
                  <div className="h-10 w-10 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-semibold">
                    {story.authorId?.charAt(0) || 'U'}
                  </div>
                  <div className="ml-3">
                    <p className="text-sm font-medium text-gray-900">{story.authorId}</p>
                    <p className="text-xs text-gray-500">{formatDate(story.createdAt)}</p>
                  </div>
                </div>
                
                <h1 className="text-3xl font-bold mb-4">{story.title}</h1>
                
                {/* Tags */}
                {story.tags && story.tags.length > 0 && (
                  <div className="flex flex-wrap gap-2 mb-6">
                    {story.tags.map((tag, index) => (
                      <span 
                        key={index} 
                        className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                      >
                        #{tag}
                      </span>
                    ))}
                  </div>
                )}
                
                {/* Story Content */}
                <div className="prose max-w-none mb-6">
                  <p className="whitespace-pre-line">{story.content}</p>
                </div>
                
                {/* Story Footer */}
                <div className="flex justify-between items-center pt-4 border-t border-gray-100">
                  <div className="flex space-x-6">
                    <button 
                      onClick={handleLike}
                      className={`flex items-center ${isLiked ? 'text-ios-black font-medium' : 'text-gray-500'}`}
                    >
                      <svg className="h-6 w-6 mr-2" fill={isLiked ? 'currentColor' : 'none'} viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={isLiked ? 0 : 2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                      </svg>
                      {likeCount} {t('feed.likes')}
                    </button>
                    
                    <div className="flex items-center text-gray-500">
                      <svg className="h-6 w-6 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                      </svg>
                      {story.commentCount || 0} {t('feed.comments')}
                    </div>
                  </div>
                  
                  <button className="text-gray-500">
                    <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
            
            {/* Comments Section */}
            <div className="ios-card p-6">
              <CommentSection postId={story.id!} />
            </div>
          </motion.div>
        ) : (
          <div className="ios-card p-6 text-center">
            <p className="text-gray-600 mb-4">{t('errors.storyNotFound')}</p>
            <button
              onClick={() => router.back()}
              className="ios-button-secondary"
            >
              {t('common.goBack')}
            </button>
          </div>
        )}
      </div>
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

export default SuccessStoryDetailPage;