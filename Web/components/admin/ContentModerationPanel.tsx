import React, { useState, useEffect } from 'react';
import { useTranslation } from 'next-i18next';
import { Post } from '../../services/postService';

// This would be replaced with actual API calls in a real implementation
const mockReportedContent = [
  {
    id: 'post1',
    type: 'post',
    title: 'Climate Change Action',
    content: 'We need to take immediate action on climate change before it\'s too late.',
    authorName: 'EcoActivist',
    createdAt: '2025-04-25T14:30:00Z',
    reportCount: 2,
    reportReasons: ['misinformation'],
    status: 'reported'
  },
  {
    id: 'comment1',
    type: 'comment',
    content: 'This is completely wrong and misleading.',
    authorName: 'AngryUser42',
    createdAt: '2025-04-26T09:15:00Z',
    postId: 'post2',
    postTitle: 'Government Reform Proposal',
    reportCount: 5,
    reportReasons: ['harassment', 'offensive'],
    status: 'reported'
  },
  {
    id: 'post3',
    type: 'post',
    title: 'Join our protest tomorrow',
    content: 'We will be gathering at City Hall to protest the new regulations.',
    authorName: 'ActivistLeader',
    createdAt: '2025-04-27T16:45:00Z',
    reportCount: 1,
    reportReasons: ['spam'],
    status: 'reported'
  }
];

type ReportedContent = {
  id: string;
  type: 'post' | 'comment';
  title?: string;
  content: string;
  authorName: string;
  createdAt: string;
  postId?: string;
  postTitle?: string;
  reportCount: number;
  reportReasons: string[];
  status: 'reported' | 'approved' | 'rejected';
};

