import React, { useState } from 'react';
import { useTranslation } from 'next-i18next';
import { Notification, broadcastNotification, createNotification } from '../../services/notificationService';

const NotificationPanel: React.FC = () => {
  const { t } = useTranslation('admin');
  const [notification, setNotification] = useState<Partial<Notification>>({
    title: '',
    content: '',
    type: 'SystemAnnouncement',
    targetAudience: {
      type: 'AllUsers'
    }
  });
  const [targetType, setTargetType] = useState('AllUsers');
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [userIds, setUserIds] = useState('');
  const [topics, setTopics] = useState('');
  const [regions, setRegions] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');
  
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setNotification({
      ...notification,
      [name]: value
    });
  };
  
  const handleRoleToggle = (role: string) => {
    if (selectedRoles.includes(role)) {
      setSelectedRoles(selectedRoles.filter(r => r !== role));
    } else {
      setSelectedRoles([...selectedRoles, role]);
    }
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setSuccess(false);
    setError('');
    
    try {
      // Prepare target audience based on selected type
      const audience: any = {
        type: targetType
      };
      
      if (targetType === 'SpecificUsers' && userIds) {
        audience.userIds = userIds.split(',').map(id => id.trim());
      } else if (targetType === 'ByRole' && selectedRoles.length > 0) {
        audience.targetRoles = selectedRoles;
      } else if (targetType === 'ByTopic' && topics) {
        audience.topics = topics.split(',').map(topic => topic.trim());
      } else if (targetType === 'ByRegion' && regions) {
        audience.regions = regions.split(',').map(region => region.trim());
      }
      
      // Update notification with audience
      const notificationToSend = {
        ...notification,
        targetAudience: audience
      };
      
      // Send notification (broadcast or targeted)
      if (targetType === 'AllUsers') {
        await broadcastNotification(notificationToSend);
      } else {
        await createNotification(notificationToSend);
      }
      
      setSuccess(true);
      // Reset form
      setNotification({
        title: '',
        content: '',
        type: 'SystemAnnouncement',
        targetAudience: {
          type: 'AllUsers'
        }
      });
      setTargetType('AllUsers');
      setSelectedRoles([]);
      setUserIds('');
      setTopics('');
      setRegions('');
    } catch (err) {
      setError(t('notifications.sendError'));
      console.error('Failed to send notification:', err);
    } finally {
      setLoading(false);
    }
  };
  
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">{t('notifications.title')}</h2>
      <p className="text-gray-600 mb-6">{t('notifications.description')}</p>
      
      {success && (
        <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-md text-green-700">
          {t('notifications.sendSuccess')}
        </div>
      )}
      
      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-md text-red-700">
          {error}
        </div>
      )}
      
      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {t('notifications.titleLabel')}
          </label>
          <input
            type="text"
            name="title"
            value={notification.title}
            onChange={handleChange}
            required
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
          />
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {t('notifications.contentLabel')}
          </label>
          <textarea
            name="content"
            value={notification.content}
            onChange={handleChange}
            required
            rows={4}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
          ></textarea>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {t('notifications.typeLabel')}
          </label>
          <select
            name="type"
            value={notification.type as string}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
          >
            <option value="SystemAnnouncement">{t('notifications.types.systemAnnouncement')}</option>
            <option value="EventReminder">{t('notifications.types.eventReminder')}</option>
            <option value="FundingOpportunity">{t('notifications.types.fundingOpportunity')}</option>
            <option value="VerificationUpdate">{t('notifications.types.verificationUpdate')}</option>
          </select>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {t('notifications.targetLabel')}
          </label>
          <select
            value={targetType}
            onChange={(e) => setTargetType(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
          >
            <option value="AllUsers">{t('notifications.targets.allUsers')}</option>
            <option value="SpecificUsers">{t('notifications.targets.specificUsers')}</option>
            <option value="ByRole">{t('notifications.targets.byRole')}</option>
            <option value="ByTopic">{t('notifications.targets.byTopic')}</option>
            <option value="ByRegion">{t('notifications.targets.byRegion')}</option>
          </select>
        </div>
        
        {/* Conditional fields based on target type */}
        {targetType === 'SpecificUsers' && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('notifications.userIdsLabel')}
            </label>
            <input
              type="text"
              value={userIds}
              onChange={(e) => setUserIds(e.target.value)}
              placeholder={t('notifications.userIdsPlaceholder')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
            />
            <p className="mt-1 text-sm text-gray-500">{t('notifications.userIdsHelp')}</p>
          </div>
        )}
        
        {targetType === 'ByRole' && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              {t('notifications.rolesLabel')}
            </label>
            <div className="space-y-2">
              {['Admin', 'Moderator', 'Activist', 'Organization'].map((role) => (
                <label key={role} className="flex items-center">
                  <input
                    type="checkbox"
                    checked={selectedRoles.includes(role)}
                    onChange={() => handleRoleToggle(role)}
                    className="h-4 w-4 text-ios-black focus:ring-ios-black border-gray-300 rounded"
                  />
                  <span className="ml-2 text-sm text-gray-700">{role}</span>
                </label>
              ))}
            </div>
          </div>
        )}
        
        {targetType === 'ByTopic' && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('notifications.topicsLabel')}
            </label>
            <input
              type="text"
              value={topics}
              onChange={(e) => setTopics(e.target.value)}
              placeholder={t('notifications.topicsPlaceholder')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
            />
            <p className="mt-1 text-sm text-gray-500">{t('notifications.topicsHelp')}</p>
          </div>
        )}
        
        {targetType === 'ByRegion' && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              {t('notifications.regionsLabel')}
            </label>
            <input
              type="text"
              value={regions}
              onChange={(e) => setRegions(e.target.value)}
              placeholder={t('notifications.regionsPlaceholder')}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
            />
            <p className="mt-1 text-sm text-gray-500">{t('notifications.regionsHelp')}</p>
          </div>
        )}
        
        <div>
          <button
            type="submit"
            disabled={loading}
            className="ios-button w-full flex justify-center items-center"
          >
            {loading ? (
              <span className="animate-pulse">{t('notifications.sending')}</span>
            ) : (
              t('notifications.sendButton')
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default NotificationPanel;