import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { useForm } from 'react-hook-form';
import { useAuth } from '../../contexts/AuthContext';
import { FormField } from '../common/Form/FormField';
import { Input } from '../common/Form/Input';
import { TextArea } from '../common/Form/TextArea';
import { Checkbox } from '../common/Form/Checkbox';
import { Button } from '../common/Button/Button';
import { Alert } from '../common/Alert/Alert';

interface RegistrationFormData {
  name: string;
  email: string;
  phone?: string;
  message?: string;
  agreeToTerms: boolean;
}

interface OpportunityRegistrationProps {
  opportunityId: string;
  opportunityTitle: string;
  onRegistrationComplete?: () => void;
  className?: string;
}

export const OpportunityRegistration: React.FC<OpportunityRegistrationProps> = ({
  opportunityId,
  opportunityTitle,
  onRegistrationComplete,
  className = '',
}) => {
  const { t } = useTranslation('common');
  const { user, isAuthenticated } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  
  const { register, handleSubmit, formState: { errors } } = useForm<RegistrationFormData>({
    defaultValues: {
      name: user?.name || '',
      email: user?.email || '',
      phone: '',
      message: '',
      agreeToTerms: false,
    }
  });
  
  const onSubmit = async (data: RegistrationFormData) => {
    try {
      setIsSubmitting(true);
      setError('');
      
      // In a real implementation, this would call an API endpoint
      // For now, we'll simulate a successful registration
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      setSuccess(true);
      
      if (onRegistrationComplete) {
        onRegistrationComplete();
      }
    } catch (err) {
      console.error('Failed to register for opportunity:', err);
      setError(t('opportunities.registrationError'));
    } finally {
      setIsSubmitting(false);
    }
  };
  
  if (!isAuthenticated) {
    return (
      <div className={`ios-card p-6 ${className}`}>
        <h3 className="text-lg font-semibold mb-4">{t('opportunities.register')}</h3>
        <p className="text-gray-600 mb-4">{t('opportunities.loginToRegister')}</p>
        <Button
          onClick={() => window.location.href = '/auth/login'}
        >
          {t('auth.login')}
        </Button>
      </div>
    );
  }
  
  if (success) {
    return (
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        className={`ios-card p-6 ${className}`}
      >
        <div className="text-center">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="h-8 w-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold mb-2">{t('opportunities.registrationSuccess')}</h3>
          <p className="text-gray-600 mb-4">
            {t('opportunities.registrationSuccessMessage', { opportunity: opportunityTitle })}
          </p>
          <div className="flex justify-center space-x-4">
            <Button
              variant="secondary"
              onClick={() => setSuccess(false)}
            >
              {t('opportunities.registerAnother')}
            </Button>
            <Button
              onClick={() => window.location.href = '/opportunities'}
            >
              {t('opportunities.browseMore')}
            </Button>
          </div>
        </div>
      </motion.div>
    );
  }
  
  return (
    <div className={`ios-card p-6 ${className}`}>
      <h3 className="text-lg font-semibold mb-4">{t('opportunities.register')}</h3>
      
      {error && (
        <Alert 
          variant="error" 
          className="mb-4"
          onClose={() => setError('')}
        >
          {error}
        </Alert>
      )}
      
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {/* Name */}
        <FormField
          label={t('opportunities.form.name')}
          htmlFor="name"
          error={errors.name?.message}
          required
        >
          <Input
            id="name"
            {...register('name', { required: t('errors.required') })}
            placeholder={t('opportunities.form.namePlaceholder')}
          />
        </FormField>
        
        {/* Email */}
        <FormField
          label={t('opportunities.form.email')}
          htmlFor="email"
          error={errors.email?.message}
          required
        >
          <Input
            id="email"
            type="email"
            {...register('email', { 
              required: t('errors.required'),
              pattern: {
                value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                message: t('errors.invalidEmail')
              }
            })}
            placeholder={t('opportunities.form.emailPlaceholder')}
          />
        </FormField>
        
        {/* Phone */}
        <FormField
          label={t('opportunities.form.phone')}
          htmlFor="phone"
          error={errors.phone?.message}
          helpText={t('opportunities.form.phoneHelp')}
        >
          <Input
            id="phone"
            type="tel"
            {...register('phone')}
            placeholder={t('opportunities.form.phonePlaceholder')}
          />
        </FormField>
        
        {/* Message */}
        <FormField
          label={t('opportunities.form.message')}
          htmlFor="message"
          error={errors.message?.message}
          helpText={t('opportunities.form.messageHelp')}
        >
          <TextArea
            id="message"
            {...register('message')}
            placeholder={t('opportunities.form.messagePlaceholder')}
            rows={3}
          />
        </FormField>
        
        {/* Terms Agreement */}
        <FormField
          error={errors.agreeToTerms?.message}
        >
          <Checkbox
            id="agreeToTerms"
            {...register('agreeToTerms', { 
              required: t('opportunities.form.termsRequired')
            })}
            label={t('opportunities.form.agreeToTerms')}
          />
        </FormField>
        
        {/* Submit Button */}
        <div className="flex justify-end">
          <Button
            type="submit"
            disabled={isSubmitting}
          >
            {isSubmitting ? t('common.submitting') : t('opportunities.register')}
          </Button>
        </div>
      </form>
    </div>
  );
};