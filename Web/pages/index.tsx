import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { useState, useEffect } from 'react';
import MainLayout from '../components/MainLayout';
import { getSocialFeed, Post } from '../services/postService';

export default function Home() {
  const { t } = useTranslation('common');
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

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

  return (
    <MainLayout>
      <div className="space-y-6">
        <h1 className="text-3xl font-bold">{t('feed.title')}</h1>
        <p className="text-gray-600">{t('feed.description')}</p>

        {/* Add Post Button */}
        <div className="mb-6">
          <button className="ios-button">
            {t('feed.createPost')}
          </button>
        </div>

        {loading ? (
          <div className="flex justify-center py-8">
            <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-ios-black"></div>
          </div>
        ) : error ? (
          <div className="bg-red-50 p-4 rounded-lg text-red-500">
            {error}
          </div>
        ) : posts.length === 0 ? (
          <div className="ios-card p-8 text-center">
            <h3 className="text-xl font-semibold mb-2">{t('feed.noPosts.title')}</h3>
            <p className="text-gray-600">{t('feed.noPosts.description')}</p>
          </div>
        ) : (
          <div className="space-y-4">
            {posts.map((post) => (
              <div key={post.id} className="ios-card p-6 ios-slide-up">
                <h2 className="text-xl font-bold mb-2">{post.title}</h2>
                <p className="text-gray-600 mb-4">{post.content}</p>
                
                {post.mediaUrls && post.mediaUrls.length > 0 && (
                  <div className="mb-4">
                    <img 
                      src={post.mediaUrls[0]} 
                      alt={post.title} 
                      className="rounded-lg w-full h-64 object-cover"
                    />
                  </div>
                )}
                
                <div className="flex justify-between items-center text-sm text-gray-500">
                  <div className="flex space-x-4">
                    <span>{post.likeCount} {t('feed.likes')}</span>
                    <span>{post.commentCount} {t('feed.comments')}</span>
                  </div>
                  <div>
                    {post.createdAt && new Date(post.createdAt).toLocaleDateString()}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </MainLayout>
  );
}

export const getServerSideProps: GetServerSideProps = async ({ locale }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale || 'en', ['common'])),
    },
  };
};