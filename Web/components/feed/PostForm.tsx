import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { useAuth } from '../../contexts/AuthContext';
import { createPost, Post } from '../../services/postService';

interface PostFormProps {
  onPostCreated?: (post: Post) => void;
}

export const PostForm: React.FC<PostFormProps> = ({ onPostCreated }) => {
  const { t } = useTranslation('common');
  const { user } = useAuth();
  
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [postType, setPostType] = useState<'Activism' | 'Opportunity' | 'SuccessStory'>('Activism');
  const [tags, setTags] = useState('');
  const [media, setMedia] = useState<File[]>([]);
  const [mediaPreview, setMediaPreview] = useState<string[]>([]);
  const [location, setLocation] = useState('');
  const [eventDate, setEventDate] = useState('');
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  
  const handleMediaChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const filesArray = Array.from(e.target.files);
      setMedia([...media, ...filesArray]);
      
      // Create preview URLs
      const newPreviews = filesArray.map(file => URL.createObjectURL(file));
      setMediaPreview([...mediaPreview, ...newPreviews]);
    }
  };
  
  const removeMedia = (index: number) => {
    const newMedia = [...media];
    newMedia.splice(index, 1);
    setMedia(newMedia);
    
    const newPreviews = [...mediaPreview];
    URL.revokeObjectURL(newPreviews[index]);
    newPreviews.splice(index, 1);
    setMediaPreview(newPreviews);
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!title.trim() || !content.trim()) {
      setError(t('errors.requiredFields'));
      return;
    }
    
    try {
      setLoading(true);
      setError('');
      
      // In a real implementation, we would upload media files first
      // and then include the URLs in the post data
      
      const post: Post = {
        title,
        content,
        postType,
        tags: tags.split(',').map(tag => tag.trim()).filter(tag => tag),
        // mediaUrls would come from uploaded files
        mediaUrls: [],
      };
      
      if (location) {
        const [city, country] = location.split(',').map(part => part.trim());
        post.location = {
          city,
          country
        };
      }
      
      if (eventDate && postType === 'Opportunity') {
        post.eventDate = new Date(eventDate).toISOString();
      }
      
      const createdPost = await createPost(post);
      
      // Reset form
      setTitle('');
      setContent('');
      setPostType('Activism');
      setTags('');
      setMedia([]);
      setMediaPreview([]);
      setLocation('');
      setEventDate('');
      
      // Notify parent component
      if (onPostCreated) {
        onPostCreated(createdPost);
      }
    } catch (err) {
      console.error('Failed to create post:', err);
      setError(t('errors.postFailed'));
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="ios-card p-4 mb-6"
    >
      <h3 className="text-lg font-semibold mb-4">{t('feed.createPost')}</h3>
      
      {error && (
        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg">
          {error}
        </div>
      )}
      
      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder={t('feed.postTitlePlaceholder')}
            className="w-full rounded-lg border border-gray-200 p-3 focus:border-ios-black focus:outline-none focus:ring-1 focus:ring-ios-black"
            required
          />
        </div>
        
        <div className="mb-4">
          <textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder={t('feed.postContentPlaceholder')}
            className="w-full rounded-lg border border-gray-200 p-3 focus:border-ios-black focus:outline-none focus:ring-1 focus:ring-ios-black"
            rows={4}
            required
          />
        </div>
        
        <div className="mb-4">
          <select
            value={postType}
            onChange={(e) => setPostType(e.target.value as any)}
            className="w-full rounded-lg border border-gray-200 p-3 focus:border-ios-black focus:outline-none focus:ring-1 focus:ring-ios-black"
          >
            <option value="Activism">{t('feed.postTypes.activism')}</option>
            <option value="Opportunity">{t('feed.postTypes.opportunity')}</option>
            <option value="SuccessStory">{t('feed.postTypes.successStory')}</option>
          </select>
        </div>
        
        <div className="mb-4">
          <input
            type="text"
            value={tags}
            onChange={(e) => setTags(e.target.value)}
            placeholder={t('feed.tagsPlaceholder')}
            className="w-full rounded-lg border border-gray-200 p-3 focus:border-ios-black focus:outline-none focus:ring-1 focus:ring-ios-black"
          />
          <p className="mt-1 text-xs text-gray-500">{t('feed.tagsHelp')}</p>
        </div>
        
        {postType === 'Opportunity' && (
          <>
            <div className="mb-4">
              <input
                type="text"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                placeholder={t('feed.locationPlaceholder')}
                className="w-full rounded-lg border border-gray-200 p-3 focus:border-ios-black focus:outline-none focus:ring-1 focus:ring-ios-black"
              />
              <p className="mt-1 text-xs text-gray-500">{t('feed.locationHelp')}</p>
            </div>
            
            <div className="mb-4">
              <input
                type="date"
                value={eventDate}
                onChange={(e) => setEventDate(e.target.value)}
                className="w-full rounded-lg border border-gray-200 p-3 focus:border-ios-black focus:outline-none focus:ring-1 focus:ring-ios-black"
              />
              <p className="mt-1 text-xs text-gray-500">{t('feed.eventDateHelp')}</p>
            </div>
          </>
        )}
        
        {/* Media Upload */}
        <div className="mb-4">
          <label className="block mb-2">
            <span className="sr-only">{t('feed.chooseMedia')}</span>
            <input
              type="file"
              onChange={handleMediaChange}
              accept="image/*,video/*"
              className="block w-full text-sm text-gray-500
                file:mr-4 file:py-2 file:px-4
                file:rounded-full file:border-0
                file:text-sm file:font-semibold
                file:bg-ios-black file:text-white
                hover:file:bg-opacity-80"
              multiple
            />
          </label>
          
          {/* Media Previews */}
          {mediaPreview.length > 0 && (
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2 mt-2">
              {mediaPreview.map((src, index) => (
                <div key={index} className="relative">
                  <img
                    src={src}
                    alt={`Preview ${index + 1}`}
                    className="h-24 w-full rounded-md object-cover"
                  />
                  <button
                    type="button"
                    onClick={() => removeMedia(index)}
                    className="absolute top-1 right-1 rounded-full bg-white p-1 text-gray-500 shadow-sm hover:text-gray-700"
                  >
                    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
        
        <div className="flex justify-end">
          <button
            type="submit"
            disabled={loading || !title.trim() || !content.trim()}
            className="ios-button"
          >
            {loading ? t('feed.posting') : t('feed.post')}
          </button>
        </div>
      </form>
    </motion.div>
  );
};