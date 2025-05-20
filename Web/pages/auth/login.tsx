import React from 'react';
import { NextPage } from 'next';
import { useRouter } from 'next/router';
import { motion } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { serverSideTranslations } from 'next-i18next/serverSideTranslations';
import { useAuth } from '../../contexts/AuthContext';
import { LoginForm } from '../../components/auth/LoginForm';

const Login: NextPage = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { isAuthenticated } = useAuth();
  
  // If already authenticated, redirect to home
  React.useEffect(() => {
    if (isAuthenticated) {
      const returnUrl = router.query.returnUrl as string || '/feed';
      router.push(returnUrl);
    }
  }, [isAuthenticated, router]);
  
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      transition={{ duration: 0.3 }}
      className="min-h-screen flex flex-col justify-center items-center px-4 bg-white"
    >
      <LoginForm />
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

export default Login;