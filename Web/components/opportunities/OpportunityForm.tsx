import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { useForm } from 'react-hook-form';
import { Post } from '../../services/postService';
import { FormField } from '../common/Form/FormField';
import { Input } from '../common/Form/Input';
import { TextArea } from '../common/Form/TextArea';
import { Select } from '../common/Form/Select';
import { FileUpload } from '../common/Form/FileUpload';
import { Button } from '../common/Button/Button';
import { Alert } from '../common/Alert/Alert';
import { LocationSearch } from './LocationSearch';

interface OpportunityFormProps {
  initialData?: Partial<Post>;
  onSubmit: (data: Partial<Post>) => Promise<void>;
  isSubmitting?: boolean;
}

export const OpportunityForm: React.FC<OpportunityFormProps> = ({
  initialData = {},
  onSubmit,
  isSubmitting = false,
}) => {
  const { t } = useTranslation('common');
  const [media, setMedia] = useState<File[]>([]);
  const [location, setLocation] = useState<{
    address?: string;
    city?: string;
    country?: string;
    latitude?: number;
    longitude?: number;
  }>(initialData.location || {});
  
  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<Partial<Post>>({
    defaultValues: {
      title: initialData.title || '',
      content: initialData.content || '',
      tags: initialData.tags?.join(', ') || '',
      eventDate: initialData.eventDate ? new Date(initialData.eventDate).toISOString().slice(0, 16) : '',
    }
  });
  
  const handleLocationSelect = (selectedLocation: any) => {
    setLocation({
      address: selectedLocation.address,
      city: selectedLocation.city,
      country: selectedLocation.country,
      latitude: selectedLocation.latitude,
      longitude: selectedLocation.longitude,
    });
  };
  
  const handleMediaChange = (files: File[]) => {
    setMedia(files);
  };
  
  const processFormData = (data: Partial<Post>) => {
    // Process tags
    const tags = data.tags ? 
      data.tags.toString().split(',').map(tag => tag.trim()).filter(Boolean) : 
      [];
    
    // Create opportunity data
    const opportunityData: Partial<Post> = {
      ...data,
      tags,
      location,
      mediaUrls: media as any, // This will be handled by the API client
    };
    
    return opportunityData;
  };
  
  const onFormSubmit = async (data: Partial<Post>) => {
    const processedData = processFormData(data);
    await onSubmit(processedData);
  };
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="ios-card p-6"
    >
      <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-6">
        {/* Title */}
        <FormField
          label={t('opportunities.form.title')}
          htmlFor="title"
          error={errors.title?.message}
          required
        >
          <Input
            id="title"
            {...register('title', { 
              required: t('errors.required'),
              minLength: { value: 5, message: t('opportunities.form.titleMinLength') }
            })}
            placeholder={t('opportunities.form.titlePlaceholder')}
          />
        </FormField>
        
        {/* Description */}
        <FormField
          label={t('opportunities.form.description')}
          htmlFor="content"
          error={errors.content?.message}
          required
        >
          <TextArea
            id="content"
            {...register('content', { 
              required: t('errors.required'),
              minLength: { value: 20, message: t('opportunities.form.descriptionMinLength') }
            })}
            placeholder={t('opportunities.form.descriptionPlaceholder')}
            rows={6}
          />
        </FormField>
        
        {/* Category Tags */}
        <FormField
          label={t('opportunities.form.tags')}
          htmlFor="tags"
          error={errors.tags?.message}
          helpText={t('opportunities.form.tagsHelp')}
          required
        >
          <Input
            id="tags"
            {...register('tags', { required: t('errors.required') })}
            placeholder={t('opportunities.form.tagsPlaceholder')}
          />
        </FormField>
        
        {/* Location */}
        <FormField
          label={t('opportunities.form.location')}
          htmlFor="location"
          required
        >
          <LocationSearch
            initialValue={location.address || ''}
            onSelect={handleLocationSelect}
            placeholder={t('opportunities.form.locationPlaceholder')}
          />
          {location.city && location.country && (
            <div className="mt-2 text-sm text-green-600">
              {t('opportunities.form.locationSelected')}: {location.city}, {location.country}
            </div>
          )}
        </FormField>
        
        {/* Event Date */}
        <FormField
          label={t('opportunities.form.eventDate')}
          htmlFor="eventDate"
          error={errors.eventDate?.message}
          required
        >
          <Input
            id="eventDate"
            type="datetime-local"
            {...register('eventDate', { required: t('errors.required') })}
          />
        </FormField>
        
        {/* Media Upload */}
        <FormField
          label={t('opportunities.form.media')}
          htmlFor="media"
          helpText={t('opportunities.form.mediaHelp')}
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
        
        {/* Submit Button */}
        <div className="flex justify-end">
          <Button
            type="submit"
            disabled={isSubmitting}
          >
            {isSubmitting ? t('common.submitting') : t('opportunities.form.submit')}
          </Button>
        </div>
      </form>
    </motion.div>
  );
};