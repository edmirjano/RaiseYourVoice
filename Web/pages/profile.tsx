import React from 'react';
import { NextPage } from 'next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { useTranslation } from 'next-i18next';
import MainLayout from '../components/MainLayout';
import ProtectedRoute from '../components/auth/ProtectedRoute';
import { useAuth } from '../contexts/AuthContext';
import { motion } from 'framer-motion';

const Profile: NextPage = () => {
  const { t } = useTranslation('common');
  const { user } = useAuth();
  
  return (
    <ProtectedRoute>
      <MainLayout>
        <div className="max-w-4xl mx-auto">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
            className="bg-white rounded-xl shadow-sm p-6 mb-8"
          >
            <div className="flex flex-col sm:flex-row items-center sm:items-start gap-6">
              <div className="w-24 h-24 rounded-full bg-ios-gray flex items-center justify-center text-white text-3xl font-semibold overflow-hidden">
                {user?.profilePicture ? (
                  <img 
                    src={user.profilePicture} 
                    alt={user.name} 
                    className="w-full h-full object-cover"
                  />
                ) : (
                  user?.name?.charAt(0).toUpperCase()
                )}
              </div>
              
              <div className="text-center sm:text-left">
                <h1 className="text-2xl font-bold text-ios-black mb-2">{user?.name}</h1>
                <p className="text-gray-600 mb-4">{user?.email}</p>
                <p className="inline-block px-3 py-1 bg-ios-light-gray rounded-full text-sm font-medium mb-4">
                  {t(`roles.${user?.role?.toLowerCase()}`)}
                </p>
                
                <div className="mt-4">
                  <button className="ios-button-secondary text-sm">
                    {t('profile.editProfile')}
                  </button>
                </div>
              </div>
            </div>
          </motion.div>
          
          <div className="space-y-6">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.3, delay: 0.1 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <h2 className="text-xl font-semibold mb-4">{t('profile.accountSettings')}</h2>
              <div className="space-y-4">
                <div className="flex justify-between items-center pb-3 border-b border-gray-100">
                  <span className="text-gray-700">{t('profile.language')}</span>
                  <span className="text-ios-black font-medium">{t('languages.current')}</span>
                </div>
                <div className="flex justify-between items-center pb-3 border-b border-gray-100">
                  <span className="text-gray-700">{t('profile.notifications')}</span>
                  <span className="text-ios-black font-medium">{t('common.enabled')}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-700">{t('profile.twoFactorAuth')}</span>
                  <span className="text-ios-black font-medium">{t('common.disabled')}</span>
                </div>
              </div>
            </motion.div>
            
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.3, delay: 0.2 }}
              className="bg-white rounded-xl shadow-sm p-6"
            >
              <h2 className="text-xl font-semibold mb-4">{t('profile.securitySettings')}</h2>
              <div className="space-y-4">
                <button className="ios-button-secondary text-sm">
                  {t('profile.changePassword')}
                </button>
                <button className="ios-button-secondary text-sm">
                  {t('profile.manageDevices')}
                </button>
              </div>
            </motion.div>
            
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.3, delay: 0.3 }}
              className="text-center py-6"
            >
              <button
                onClick={() => {}}
                className="text-red-500 hover:text-red-700 font-medium"
              >
                {t('profile.deleteAccount')}
              </button>
            </motion.div>
          </div>
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
};

export const getStaticProps = async ({ locale }: { locale: string }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale, ['common'])),
    },
  };
};

export default Profile;