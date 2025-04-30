import React, { useState, useEffect } from 'react';
import { useTranslation } from 'next-i18next';

// Mock data for analytics - in a real implementation this would come from API calls
const mockAnalyticsData = {
  overview: {
    totalUsers: 8724,
    activeUsers: 3152,
    totalPosts: 12890,
    totalComments: 34567,
    pendingContentReports: 17,
    pendingOrganizations: 5
  },
  userGrowth: [
    { date: '2025-01-01', count: 7200 },
    { date: '2025-02-01', count: 7550 },
    { date: '2025-03-01', count: 7980 },
    { date: '2025-04-01', count: 8450 },
    { date: '2025-04-30', count: 8724 }
  ],
  engagementMetrics: {
    averageCommentsPerPost: 2.7,
    averageLikesPerPost: 15.3,
    postsByCategory: {
      'activism': 45,
      'environment': 25,
      'human-rights': 15,
      'education': 10,
      'other': 5
    },
    topPosts: [
      { id: 'post1', title: 'Climate Change Rally', likes: 342, comments: 87 },
      { id: 'post2', title: 'Education Reform Petition', likes: 256, comments: 42 },
      { id: 'post3', title: 'Human Rights Watch Report', likes: 189, comments: 63 }
    ]
  },
  geographicDistribution: {
    'North America': 35,
    'Europe': 30,
    'Asia': 20,
    'Africa': 10,
    'South America': 3,
    'Australia': 2
  }
};

