import React from 'react';
import Head from 'next/head';
import { motion } from 'framer-motion';
import { Header } from './Header';
import { Footer } from './Footer';

type PageLayoutProps = {
  children: React.ReactNode;
  title?: string;
  description?: string;
  keywords?: string;
  ogImage?: string;
  ogUrl?: string;
  noIndex?: boolean;
  className?: string;
  fullWidth?: boolean;
};

export const PageLayout: React.FC<PageLayoutProps> = ({
  children,
  title = 'RaiseYourVoice - Amplify your activism',
  description = 'Connect with activists, discover opportunities, and make a difference in your community.',
  keywords = 'activism, social change, community, opportunities, campaigns',
  ogImage = '/images/og-image.jpg',
  ogUrl,
  noIndex = false,
  className = '',
  fullWidth = false,
}) => {
  const pageTransition = {
    initial: { opacity: 0, y: 20 },
    animate: { opacity: 1, y: 0 },
    exit: { opacity: 0, y: -20 },
    transition: { duration: 0.3 },
  };

  return (
    <>
      <Head>
        <title>{title}</title>
        <meta name="description" content={description} />
        <meta name="keywords" content={keywords} />
        {noIndex && <meta name="robots" content="noindex, nofollow" />}
        
        {/* Open Graph / Social Media Meta Tags */}
        <meta property="og:title" content={title} />
        <meta property="og:description" content={description} />
        <meta property="og:image" content={ogImage} />
        {ogUrl && <meta property="og:url" content={ogUrl} />}
        <meta property="og:type" content="website" />
        
        {/* Twitter Card */}
        <meta name="twitter:card" content="summary_large_image" />
        <meta name="twitter:title" content={title} />
        <meta name="twitter:description" content={description} />
        <meta name="twitter:image" content={ogImage} />
        
        {/* Favicon */}
        <link rel="icon" href="/favicon.ico" />
      </Head>

      <div className="flex flex-col min-h-screen">
        <Header />
        
        <main className="flex-grow">
          <motion.div
            {...pageTransition}
            className={className}
          >
            {fullWidth ? (
              children
            ) : (
              <div className="container mx-auto px-4 py-8">
                {children}
              </div>
            )}
          </motion.div>
        </main>
        
        <Footer />
      </div>
    </>
  );
};