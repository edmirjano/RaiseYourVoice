import React, { useState, useEffect, useRef } from 'react';
import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion, AnimatePresence } from 'framer-motion';
import { PageLayout } from '../components/layout';
import { PostCard } from '../components/feed/PostCard';
import { PostForm } from '../components/feed/PostForm';
import { CommentSection } from '../components/feed/CommentSection';
import { getSocialFeed, likePost, unlikePost, Post } from '../services/postService';
import { useAuth } from '../contexts/AuthContext';
import { useInfiniteScroll } from '../hooks/useInfiniteScroll';
import { Loader } from '../components/common/Loader/Loader';
import { EmptyState } from '../components/common/EmptyState/EmptyState';
import { Alert } from '../components/common/Alert/Alert';
import { Button } from '../components/common/Button/Button';

const FeedPage: React.FC = () => {
  const { t } = useTranslation('common');
  const { isAuthenticated } = useAuth();
  const [selectedPostId, setSelectedPostId] = useState<string | null>(null);
  const [likedPosts, setLikedPosts] = useState<Record<string, boolean>>({});
  const [showPostForm, setShowPostForm] = useState(false);
  
  // Use the infinite scroll hook to load posts
  const fetchPosts = async (page: number) => {
    const data = await getSocialFeed(page);
    return data;
  };
  
  const {
    items: posts,
    loading,
    error,
    hasMore,
    lastItemRef,
    loadMore,
    reset
  } = useInfiniteScroll<Post>(fetchPosts);
  
  // Initialize liked posts state from the API response
  useEffect(() => {
    const initialLikedPosts: Record<string, boolean> = {};
    posts.forEach(post => {
      if (post.id && post.isLikedByCurrentUser) {
        initialLikedPosts[post.id] = true;
      }
    });
    setLikedPosts(initialLikedPosts);
  }, [posts]);
  
  const handlePostCreated = (post: Post) => {
    reset();
    setShowPostForm(false);
  };
  
  const handleLike = async (id: string) => {
    try {
      const isLiked = likedPosts[id];
      
      if (isLiked) {
        await unlikePost(id);
      } else {
        await likePost(id);
      }
      
      // Update local state
      setLikedPosts(prev => ({
        ...prev,
        [id]: !isLiked
      }));
    } catch (err) {
      console.error('Failed to like/unlike post:', err);
    }
  };
  
  const handleComment = (id: string) => {
    setSelectedPostId(id === selectedPostId ? null : id);
  };
  
  return (
    <PageLayout title={`${t('feed.title')} | RaiseYourVoice`}>
      <div className="max-w-2xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold">{t('feed.title')}</h1>
            
            {isAuthenticated && (
              <Button 
                onClick={() => setShowPostForm(!showPostForm)}
                variant="primary"
              >
                {showPostForm ? t('common.cancel') : t('feed.createPost')}
              </Button>
            )}
          </div>
          
          <AnimatePresence>
            {showPostForm && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                exit={{ opacity: 0, height: 0 }}
                transition={{ duration: 0.3 }}
              >
                <PostForm onPostCreated={handlePostCreated} />
              </motion.div>
            )}
          </AnimatePresence>
          
          {error && (
            <Alert 
              variant="error" 
              className="mb-6"
              onClose={() => reset()}
            >
              {error}
            </Alert>
          )}
          
          {!loading && posts.length === 0 ? (
            <EmptyState
              title={t('feed.noPosts.title')}
              description={t('feed.noPosts.description')}
              action={
                isAuthenticated ? (
                  <Button onClick={() => setShowPostForm(true)}>
                    {t('feed.createPost')}
                  </Button>
                ) : (
                  <Button onClick={() => window.location.href = '/auth/login'}>
                    {t('auth.login')}
                  </Button>
                )
              }
            />
          ) : (
            <div className="space-y-6">
              {posts.map((post, index) => {
                // Add ref to the last item for infinite scrolling
                const isLastItem = index === posts.length - 1;
                
                return (
                  <div key={post.id || index} ref={isLastItem ? lastItemRef : undefined}>
                    <PostCard 
                      post={post} 
                      onLike={() => post.id && handleLike(post.id)}
                      onComment={() => post.id && handleComment(post.id)}
                      isLiked={post.id ? likedPosts[post.id] : false}
                    />
                    
                    <AnimatePresence>
                      {selectedPostId === post.id && (
                        <motion.div
                          initial={{ opacity: 0, height: 0 }}
                          animate={{ opacity: 1, height: 'auto' }}
                          exit={{ opacity: 0, height: 0 }}
                          transition={{ duration: 0.3 }}
                          className="mt-2 ios-card p-4"
                        >
                          <CommentSection postId={post.id!} />
                        </motion.div>
                      )}
                    </AnimatePresence>
                  </div>
                );
              })}
              
              {/* Loading indicator for infinite scroll */}
              {loading && (
                <div className="py-6 flex justify-center">
                  <Loader size="md" type="spinner" />
                </div>
              )}
              
              {/* Load more button as fallback */}
              {!loading && hasMore && (
                <div className="py-6 flex justify-center">
                  <Button 
                    variant="secondary" 
                    onClick={loadMore}
                  >
                    {t('feed.loadMore')}
                  </Button>
                </div>
              )}
            </div>
          )}
        </motion.div>
      </div>
    </PageLayout>
  );
};

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common'])),
    },
  };
};

export default FeedPage;