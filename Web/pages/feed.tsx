import React, { useState, useEffect } from 'react';
import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { motion } from 'framer-motion';
import MainLayout from '../components/MainLayout';
import { PostCard } from '../components/feed/PostCard';
import { PostForm } from '../components/feed/PostForm';
import { CommentSection } from '../components/feed/CommentSection';
import { getSocialFeed, likePost, Post } from '../services/postService';
import { useAuth } from '../contexts/AuthContext';

const FeedPage: React.FC = () => {
  const { t } = useTranslation('common');
  const { isAuthenticated } = useAuth();
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedPostId, setSelectedPostId] = useState<string | null>(null);
  
  useEffect(() => {
    const fetchPosts = async () => {
      try {
        setLoading(true);
        const data = await getSocialFeed();
        setPosts(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch posts:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchPosts();
  }, [t]);
  
  const handlePostCreated = (post: Post) => {
    setPosts([post, ...posts]);
  };
  
  const handleLike = async (id: string) => {
    try {
      await likePost(id);
    } catch (err) {
      console.error('Failed to like post:', err);
    }
  };
  
  const handleComment = (id: string) => {
    setSelectedPostId(id === selectedPostId ? null : id);
  };
  
  return (
    <MainLayout>
      <div className="max-w-2xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <h1 className="text-2xl font-bold mb-6">{t('feed.title')}</h1>
          
          {isAuthenticated && (
            <PostForm onPostCreated={handlePostCreated} />
          )}
          
          {error && (
            <div className="mb-6 p-4 bg-red-50 rounded-lg text-red-700">
              {error}
            </div>
          )}
          
          {loading ? (
            <div className="flex justify-center py-12">
              <div className="w-12 h-12 border-4 border-ios-black border-t-transparent rounded-full animate-spin"></div>
            </div>
          ) : posts.length === 0 ? (
            <div className="ios-card p-8 text-center">
              <h3 className="text-xl font-semibold mb-2">{t('feed.noPosts.title')}</h3>
              <p className="text-gray-600 mb-4">{t('feed.noPosts.description')}</p>
              {isAuthenticated && (
                <button className="ios-button">{t('feed.createPost')}</button>
              )}
            </div>
          ) : (
            <div className="space-y-6">
              {posts.map((post) => (
                <div key={post.id}>
                  <PostCard 
                    post={post} 
                    onLike={handleLike}
                    onComment={handleComment}
                  />
                  
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
                </div>
              ))}
            </div>
          )}
        </motion.div>
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

export default FeedPage;