import React from 'react';
import { NextPage } from 'next';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { ResetPasswordForm } from '../../components/auth/ResetPasswordForm';

const ResetPassword: NextPage = () => {
  const { t } = useTranslation('common');
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      transition={{ duration: 0.3 }}
      className="min-h-screen flex flex-col justify-center items-center px-4 bg-white"
    >
      <ResetPasswordForm />
    </motion.div>
  );
};

export const getServerSideProps = async ({ locale }: { locale: string }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale, ['common'])),
    },
  };
};

export default ResetPassword;