import React, { ReactNode } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { motion } from 'framer-motion';
import { SearchBar } from './search/SearchBar';
import NotificationBell from './NotificationBell';
import { useAuth } from '../contexts/AuthContext';

type MainLayoutProps = {
  children: ReactNode;
};

const MainLayout = ({ children }: MainLayoutProps) => {
  const router = useRouter();
  const { t } = useTranslation('common');
  const { user, isAuthenticated, logout } = useAuth();
  
  const handleLogout = () => {
    logout();
    router.push('/auth/login');
  };
  
  return (
    <div className="min-h-screen flex flex-col">
      {/* Header */}
      <header className="bg-ios-black text-white sticky top-0 z-50">
        <div className="container mx-auto py-4 px-4 flex justify-between items-center">
          <Link href="/" className="text-xl font-bold">
            RaiseYourVoice
          </Link>
          
          <div className="hidden md:flex space-x-6">
            <Link 
              href="/feed" 
              className={`hover:opacity-80 transition-opacity ${
                router.pathname === '/feed' ? 'font-bold' : ''
              }`}
            >
              {t('nav.feed')}
            </Link>
            <Link 
              href="/opportunities" 
              className={`hover:opacity-80 transition-opacity ${
                router.pathname.startsWith('/opportunities') ? 'font-bold' : ''
              }`}
            >
              {t('nav.opportunities')}
            </Link>
            <Link 
              href="/success-stories" 
              className={`hover:opacity-80 transition-opacity ${
                router.pathname.startsWith('/success-stories') ? 'font-bold' : ''
              }`}
            >
              {t('nav.successStories')}
            </Link>
            <Link 
              href="/campaigns" 
              className={`hover:opacity-80 transition-opacity ${
                router.pathname.startsWith('/campaigns') ? 'font-bold' : ''
              }`}
            >
              {t('nav.campaigns')}
            </Link>
            <Link 
              href="/organizations" 
              className={`hover:opacity-80 transition-opacity ${
                router.pathname.startsWith('/organizations') ? 'font-bold' : ''
              }`}
            >
              {t('nav.organizations')}
            </Link>
          </div>
          
          <div className="flex items-center space-x-4">
            {/* Search */}
            <div className="hidden md:block w-64">
              <SearchBar />
            </div>
            
            {isAuthenticated ? (
              <>
                {/* Notification Bell */}
                <NotificationBell />
                
                {/* User Menu */}
                <div className="relative group">
                  <button className="flex items-center space-x-1 hover:opacity-80 transition-opacity">
                    <div className="h-8 w-8 rounded-full bg-white text-ios-black flex items-center justify-center font-semibold">
                      {user?.name?.charAt(0) || 'U'}
                    </div>
                    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </button>
                  
                  {/* Dropdown Menu */}
                  <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg overflow-hidden opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 transform origin-top-right scale-95 group-hover:scale-100">
                    <div className="py-2">
                      <Link 
                        href="/profile" 
                        className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                      >
                        {t('nav.profile')}
                      </Link>
                      {(user?.role === 'Admin' || user?.role === 'Moderator') && (
                        <Link 
                          href="/admin/dashboard" 
                          className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                        >
                          {t('nav.adminDashboard')}
                        </Link>
                      )}
                      <button 
                        onClick={handleLogout}
                        className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                      >
                        {t('nav.logout')}
                      </button>
                    </div>
                  </div>
                </div>
              </>
            ) : (
              <>
                <Link 
                  href="/auth/login" 
                  className="hover:opacity-80 transition-opacity"
                >
                  {t('nav.login')}
                </Link>
                <Link 
                  href="/auth/register" 
                  className="ios-button text-sm"
                >
                  {t('nav.register')}
                </Link>
              </>
            )}
            
            {/* Language switcher */}
            <div className="flex space-x-2">
              <Link 
                href={router.pathname} 
                locale="en"
                className={`hover:opacity-80 transition-opacity ${
                  router.locale === 'en' ? 'font-bold underline' : ''
                }`}
              >
                EN
              </Link>
              <Link 
                href={router.pathname} 
                locale="sq"
                className={`hover:opacity-80 transition-opacity ${
                  router.locale === 'sq' ? 'font-bold underline' : ''
                }`}
              >
                SQ
              </Link>
            </div>
          </div>
        </div>
      </header>
      
      {/* Mobile Search Bar */}
      <div className="md:hidden bg-white border-b border-gray-200 px-4 py-2">
        <SearchBar />
      </div>
      
      {/* Mobile Navigation */}
      <div className="md:hidden fixed bottom-0 inset-x-0 bg-ios-black text-white z-40">
        <div className="flex justify-around py-3">
          <Link 
            href="/feed" 
            className={`flex flex-col items-center ${
              router.pathname === '/feed' ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1M19 20a2 2 0 002-2V8a2 2 0 00-2-2h-5a2 2 0 00-2 2v12a2 2 0 002 2h5z" />
            </svg>
            <span className="text-xs mt-1">{t('nav.feed')}</span>
          </Link>
          <Link 
            href="/opportunities" 
            className={`flex flex-col items-center ${
              router.pathname.startsWith('/opportunities') ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            <span className="text-xs mt-1">{t('nav.opportunities')}</span>
          </Link>
          <Link 
            href="/campaigns" 
            className={`flex flex-col items-center ${
              router.pathname.startsWith('/campaigns') ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <span className="text-xs mt-1">{t('nav.campaigns')}</span>
          </Link>
          <Link 
            href="/profile" 
            className={`flex flex-col items-center ${
              router.pathname.startsWith('/profile') ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            <span className="text-xs mt-1">{t('nav.profile')}</span>
          </Link>
        </div>
      </div>
      
      {/* Main Content */}
      <main className="flex-grow container mx-auto py-8 px-4 mb-16 md:mb-0">
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -10 }}
          transition={{ duration: 0.3 }}
        >
          {children}
        </motion.div>
      </main>
      
      {/* Footer */}
      <footer className="bg-ios-black text-white py-6">
        <div className="container mx-auto px-4">
          <div className="flex flex-col md:flex-row justify-between items-center">
            <div className="mb-4 md:mb-0">
              <div className="text-lg font-bold">RaiseYourVoice</div>
              <div className="text-sm opacity-70">{t('footer.tagline')}</div>
            </div>
            <div className="flex space-x-6">
              <Link href="/about" className="hover:opacity-80 transition-opacity">
                {t('footer.about')}
              </Link>
              <Link href="/privacy" className="hover:opacity-80 transition-opacity">
                {t('footer.privacy')}
              </Link>
              <Link href="/terms" className="hover:opacity-80 transition-opacity">
                {t('footer.terms')}
              </Link>
              <Link href="/contact" className="hover:opacity-80 transition-opacity">
                {t('footer.contact')}
              </Link>
            </div>
          </div>
          <div className="mt-6 text-center text-sm opacity-70">
            &copy; {new Date().getFullYear()} RaiseYourVoice. {t('footer.rights')}
          </div>
        </div>
      </footer>
    </div>
  );
};

export default MainLayout;