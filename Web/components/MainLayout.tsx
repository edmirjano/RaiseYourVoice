import React, { ReactNode } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/router';
import { useTranslation } from 'next-i18next';
import { getCurrentUser, logout } from '../services/authService';
import NotificationBell from './NotificationBell';

type MainLayoutProps = {
  children: ReactNode;
};

const MainLayout = ({ children }: MainLayoutProps) => {
  const router = useRouter();
  const { t } = useTranslation('common');
  const user = getCurrentUser();
  
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
              href="/" 
              className={`hover:opacity-80 transition-opacity ${
                router.pathname === '/' ? 'font-bold' : ''
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
          </div>
          
          <div className="flex items-center space-x-4">
            {user ? (
              <>
                {/* Notification Bell */}
                <NotificationBell />
                
                <Link 
                  href="/profile" 
                  className="hover:opacity-80 transition-opacity"
                >
                  {t('nav.profile')}
                </Link>
                <button 
                  onClick={handleLogout}
                  className="hover:opacity-80 transition-opacity"
                >
                  {t('nav.logout')}
                </button>
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
      
      {/* Mobile Navigation */}
      <div className="md:hidden fixed bottom-0 inset-x-0 bg-ios-black text-white z-40">
        <div className="flex justify-around py-3">
          <Link 
            href="/" 
            className={`flex flex-col items-center ${
              router.pathname === '/' ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <span className="text-xs mt-1">{t('nav.feed')}</span>
          </Link>
          <Link 
            href="/opportunities" 
            className={`flex flex-col items-center ${
              router.pathname.startsWith('/opportunities') ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <span className="text-xs mt-1">{t('nav.opportunities')}</span>
          </Link>
          <Link 
            href="/success-stories" 
            className={`flex flex-col items-center ${
              router.pathname.startsWith('/success-stories') ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <span className="text-xs mt-1">{t('nav.successStories')}</span>
          </Link>
          <Link 
            href="/profile" 
            className={`flex flex-col items-center ${
              router.pathname.startsWith('/profile') ? 'opacity-100' : 'opacity-70'
            }`}
          >
            <span className="text-xs mt-1">{t('nav.profile')}</span>
          </Link>
        </div>
      </div>
      
      {/* Main Content */}
      <main className="flex-grow container mx-auto py-8 px-4 mb-16 md:mb-0">
        {children}
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