const AnalyticsPanel: React.FC = () => {
  const { t } = useTranslation('admin');
  const [analyticsData, setAnalyticsData] = useState(mockAnalyticsData);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [timeRange, setTimeRange] = useState('month');
  
  useEffect(() => {
    // In a real application, this would be an API call with the selected time range
    const fetchAnalyticsData = async () => {
      try {
        setLoading(true);
        // Simulate API delay
        await new Promise(resolve => setTimeout(resolve, 800));
        setAnalyticsData(mockAnalyticsData);
        setError('');
      } catch (err) {
        setError(t('analytics.fetchError'));
        console.error('Error fetching analytics data:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchAnalyticsData();
  }, [timeRange, t]);
  
  const formatNumber = (num: number) => {
    return new Intl.NumberFormat().format(num);
  };
  
  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-pulse text-gray-500">{t('analytics.loading')}</div>
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
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-xl font-semibold">{t('analytics.title')}</h2>
          <p className="text-gray-600">{t('analytics.description')}</p>
        </div>
        
        <div>
          <select
            value={timeRange}
            onChange={(e) => setTimeRange(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
          >
            <option value="week">{t('analytics.timeRanges.week')}</option>
            <option value="month">{t('analytics.timeRanges.month')}</option>
            <option value="quarter">{t('analytics.timeRanges.quarter')}</option>
            <option value="year">{t('analytics.timeRanges.year')}</option>
            <option value="all">{t('analytics.timeRanges.all')}</option>
          </select>
        </div>
      </div>
      
      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 bg-indigo-500 rounded-md p-3">
                <svg className="h-6 w-6 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    {t('analytics.totalUsers')}
                  </dt>
                  <dd>
                    <div className="text-lg font-medium text-gray-900">
                      {formatNumber(analyticsData.overview.totalUsers)}
                    </div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
          <div className="bg-gray-50 px-4 py-4 sm:px-6">
            <div className="text-sm">
              <span className="font-medium text-green-600">
                {formatNumber(analyticsData.overview.activeUsers)}
              </span>
              <span className="text-gray-500 ml-1">
                {t('analytics.activeUsers')}
              </span>
            </div>
          </div>
        </div>
        
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 bg-green-500 rounded-md p-3">
                <svg className="h-6 w-6 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    {t('analytics.totalPosts')}
                  </dt>
                  <dd>
                    <div className="text-lg font-medium text-gray-900">
                      {formatNumber(analyticsData.overview.totalPosts)}
                    </div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
          <div className="bg-gray-50 px-4 py-4 sm:px-6">
            <div className="text-sm">
              <span className="font-medium text-indigo-600">
                {formatNumber(analyticsData.overview.totalComments)}
              </span>
              <span className="text-gray-500 ml-1">
                {t('analytics.totalComments')}
              </span>
            </div>
          </div>
        </div>
        
        <div className="bg-white overflow-hidden shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0 bg-yellow-500 rounded-md p-3">
                <svg className="h-6 w-6 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>
              <div className="ml-5 w-0 flex-1">
                <dl>
                  <dt className="text-sm font-medium text-gray-500 truncate">
                    {t('analytics.pendingReports')}
                  </dt>
                  <dd>
                    <div className="text-lg font-medium text-gray-900">
                      {formatNumber(analyticsData.overview.pendingContentReports)}
                    </div>
                  </dd>
                </dl>
              </div>
            </div>
          </div>
          <div className="bg-gray-50 px-4 py-4 sm:px-6">
            <div className="text-sm">
              <span className="font-medium text-orange-600">
                {formatNumber(analyticsData.overview.pendingOrganizations)}
              </span>
              <span className="text-gray-500 ml-1">
                {t('analytics.pendingOrganizations')}
              </span>
            </div>
          </div>
        </div>
      </div>
      
      {/* Engagement Metrics */}
      <div className="bg-white shadow rounded-lg mb-8">
        <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
          <h3 className="text-lg font-medium leading-6 text-gray-900">
            {t('analytics.engagementTitle')}
          </h3>
        </div>
        <div className="px-4 py-5 sm:p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h4 className="text-base font-medium text-gray-900 mb-4">
                {t('analytics.postsByCategory')}
              </h4>
              <div className="space-y-4">
                {Object.entries(analyticsData.engagementMetrics.postsByCategory).map(([category, percentage]) => (
                  <div key={category}>
                    <div className="flex items-center justify-between">
                      <div className="text-sm font-medium text-gray-900">
                        {t(`analytics.categories.${category}`)}
                      </div>
                      <div className="text-sm text-gray-500">{percentage}%</div>
                    </div>
                    <div className="mt-1 relative">
                      <div className="overflow-hidden h-2 text-xs flex rounded bg-gray-200">
                        <div 
                          style={{ width: `${percentage}%` }} 
                          className="shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center bg-indigo-500"
                        ></div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
            
            <div>
              <h4 className="text-base font-medium text-gray-900 mb-4">
                {t('analytics.topPosts')}
              </h4>
              <div className="overflow-hidden">
                <ul className="divide-y divide-gray-200">
                  {analyticsData.engagementMetrics.topPosts.map((post) => (
                    <li key={post.id} className="py-3">
                      <div className="flex items-center justify-between">
                        <div className="text-sm font-medium text-gray-900 truncate max-w-xs">
                          {post.title}
                        </div>
                        <div className="ml-2 flex-shrink-0 flex">
                          <div className="flex items-center text-sm text-gray-500 mr-3">
                            <svg className="h-4 w-4 mr-1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                              <path d="M2 10.5a1.5 1.5 0 113 0v6a1.5 1.5 0 01-3 0v-6zM6 10.333v5.43a2 2 0 001.106 1.79l.05.025A4 4 0 008.943 18h5.416a2 2 0 001.962-1.608l1.2-6A2 2 0 0015.56 8H12V4a2 2 0 00-2-2 1 1 0 00-1 1v.667a4 4 0 01-.8 2.4L6.8 7.933a4 4 0 00-.8 2.4z" />
                            </svg>
                            {post.likes}
                          </div>
                          <div className="flex items-center text-sm text-gray-500">
                            <svg className="h-4 w-4 mr-1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                              <path fillRule="evenodd" d="M18 5v8a2 2 0 01-2 2h-5l-5 4v-4H4a2 2 0 01-2-2V5a2 2 0 012-2h12a2 2 0 012 2zM7 8H5v2h2V8zm2 0h2v2H9V8zm6 0h-2v2h2V8z" clipRule="evenodd" />
                            </svg>
                            {post.comments}
                          </div>
                        </div>
                      </div>
                    </li>
                  ))}
                </ul>
              </div>
            </div>
          </div>
          
          <div className="mt-6 text-center text-sm">
            <div className="inline-flex rounded-md shadow">
              <a
                href="#"
                className="ios-button-secondary"
              >
                {t('analytics.viewDetailedReports')}
              </a>
            </div>
          </div>
        </div>
      </div>
      
      {/* Geographic Distribution */}
      <div className="bg-white shadow rounded-lg">
        <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
          <h3 className="text-lg font-medium leading-6 text-gray-900">
            {t('analytics.geographicTitle')}
          </h3>
        </div>
        <div className="px-4 py-5 sm:p-6">
          <div className="space-y-4">
            {Object.entries(analyticsData.geographicDistribution).map(([region, percentage]) => (
              <div key={region}>
                <div className="flex items-center justify-between">
                  <div className="text-sm font-medium text-gray-900">
                    {region}
                  </div>
                  <div className="text-sm text-gray-500">{percentage}%</div>
                </div>
                <div className="mt-1 relative">
                  <div className="overflow-hidden h-2 text-xs flex rounded bg-gray-200">
                    <div 
                      style={{ width: `${percentage}%` }} 
                      className="shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center bg-green-500"
                    ></div>
                  </div>
                </div>
              </div>
            ))}
          </div>
          
          <div className="mt-6 text-center text-sm text-gray-500">
            {t('analytics.geographicNote')}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AnalyticsPanel;