import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { useAuth } from '../../contexts/AuthContext';

interface DonationFormProps {
  campaignId: string;
  campaignTitle: string;
  onDonationComplete?: () => void;
}

export const DonationForm: React.FC<DonationFormProps> = ({ 
  campaignId, 
  campaignTitle,
  onDonationComplete 
}) => {
  const { t } = useTranslation('common');
  const { isAuthenticated, user } = useAuth();
  
  const [amount, setAmount] = useState('');
  const [customAmount, setCustomAmount] = useState('');
  const [isAnonymous, setIsAnonymous] = useState(false);
  const [message, setMessage] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('card');
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  
  const predefinedAmounts = ['10', '25', '50', '100', '250'];
  
  const handleAmountSelect = (value: string) => {
    setAmount(value);
    setCustomAmount('');
  };
  
  const handleCustomAmountChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    // Only allow numbers and decimal point
    if (value === '' || /^\d+(\.\d{0,2})?$/.test(value)) {
      setCustomAmount(value);
      setAmount('custom');
    }
  };
  
  const getDonationAmount = () => {
    if (amount === 'custom') {
      return parseFloat(customAmount);
    }
    return parseFloat(amount);
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    const donationAmount = getDonationAmount();
    
    if (isNaN(donationAmount) || donationAmount <= 0) {
      setError(t('donations.invalidAmount'));
      return;
    }
    
    try {
      setLoading(true);
      setError('');
      
      // In a real implementation, this would call the API to process the donation
      // For now, we'll just simulate a successful donation
      await new Promise(resolve => setTimeout(resolve, 1500));
      
      setSuccess(true);
      
      // Reset form
      setAmount('');
      setCustomAmount('');
      setIsAnonymous(false);
      setMessage('');
      
      // Notify parent component
      if (onDonationComplete) {
        onDonationComplete();
      }
    } catch (err) {
      console.error('Failed to process donation:', err);
      setError(t('donations.processingError'));
    } finally {
      setLoading(false);
    }
  };
  
  if (success) {
    return (
      <motion.div
        initial={{ opacity: 0, scale: 0.95 }}
        animate={{ opacity: 1, scale: 1 }}
        className="ios-card p-6 text-center"
      >
        <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg className="h-8 w-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h3 className="text-xl font-semibold mb-2">{t('donations.thankYou')}</h3>
        <p className="text-gray-600 mb-6">
          {t('donations.successMessage', { amount: getDonationAmount() })}
        </p>
        <button
          onClick={() => setSuccess(false)}
          className="ios-button"
        >
          {t('donations.donateAgain')}
        </button>
      </motion.div>
    );
  }
  
  if (!isAuthenticated) {
    return (
      <div className="ios-card p-6 text-center">
        <h3 className="text-xl font-semibold mb-2">{t('donations.loginRequired')}</h3>
        <p className="text-gray-600 mb-6">
          {t('donations.loginMessage')}
        </p>
        <a href="/auth/login" className="ios-button inline-block">
          {t('auth.login')}
        </a>
      </div>
    );
  }
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="ios-card p-6"
    >
      <h3 className="text-xl font-semibold mb-4">{t('donations.title')}</h3>
      <p className="text-gray-600 mb-6">
        {t('donations.supportMessage', { campaign: campaignTitle })}
      </p>
      
      {error && (
        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg">
          {error}
        </div>
      )}
      
      <form onSubmit={handleSubmit}>
        {/* Donation Amount */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            {t('donations.selectAmount')}
          </label>
          <div className="grid grid-cols-3 gap-2 mb-2">
            {predefinedAmounts.map((value) => (
              <button
                key={value}
                type="button"
                className={`py-2 px-4 rounded-lg border ${
                  amount === value 
                    ? 'border-ios-black bg-ios-black text-white' 
                    : 'border-gray-200 hover:border-gray-300'
                }`}
                onClick={() => handleAmountSelect(value)}
              >
                ${value}
              </button>
            ))}
            <button
              type="button"
              className={`py-2 px-4 rounded-lg border ${
                amount === 'custom' 
                  ? 'border-ios-black bg-ios-black text-white' 
                  : 'border-gray-200 hover:border-gray-300'
              }`}
              onClick={() => setAmount('custom')}
            >
              {t('donations.custom')}
            </button>
          </div>
          
          {amount === 'custom' && (
            <div className="mt-2">
              <div className="relative rounded-lg">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <span className="text-gray-500 sm:text-sm">$</span>
                </div>
                <input
                  type="text"
                  value={customAmount}
                  onChange={handleCustomAmountChange}
                  placeholder="0.00"
                  className="ios-input pl-7 w-full"
                  required
                />
              </div>
            </div>
          )}
        </div>
        
        {/* Payment Method */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            {t('donations.paymentMethod')}
          </label>
          <div className="grid grid-cols-3 gap-2">
            <button
              type="button"
              className={`py-2 px-4 rounded-lg border ${
                paymentMethod === 'card' 
                  ? 'border-ios-black bg-ios-black text-white' 
                  : 'border-gray-200 hover:border-gray-300'
              }`}
              onClick={() => setPaymentMethod('card')}
            >
              {t('donations.creditCard')}
            </button>
            <button
              type="button"
              className={`py-2 px-4 rounded-lg border ${
                paymentMethod === 'paypal' 
                  ? 'border-ios-black bg-ios-black text-white' 
                  : 'border-gray-200 hover:border-gray-300'
              }`}
              onClick={() => setPaymentMethod('paypal')}
            >
              PayPal
            </button>
            <button
              type="button"
              className={`py-2 px-4 rounded-lg border ${
                paymentMethod === 'apple' 
                  ? 'border-ios-black bg-ios-black text-white' 
                  : 'border-gray-200 hover:border-gray-300'
              }`}
              onClick={() => setPaymentMethod('apple')}
            >
              Apple Pay
            </button>
          </div>
        </div>
        
        {/* Anonymous Donation */}
        <div className="mb-6">
          <div className="flex items-center">
            <input
              id="anonymous"
              type="checkbox"
              checked={isAnonymous}
              onChange={(e) => setIsAnonymous(e.target.checked)}
              className="h-4 w-4 text-ios-black focus:ring-ios-black border-gray-300 rounded"
            />
            <label htmlFor="anonymous" className="ml-2 block text-sm text-gray-900">
              {t('donations.donateAnonymously')}
            </label>
          </div>
          <p className="mt-1 text-xs text-gray-500">
            {t('donations.anonymousDescription')}
          </p>
        </div>
        
        {/* Message */}
        <div className="mb-6">
          <label htmlFor="message" className="block text-sm font-medium text-gray-700 mb-1">
            {t('donations.message')} ({t('donations.optional')})
          </label>
          <textarea
            id="message"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            rows={3}
            className="ios-input w-full"
            placeholder={t('donations.messagePlaceholder')}
          />
        </div>
        
        {/* Submit Button */}
        <button
          type="submit"
          disabled={loading || (!amount || (amount === 'custom' && !customAmount))}
          className="ios-button w-full flex justify-center"
        >
          {loading ? (
            <>
              <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
              {t('donations.processing')}
            </>
          ) : (
            t('donations.donate', { amount: amount === 'custom' ? `$${customAmount}` : `$${amount}` })
          )}
        </button>
      </form>
    </motion.div>
  );
};