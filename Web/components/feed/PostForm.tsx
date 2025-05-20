import React, { useState, useRef } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { useAuth } from '../../contexts/AuthContext';
import { createPost, Post } from '../../services/postService';
import { Button } from '../common/Button/Button';
import { FormField } from '../common/Form/FormField';
import { Input } from '../common/Form/Input';
import { TextArea } from '../common/Form/TextArea';
import { Select } from '../common/Form/Select';
import { FileUpload } from '../common/Form/FileUpload';
import { Alert } from '../common/Alert/Alert';
import { batchConvertToWebP } from '../common/MediaConverter/WebPConverter';
import { checkVideoFormatCompatibility } from '../common/MediaConverter/WebMConverter';
import { uploadFiles } from '../../services/mediaService';

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
  const [location, setLocation] = useState('');
  const [eventDate, setEventDate] = useState('');
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [mediaUploadProgress, setMediaUploadProgress] = useState(0);
  
  const formRef = useRef<HTMLFormElement>(null);
  
  const handleMediaChange = (files: File[]) => {
    setMedia(files);
  };
  
  const validateForm = () => {
    if (!title.trim()) {
      setError(t('errors.required'));
      return false;
    }
    
    if (!content.trim()) {
      setError(t('errors.required'));
      return false;
    }
    
    return true;
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }
    
    try {
      setLoading(true);
      setError('');
      
      let mediaUrls: string[] = [];
      
      // Upload media files if any
      if (media.length > 0) {
        // Convert images to WebP format for better performance
        const optimizedMedia = await batchConvertToWebP(media);
        
        // Check video compatibility
        for (const file of optimizedMedia) {
          if (file.type.startsWith('video/')) {
            const compatibility = await checkVideoFormatCompatibility(file);
            if (!compatibility.isCompatible) {
              setError(compatibility.message);
              setLoading(false);
              return;
            }
          }
        }
        
        // Upload files
        mediaUrls = await uploadFiles(optimizedMedia, 'posts');
      }
      
      // Parse location
      let locationObj = null;
      if (location) {
        const [city, country] = location.split(',').map(part => part.trim());
        locationObj = {
          city,
          country
        };
      }
      
      // Create post object
      const post: Post = {
        title,
        content,
        postType,
        tags: tags.split(',').map(tag => tag.trim()).filter(tag => tag),
        mediaUrls,
        location: locationObj,
        eventDate: eventDate ? new Date(eventDate).toISOString() : undefined,
      };
      
      // Submit post to API
      const createdPost = await createPost(post);
      
      // Reset form
      setTitle('');
      setContent('');
      setPostType('Activism');
      setTags('');
      setMedia([]);
      setLocation('');
      setEventDate('');
      
      // Notify parent component
      if (onPostCreated) {
        onPostCreated(createdPost);
      }
      
      // Reset form using ref
      if (formRef.current) {
        formRef.current.reset();
      }
    } catch (err) {
      console.error('Failed to create post:', err);
      setError(t('errors.postFailed'));
    } finally {
      setLoading(false);
      setMediaUploadProgress(0);
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
        <Alert 
          variant="error" 
          className="mb-4"
          onClose={() => setError('')}
        >
          {error}
        </Alert>
      )}
      
      <form ref={formRef} onSubmit={handleSubmit} className="space-y-4">
        <FormField
          label={t('feed.postTitlePlaceholder')}
          htmlFor="title"
          required
        >
          <Input
            id="title"
            name="title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder={t('feed.postTitlePlaceholder')}
            required
          />
        </FormField>
        
        <FormField
          label={t('feed.postContentPlaceholder')}
          htmlFor="content"
          required
        >
          <TextArea
            id="content"
            name="content"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder={t('feed.postContentPlaceholder')}
            rows={4}
            required
          />
        </FormField>
        
        <FormField
          label={t('feed.postType')}
          htmlFor="postType"
          required
        >
          <Select
            id="postType"
            name="postType"
            value={postType}
            onChange={(e) => setPostType(e.target.value as any)}
            options={[
              { value: 'Activism', label: t('feed.postTypes.activism') },
              { value: 'Opportunity', label: t('feed.postTypes.opportunity') },
              { value: 'SuccessStory', label: t('feed.postTypes.successStory') }
            ]}
          />
        </FormField>
        
        <FormField
          label={t('feed.tagsPlaceholder')}
          htmlFor="tags"
          helpText={t('feed.tagsHelp')}
        >
          <Input
            id="tags"
            name="tags"
            value={tags}
            onChange={(e) => setTags(e.target.value)}
            placeholder={t('feed.tagsPlaceholder')}
          />
        </FormField>
        
        {postType === 'Opportunity' && (
          <>
            <FormField
              label={t('feed.locationPlaceholder')}
              htmlFor="location"
              helpText={t('feed.locationHelp')}
            >
              <Input
                id="location"
                name="location"
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                placeholder={t('feed.locationPlaceholder')}
              />
            </FormField>
            
            <FormField
              label={t('opportunities.date')}
              htmlFor="eventDate"
              helpText={t('feed.eventDateHelp')}
            >
              <Input
                id="eventDate"
                name="eventDate"
                type="datetime-local"
                value={eventDate}
                onChange={(e) => setEventDate(e.target.value)}
              />
            </FormField>
          </>
        )}
        
        {/* Media Upload */}
        <FormField
          label={t('feed.chooseMedia')}
          htmlFor="media"
        >
          <FileUpload
            onChange={handleMediaChange}
            accept="image/*,video/*"
            multiple
            maxFiles={5}
            maxSize={10} // 10MB
            value={media}
          />
        </FormField>
        
        {/* Media Upload Progress */}
        {loading && mediaUploadProgress > 0 && (
          <div className="w-full bg-gray-200 rounded-full h-2.5">
            <div 
              className="bg-ios-black h-2.5 rounded-full" 
              style={{ width: `${mediaUploadProgress}%` }}
            ></div>
            <p className="text-xs text-gray-500 mt-1">
              {t('feed.uploading')}: {mediaUploadProgress}%
            </p>
          </div>
        )}
        
        <div className="flex justify-end">
          <Button
            type="submit"
            disabled={loading || !title.trim() || !content.trim()}
          >
            {loading ? t('feed.posting') : t('feed.post')}
          </Button>
        </div>
      </form>
    </motion.div>
  );
};