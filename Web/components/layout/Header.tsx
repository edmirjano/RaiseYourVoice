import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/router';
import { motion, AnimatePresence } from 'framer-motion';
import { useTranslation } from 'next-i18next';
import { useAuth } from '../../contexts/AuthContext';
import { SearchBar } from '../search/SearchBar';
import { NotificationBell } from '../notifications/NotificationBell';
import { Avatar } from '../common/Avatar/Avatar';
import { Dropdown, DropdownItem } from '../common/Dropdown/Dropdown';

export const Header: React.FC = () => {
  const { t } = useTranslation('common');
  const router = useRouter();
  const { user, isAuthenticated, logout } = useAuth();
  const [isScrolled, setIsScrolled] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  // Track scroll position to add shadow to header
  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 10);
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const handleLogout = () => {
    logout();
    router.push('/');
  };

  const isActive = (path: string) => {
    if (path === '/') {
      return router.pathname === '/';
    }
    return router.pathname.startsWith(path);
  };

  return (
    <header
      className={`sticky top-0 z-50 bg-white ${
        isScrolled ? 'shadow-md' : ''
      } transition-shadow duration-300`}
    >
      <div className="container mx-auto px-4">
        <div className="flex justify-between items-center py-4">
          {/* Logo */}
          <Link href="/" className="flex items-center">
            <span className="text-xl font-bold text-ios-black">RaiseYourVoice</span>
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center space-x-8">
            <Link
              href="/feed"
              className={`text-sm font-medium ${
                isActive('/feed')
                  ? 'text-ios-black'
                  : 'text-gray-600 hover:text-ios-black'
              }`}
            >
              {t('nav.feed')}
            </Link>
            <Link
              href="/opportunities"
              className={`text-sm font-medium ${
                isActive('/opportunities')
                  ? 'text-ios-black'
                  : 'text-gray-600 hover:text-ios-black'
              }`}
            >
              {t('nav.opportunities')}
            </Link>
            <Link
              href="/success-stories"
              className={`text-sm font-medium ${
                isActive('/success-stories')
                  ? 'text-ios-black'
                  : 'text-gray-600 hover:text-ios-black'
              }`}
            >
              {t('nav.successStories')}
            </Link>
            <Link
              href="/campaigns"
              className={`text-sm font-medium ${
                isActive('/campaigns')
                  ? 'text-ios-black'
                  : 'text-gray-600 hover:text-ios-black'
              }`}
            >
              {t('nav.campaigns')}
            </Link>
            <Link
              href="/organizations"
              className={`text-sm font-medium ${
                isActive('/organizations')
                  ? 'text-ios-black'
                  : 'text-gray-600 hover:text-ios-black'
              }`}
            >
              {t('nav.organizations')}
            </Link>
          </div>

          {/* Right Section: Search, Notifications, User Menu */}
          <div className="flex items-center space-x-4">
            {/* Search (Desktop) */}
            <div className="hidden md:block w-64">
              <SearchBar />
            </div>

            {/* User Menu or Auth Buttons */}
            {isAuthenticated ? (
              <div className="flex items-center space-x-4">
                {/* Notification Bell */}
                <NotificationBell />

                {/* User Menu */}
                <Dropdown
                  trigger={
                    <button className="flex items-center space-x-1">
                      <Avatar
                        src={user?.profilePicture}
                        name={user?.name}
                        size="sm"
                      />
                      <svg
                        className="h-4 w-4 text-gray-500"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={2}
                          d="M19 9l-7 7-7-7"
                        />
                      </svg>
                    </button>
                  }
                  align="right"
                  width="w-48"
                >
                  <div className="py-1 border-b border-gray-100">
                    <div className="px-4 py-2">
                      <p className="text-sm font-medium text-gray-900 truncate">
                        {user?.name}
                      </p>
                      <p className="text-xs text-gray-500 truncate">
                        {user?.email}
                      </p>
                    </div>
                  </div>
                  <div className="py-1">
                    <DropdownItem
                      onClick={() => router.push('/profile')}
                    >
                      {t('nav.profile')}
                    </DropdownItem>
                    {(user?.role === 'Admin' || user?.role === 'Moderator') && (
                      <DropdownItem
                        onClick={() => router.push('/admin/dashboard')}
                      >
                        {t('nav.adminDashboard')}
                      </DropdownItem>
                    )}
                    <DropdownItem onClick={handleLogout}>
                      {t('nav.logout')}
                    </DropdownItem>
                  </div>
                </Dropdown>
              </div>
            ) : (
              <div className="flex items-center space-x-2">
                <Link
                  href="/auth/login"
                  className="text-sm font-medium text-gray-600 hover:text-ios-black"
                >
                  {t('nav.login')}
                </Link>
                <Link
                  href="/auth/register"
                  className="ios-button text-sm"
                >
                  {t('nav.register')}
                </Link>
              </div>
            )}

            {/* Mobile Menu Button */}
            <button
              className="md:hidden"
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            >
              <svg
                className="h-6 w-6 text-ios-black"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                {isMobileMenuOpen ? (
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M6 18L18 6M6 6l12 12"
                  />
                ) : (
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M4 6h16M4 12h16M4 18h16"
                  />
                )}
              </svg>
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Search Bar */}
      <div className="md:hidden border-t border-gray-100 px-4 py-2">
        <SearchBar />
      </div>

      {/* Mobile Menu */}
      <AnimatePresence>
        {isMobileMenuOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            transition={{ duration: 0.2 }}
            className="md:hidden bg-white border-t border-gray-100"
          >
            <div className="px-4 py-2 space-y-1">
              <Link
                href="/feed"
                className={`block py-2 text-base font-medium ${
                  isActive('/feed')
                    ? 'text-ios-black'
                    : 'text-gray-600 hover:text-ios-black'
                }`}
                onClick={() => setIsMobileMenuOpen(false)}
              >
                {t('nav.feed')}
              </Link>
              <Link
                href="/opportunities"
                className={`block py-2 text-base font-medium ${
                  isActive('/opportunities')
                    ? 'text-ios-black'
                    : 'text-gray-600 hover:text-ios-black'
                }`}
                onClick={() => setIsMobileMenuOpen(false)}
              >
                {t('nav.opportunities')}
              </Link>
              <Link
                href="/success-stories"
                className={`block py-2 text-base font-medium ${
                  isActive('/success-stories')
                    ? 'text-ios-black'
                    : 'text-gray-600 hover:text-ios-black'
                }`}
                onClick={() => setIsMobileMenuOpen(false)}
              >
                {t('nav.successStories')}
              </Link>
              <Link
                href="/campaigns"
                className={`block py-2 text-base font-medium ${
                  isActive('/campaigns')
                    ? 'text-ios-black'
                    : 'text-gray-600 hover:text-ios-black'
                }`}
                onClick={() => setIsMobileMenuOpen(false)}
              >
                {t('nav.campaigns')}
              </Link>
              <Link
                href="/organizations"
                className={`block py-2 text-base font-medium ${
                  isActive('/organizations')
                    ? 'text-ios-black'
                    : 'text-gray-600 hover:text-ios-black'
                }`}
                onClick={() => setIsMobileMenuOpen(false)}
              >
                {t('nav.organizations')}
              </Link>

              {isAuthenticated ? (
                <>
                  <Link
                    href="/profile"
                    className={`block py-2 text-base font-medium ${
                      isActive('/profile')
                        ? 'text-ios-black'
                        : 'text-gray-600 hover:text-ios-black'
                    }`}
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    {t('nav.profile')}
                  </Link>
                  {(user?.role === 'Admin' || user?.role === 'Moderator') && (
                    <Link
                      href="/admin/dashboard"
                      className={`block py-2 text-base font-medium ${
                        isActive('/admin')
                          ? 'text-ios-black'
                          : 'text-gray-600 hover:text-ios-black'
                      }`}
                      onClick={() => setIsMobileMenuOpen(false)}
                    >
                      {t('nav.adminDashboard')}
                    </Link>
                  )}
                  <button
                    onClick={() => {
                      handleLogout();
                      setIsMobileMenuOpen(false);
                    }}
                    className="block w-full text-left py-2 text-base font-medium text-gray-600 hover:text-ios-black"
                  >
                    {t('nav.logout')}
                  </button>
                </>
              ) : (
                <div className="pt-4 pb-2 border-t border-gray-100">
                  <Link
                    href="/auth/login"
                    className="block w-full py-2 text-center ios-button-secondary"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    {t('nav.login')}
                  </Link>
                  <Link
                    href="/auth/register"
                    className="block w-full py-2 mt-2 text-center ios-button"
                    onClick={() => setIsMobileMenuOpen(false)}
                  >
                    {t('nav.register')}
                  </Link>
                </div>
              )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </header>
  );
};