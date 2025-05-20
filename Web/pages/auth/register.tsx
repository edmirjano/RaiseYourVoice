import React from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { useAuth } from '../../contexts/AuthContext';
import { RegisterForm } from '../../components/auth/RegisterForm';

const Register: NextPage = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { isAuthenticated } = useAuth();
  
  // If already authenticated, redirect to home
  React.useEffect(() => {
    if (isAuthenticated) {
      router.push('/feed');
    }
  }, [isAuthenticated, router]);
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      transition={{ duration: 0.3 }}
      className="min-h-screen flex flex-col justify-center items-center px-4 py-12 bg-white"
    >
      <RegisterForm />
    </motion.div>
  );
};

export const getStaticProps = async ({ locale }: { locale: string }) => {
  return {
    props: {
      ...(await serverSideTranslations(locale, ['common'])),
    },
  };
};

export default Register;