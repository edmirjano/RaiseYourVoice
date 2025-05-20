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

const PostDetailPage: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { id } = router.query;
  
  const [post, setPost] = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isLiked, setIsLiked] = useState(false);
  const [likeCount, setLikeCount] = useState(0);
  
  useEffect(() => {
    const fetchPost = async () => {
      if (!id) return;
      
      try {
        setLoading(true);
        const data = await getPostById(id as string);
        setPost(data);
        setLikeCount(data.likeCount || 0);
        setError('');
      } catch (err) {
        console.error('Failed to fetch post:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchPost();
  }, [id, t]);
  
  const handleLike = async () => {
    if (!post?.id) return;
    
    try {
      await likePost(post.id);
      setIsLiked(!isLiked);
      setLikeCount(isLiked ? likeCount - 1 : likeCount + 1);
    } catch (err) {
      console.error('Failed to like post:', err);
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
      <div className="max-w-3xl mx-auto">
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
        ) : post ? (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            <div className="ios-card overflow-hidden mb-6">
              {/* Post Header */}
              <div className="p-6 border-b border-gray-100">
                <div className="flex items-center mb-4">
                  <div className="h-12 w-12 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-semibold">
                    {post.authorId?.charAt(0) || 'U'}
                  </div>
                  <div className="ml-4">
                    <p className="font-medium text-gray-900">{post.authorId}</p>
                    <p className="text-sm text-gray-500">{formatDate(post.createdAt)}</p>
                  </div>
                </div>
                <h1 className="text-2xl font-bold mb-2">{post.title}</h1>
                
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
              </div>
              
              {/* Post Media */}
              {post.mediaUrls && post.mediaUrls.length > 0 && (
                <div className="border-b border-gray-100">
                  <img 
                    src={post.mediaUrls[0]} 
                    alt={post.title} 
                    className="w-full h-96 object-cover"
                  />
                </div>
              )}
              
              {/* Post Content */}
              <div className="p-6">
                <p className="text-gray-700 whitespace-pre-line">{post.content}</p>
                
                {/* Post Location & Date */}
                {(post.location || post.eventDate) && (
                  <div className="flex flex-col space-y-2 mt-6 text-sm text-gray-500">
                    {post.location && (
                      <div className="flex items-center">
                        <svg className="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
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
                        <svg className="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                        </svg>
                        <span>
                          {new Date(post.eventDate).toLocaleDateString()}
                        </span>
                      </div>
                    )}
                  </div>
                )}
              </div>
              
              {/* Post Footer */}
              <div className="px-6 py-4 border-t border-gray-100 flex justify-between">
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
                    {post.commentCount || 0} {t('feed.comments')}
                  </div>
                </div>
                
                <button className="text-gray-500">
                  <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.684 13.342C8.886 12.938 9 12.482 9 12c0-.482-.114-.938-.316-1.342m0 2.684a3 3 0 110-2.684m0 2.684l6.632 3.316m-6.632-6l6.632-3.316m0 0a3 3 0 105.367-2.684 3 3 0 00-5.367 2.684zm0 9.316a3 3 0 105.368 2.684 3 3 0 00-5.368-2.684z" />
                  </svg>
                </button>
              </div>
            </div>
            
            {/* Comments Section */}
            <div className="ios-card p-6">
              <CommentSection postId={post.id!} />
            </div>
          </motion.div>
        ) : (
          <div className="ios-card p-6 text-center">
            <p className="text-gray-600 mb-4">{t('errors.postNotFound')}</p>
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

export default PostDetailPage;