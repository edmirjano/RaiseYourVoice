import { GetServerSideProps } from 'next';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { useState, useEffect } from 'react';
import MainLayout from '../components/MainLayout';
import { getSuccessStories, Post } from '../services/postService';
import Link from 'next/link';

export default function SuccessStories() {
  const { t } = useTranslation('common');
  const [stories, setStories] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchStories = async () => {
      try {
        setLoading(true);
        const data = await getSuccessStories();
        setStories(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch success stories:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };

    fetchStories();
  }, [t]);

  return (
    <MainLayout>
      <div className="space-y-6">
        <h1 className="text-3xl font-bold">{t('successStories.title')}</h1>
        <p className="text-gray-600">{t('successStories.description')}</p>

        {/* Create Story Button */}
        <div className="mb-6">
          <button className="ios-button">
            {t('successStories.createStory')}
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
        ) : stories.length === 0 ? (
          <div className="ios-card p-8 text-center">
            <h3 className="text-xl font-semibold mb-2">{t('successStories.noPosts.title')}</h3>
            <p className="text-gray-600">{t('successStories.noPosts.description')}</p>
          </div>
        ) : (
          <div className="space-y-8">
            {stories.map((story) => (
              <article key={story.id} className="ios-card p-6 ios-slide-up">
                <div className="flex flex-col md:flex-row">
                  {story.mediaUrls && story.mediaUrls.length > 0 && (
                    <div className="md:w-1/3 mb-4 md:mb-0 md:mr-6">
                      <img 
                        src={story.mediaUrls[0]} 
                        alt={story.title} 
                        className="rounded-lg w-full h-48 md:h-full object-cover"
                      />
                    </div>
                  )}
                  
                  <div className={story.mediaUrls && story.mediaUrls.length > 0 ? "md:w-2/3" : "w-full"}>
                    <Link href={`/success-stories/${story.id}`}>
                      <h2 className="text-2xl font-bold mb-3 hover:underline">{story.title}</h2>
                    </Link>
                    <p className="text-gray-600 mb-4">
                      {story.content.length > 200
                        ? `${story.content.substring(0, 200)}...`
                        : story.content}
                    </p>
                    
                    <div className="flex justify-between items-center">
                      <div className="flex space-x-4 text-sm text-gray-500">
                        <span>{story.likeCount} {t('feed.likes')}</span>
                        <span>{story.commentCount} {t('feed.comments')}</span>
                      </div>
                      <div className="text-sm text-gray-500">
                        {story.createdAt && new Date(story.createdAt).toLocaleDateString()}
                      </div>
                    </div>
                    
                    <div className="mt-4">
                      <Link 
                        href={`/success-stories/${story.id}`} 
                        className="text-ios-black font-semibold hover:underline"
                      >
                        Read More →
                      </Link>
                    </div>
                  </div>
                </div>
              </article>
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