import React, { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'next-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { formatDistanceToNow } from 'date-fns';
import { getUserNotifications, markAsRead, dismissNotification, Notification } from '../../services/notificationService';
import { Badge } from '../common/Badge/Badge';

export const NotificationBell: React.FC = () => {
  const { t } = useTranslation('common');
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [isOpen, setIsOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  
  // Fetch notifications when component mounts
  useEffect(() => {
    fetchNotifications();
    
    // Poll for new notifications every minute
    const interval = setInterval(fetchNotifications, 60000);
    return () => clearInterval(interval);
  }, []);
  
  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);
  
  const fetchNotifications = async () => {
    try {
      setLoading(true);
      const data = await getUserNotifications();
      setNotifications(data);
      setUnreadCount(data.filter(n => n.readStatus === 'Unread').length);
    } catch (error) {
      console.error('Failed to fetch notifications:', error);
    } finally {
      setLoading(false);
    }
  };
  
  const handleMarkAsRead = async (id: string) => {
    try {
      await markAsRead(id);
      setNotifications(notifications.map(n => 
        n.id === id ? { ...n, readStatus: 'Read' } : n
      ));
      setUnreadCount(prev => Math.max(0, prev - 1));
    } catch (error) {
      console.error('Failed to mark notification as read:', error);
    }
  };
  
  const handleDismiss = async (id: string) => {
    try {
      await dismissNotification(id);
      setNotifications(notifications.map(n => 
        n.id === id ? { ...n, readStatus: 'Dismissed' } : n
      ));
      if (notifications.find(n => n.id === id)?.readStatus === 'Unread') {
        setUnreadCount(prev => Math.max(0, prev - 1));
      }
    } catch (error) {
      console.error('Failed to dismiss notification:', error);
    }
  };
  
  const toggleDropdown = () => {
    setIsOpen(prev => !prev);
  };
  
  const formatDate = (dateString: string) => {
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  return (
    <div className="relative" ref={dropdownRef}>
      <button 
        className="p-2 rounded-full hover:bg-gray-100 focus:outline-none relative"
        onClick={toggleDropdown}
        aria-label="Notifications"
      >
        <svg 
          xmlns="http://www.w3.org/2000/svg" 
          className="h-6 w-6 text-gray-600" 
          fill="none" 
          viewBox="0 0 24 24" 
          stroke="currentColor"
        >
          <path 
            strokeLinecap="round" 
            strokeLinejoin="round" 
            strokeWidth={2} 
            d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" 
          />
        </svg>
        
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 flex h-5 w-5">
            <Badge variant="danger" className="flex items-center justify-center">
              {unreadCount > 9 ? '9+' : unreadCount}
            </Badge>
          </span>
        )}
      </button>
      
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, y: 10, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 10, scale: 0.95 }}
            transition={{ duration: 0.2 }}
            className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg z-50 max-h-96 overflow-y-auto"
          >
            <div className="p-4 border-b border-gray-100">
              <h3 className="text-lg font-semibold">{t('notifications.title')}</h3>
            </div>
            
            {loading && notifications.length === 0 ? (
              <div className="p-4 text-center">
                <div className="inline-block h-6 w-6 border-2 border-gray-200 border-t-ios-black rounded-full animate-spin"></div>
              </div>
            ) : notifications.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                {t('notifications.empty')}
              </div>
            ) : (
              <div>
                {notifications
                  .filter(notification => notification.readStatus !== 'Dismissed')
                  .map(notification => (
                    <motion.div 
                      key={notification.id} 
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, height: 0, overflow: 'hidden' }}
                      transition={{ duration: 0.2 }}
                      className={`p-4 border-b border-gray-100 hover:bg-gray-50 ${
                        notification.readStatus === 'Unread' ? 'bg-blue-50' : ''
                      }`}
                    >
                      <div className="flex justify-between items-start">
                        <h4 className="font-semibold text-sm">{notification.title}</h4>
                        <div className="flex space-x-1">
                          <button 
                            onClick={() => handleMarkAsRead(notification.id)}
                            className="text-gray-400 hover:text-gray-600 p-1"
                            title={t('notifications.markAsRead')}
                          >
                            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                          </button>
                          <button 
                            onClick={() => handleDismiss(notification.id)}
                            className="text-gray-400 hover:text-gray-600 p-1"
                            title={t('notifications.dismiss')}
                          >
                            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                          </button>
                        </div>
                      </div>
                      <p className="text-sm text-gray-600 mt-1">{notification.content}</p>
                      <div className="text-xs text-gray-400 mt-2">{formatDate(notification.sentAt)}</div>
                    </motion.div>
                  ))}
              </div>
            )}
            
            <div className="p-3 text-center border-t border-gray-100">
              <Link href="/notifications" className="text-sm text-ios-black hover:underline">
                {t('notifications.viewAll')}
              </Link>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};