const ContentModerationPanel: React.FC = () => {
  const { t } = useTranslation('admin');
  const [reportedItems, setReportedItems] = useState<ReportedContent[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedItem, setSelectedItem] = useState<ReportedContent | null>(null);
  const [showDetails, setShowDetails] = useState(false);
  const [filterStatus, setFilterStatus] = useState('reported');
  const [filterType, setFilterType] = useState('all');
  
  useEffect(() => {
    // In a real application, this would be an API call
    const fetchReportedContent = async () => {
      try {
        setLoading(true);
        // Simulate API delay
        await new Promise(resolve => setTimeout(resolve, 800));
        setReportedItems(mockReportedContent);
        setError('');
      } catch (err) {
        setError(t('contentModeration.fetchError'));
        console.error('Error fetching reported content:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchReportedContent();
  }, [t]);
  
  const handleApproveContent = async (id: string) => {
    try {
      // In a real app, this would be an API call
      // await approveContent(id);
      
      // Update local state
      setReportedItems(prevItems => 
        prevItems.map(item => 
          item.id === id ? { ...item, status: 'approved' } : item
        )
      );
      
      if (selectedItem?.id === id) {
        setSelectedItem({ ...selectedItem, status: 'approved' });
      }
    } catch (err) {
      console.error('Error approving content:', err);
    }
  };
  
  const handleRejectContent = async (id: string) => {
    try {
      // In a real app, this would be an API call
      // await rejectContent(id);
      
      // Update local state
      setReportedItems(prevItems => 
        prevItems.map(item => 
          item.id === id ? { ...item, status: 'rejected' } : item
        )
      );
      
      if (selectedItem?.id === id) {
        setSelectedItem({ ...selectedItem, status: 'rejected' });
      }
    } catch (err) {
      console.error('Error rejecting content:', err);
    }
  };
  
  const handleShowDetails = (item: ReportedContent) => {
    setSelectedItem(item);
    setShowDetails(true);
  };
  
  const filteredItems = reportedItems.filter(item => {
    if (filterStatus !== 'all' && item.status !== filterStatus) return false;
    if (filterType !== 'all' && item.type !== filterType) return false;
    return true;
  });
  
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };
  
  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-pulse text-gray-500">{t('contentModeration.loading')}</div>
      </div>
    );
  }
  
  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-md text-red-700">
        {error}
      </div>
    );
  }
  
  return (
    <div>
      <h2 className="text-xl font-semibold mb-4">{t('contentModeration.title')}</h2>
      <p className="text-gray-600 mb-6">{t('contentModeration.description')}</p>
      
      <div className="mb-6 flex space-x-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {t('contentModeration.statusFilter')}
          </label>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
          >
            <option value="all">{t('contentModeration.allStatuses')}</option>
            <option value="reported">{t('contentModeration.reportedStatus')}</option>
            <option value="approved">{t('contentModeration.approvedStatus')}</option>
            <option value="rejected">{t('contentModeration.rejectedStatus')}</option>
          </select>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {t('contentModeration.typeFilter')}
          </label>
          <select
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
          >
            <option value="all">{t('contentModeration.allTypes')}</option>
            <option value="post">{t('contentModeration.postType')}</option>
            <option value="comment">{t('contentModeration.commentType')}</option>
          </select>
        </div>
      </div>
      
      {filteredItems.length === 0 ? (
        <div className="text-center py-12 text-gray-500">
          {t('contentModeration.noReportedContent')}
        </div>
      ) : (
        <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
          <table className="min-w-full divide-y divide-gray-300">
            <thead className="bg-gray-50">
              <tr>
                <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                  {t('contentModeration.contentColumn')}
                </th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                  {t('contentModeration.authorColumn')}
                </th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                  {t('contentModeration.reportsColumn')}
                </th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                  {t('contentModeration.statusColumn')}
                </th>
                <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                  <span className="sr-only">{t('contentModeration.actionsColumn')}</span>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredItems.map((item) => (
                <tr key={item.id}>
                  <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm sm:pl-6">
                    <div className="flex flex-col">
                      <div className="font-medium text-gray-900">
                        {item.type === 'post' ? item.title : t('contentModeration.commentOn', { post: item.postTitle })}
                      </div>
                      <div className="text-gray-500 truncate max-w-xs">
                        {item.content.length > 60 ? `${item.content.substring(0, 60)}...` : item.content}
                      </div>
                    </div>
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {item.authorName}
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                      {item.reportCount}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
                      ${item.status === 'reported' ? 'bg-yellow-100 text-yellow-800' : ''}
                      ${item.status === 'approved' ? 'bg-green-100 text-green-800' : ''}
                      ${item.status === 'rejected' ? 'bg-red-100 text-red-800' : ''}`
                    }>
                      {t(`contentModeration.${item.status}Status`)}
                    </span>
                  </td>
                  <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                    <button
                      onClick={() => handleShowDetails(item)}
                      className="text-indigo-600 hover:text-indigo-900 mr-4"
                    >
                      {t('contentModeration.viewDetails')}
                    </button>
                    {item.status === 'reported' && (
                      <>
                        <button
                          onClick={() => handleApproveContent(item.id)}
                          className="text-green-600 hover:text-green-900 mr-3"
                        >
                          {t('contentModeration.approve')}
                        </button>
                        <button
                          onClick={() => handleRejectContent(item.id)}
                          className="text-red-600 hover:text-red-900"
                        >
                          {t('contentModeration.reject')}
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      
      {/* Details Modal */}
      {showDetails && selectedItem && (
        <div className="fixed z-10 inset-0 overflow-y-auto">
          <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => setShowDetails(false)}></div>
            
            <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <div className="sm:flex sm:items-start">
                  <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                    <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
                      {selectedItem.type === 'post' ? 
                        t('contentModeration.postDetails') : 
                        t('contentModeration.commentDetails')}
                    </h3>
                    
                    <div className="mb-4">
                      {selectedItem.type === 'post' && (
                        <div className="mb-2">
                          <div className="text-sm font-medium text-gray-500">
                            {t('contentModeration.title')}
                          </div>
                          <div className="mt-1 text-sm text-gray-900">{selectedItem.title}</div>
                        </div>
                      )}
                      
                      <div className="mb-2">
                        <div className="text-sm font-medium text-gray-500">
                          {t('contentModeration.content')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">{selectedItem.content}</div>
                      </div>
                      
                      <div className="mb-2">
                        <div className="text-sm font-medium text-gray-500">
                          {t('contentModeration.author')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">{selectedItem.authorName}</div>
                      </div>
                      
                      <div className="mb-2">
                        <div className="text-sm font-medium text-gray-500">
                          {t('contentModeration.createdAt')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">{formatDate(selectedItem.createdAt)}</div>
                      </div>
                      
                      {selectedItem.type === 'comment' && (
                        <div className="mb-2">
                          <div className="text-sm font-medium text-gray-500">
                            {t('contentModeration.parentPost')}
                          </div>
                          <div className="mt-1 text-sm text-gray-900">{selectedItem.postTitle}</div>
                        </div>
                      )}
                      
                      <div className="mb-2">
                        <div className="text-sm font-medium text-gray-500">
                          {t('contentModeration.reportCount')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">{selectedItem.reportCount}</div>
                      </div>
                      
                      <div className="mb-2">
                        <div className="text-sm font-medium text-gray-500">
                          {t('contentModeration.reportReasons')}
                        </div>
                        <div className="mt-1 flex flex-wrap gap-1">
                          {selectedItem.reportReasons.map(reason => (
                            <span 
                              key={reason} 
                              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                            >
                              {t(`contentModeration.reasons.${reason}`)}
                            </span>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                <button
                  type="button"
                  onClick={() => setShowDetails(false)}
                  className="ios-button-secondary w-full inline-flex justify-center sm:ml-3 sm:w-auto sm:text-sm"
                >
                  {t('contentModeration.close')}
                </button>
                {selectedItem.status === 'reported' && (
                  <>
                    <button
                      type="button"
                      onClick={() => {
                        handleRejectContent(selectedItem.id);
                        setShowDetails(false);
                      }}
                      className="ios-button-danger mt-3 w-full inline-flex justify-center sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                    >
                      {t('contentModeration.reject')}
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        handleApproveContent(selectedItem.id);
                        setShowDetails(false);
                      }}
                      className="ios-button mt-3 w-full inline-flex justify-center sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                    >
                      {t('contentModeration.approve')}
                    </button>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ContentModerationPanel;