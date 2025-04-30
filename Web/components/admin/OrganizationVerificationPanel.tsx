import React, { useState, useEffect } from 'react';
import { useTranslation } from 'next-i18next';

// Mock data - in a real implementation this would come from an API
const mockOrganizations = [
  {
    id: 'org1',
    name: 'Environmental Action Coalition',
    description: 'Dedicated to protecting the environment through advocacy and action.',
    logoUrl: '/images/orgs/eac-logo.png',
    website: 'https://enviroaction.org',
    contactEmail: 'info@enviroaction.org',
    verificationStatus: 'pending',
    submittedAt: '2025-04-20T15:30:00Z',
    operatingRegions: ['Europe', 'North America'],
    organizationType: 'Non-profit',
    registrationNumber: 'NP-93824-721',
    foundingDate: '2018-03-15T00:00:00Z'
  },
  {
    id: 'org2',
    name: 'Youth for Democracy',
    description: 'Empowering young people to participate in democratic processes.',
    logoUrl: '/images/orgs/yfd-logo.png',
    website: 'https://youthfordemocracy.org',
    contactEmail: 'info@youthfordemocracy.org',
    verificationStatus: 'pending',
    submittedAt: '2025-04-22T09:45:00Z',
    operatingRegions: ['Global'],
    organizationType: 'NGO',
    registrationNumber: 'NGO-47392-185',
    foundingDate: '2020-01-10T00:00:00Z'
  },
  {
    id: 'org3',
    name: 'Human Rights Watch Group',
    description: 'Monitoring and advocating for human rights across the globe.',
    logoUrl: '/images/orgs/hrwg-logo.png',
    website: 'https://hrwg.org',
    contactEmail: 'contact@hrwg.org',
    verificationStatus: 'verified',
    submittedAt: '2025-03-12T11:20:00Z',
    verifiedAt: '2025-03-15T14:30:00Z',
    operatingRegions: ['Africa', 'Asia', 'Middle East'],
    organizationType: 'Advocacy Group',
    registrationNumber: 'AG-58291-342',
    foundingDate: '2010-11-05T00:00:00Z'
  }
];

type Organization = {
  id: string;
  name: string;
  description: string;
  logoUrl: string;
  website: string;
  contactEmail: string;
  verificationStatus: 'pending' | 'verified' | 'rejected';
  submittedAt: string;
  verifiedAt?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  operatingRegions: string[];
  organizationType: string;
  registrationNumber: string;
  foundingDate: string;
  documentsUrls?: string[];
};

const OrganizationVerificationPanel: React.FC = () => {
  const { t } = useTranslation('admin');
  const [organizations, setOrganizations] = useState<Organization[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedOrg, setSelectedOrg] = useState<Organization | null>(null);
  const [showDetails, setShowDetails] = useState(false);
  const [filterStatus, setFilterStatus] = useState('pending');
  const [rejectionReason, setRejectionReason] = useState('');
  
  useEffect(() => {
    // In a real application, this would be an API call
    const fetchOrganizations = async () => {
      try {
        setLoading(true);
        // Simulate API delay
        await new Promise(resolve => setTimeout(resolve, 800));
        setOrganizations(mockOrganizations);
        setError('');
      } catch (err) {
        setError(t('organizationVerification.fetchError'));
        console.error('Error fetching organizations:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchOrganizations();
  }, [t]);
  
  const handleVerifyOrganization = async (id: string) => {
    try {
      // In a real app, this would be an API call
      // await verifyOrganization(id);
      
      // Update local state
      setOrganizations(prevOrgs => 
        prevOrgs.map(org => 
          org.id === id ? { 
            ...org, 
            verificationStatus: 'verified',
            verifiedAt: new Date().toISOString()
          } : org
        )
      );
      
      if (selectedOrg?.id === id) {
        setSelectedOrg({ 
          ...selectedOrg, 
          verificationStatus: 'verified',
          verifiedAt: new Date().toISOString()
        });
      }
    } catch (err) {
      console.error('Error verifying organization:', err);
    }
  };
  
  const handleRejectOrganization = async (id: string, reason: string) => {
    if (!reason.trim()) {
      alert(t('organizationVerification.rejectionReasonRequired'));
      return;
    }
    
    try {
      // In a real app, this would be an API call
      // await rejectOrganization(id, reason);
      
      // Update local state
      setOrganizations(prevOrgs => 
        prevOrgs.map(org => 
          org.id === id ? { 
            ...org, 
            verificationStatus: 'rejected',
            rejectedAt: new Date().toISOString(),
            rejectionReason: reason
          } : org
        )
      );
      
      if (selectedOrg?.id === id) {
        setSelectedOrg({ 
          ...selectedOrg, 
          verificationStatus: 'rejected',
          rejectedAt: new Date().toISOString(),
          rejectionReason: reason
        });
      }
      
      setRejectionReason('');
    } catch (err) {
      console.error('Error rejecting organization:', err);
    }
  };
  
  const handleShowDetails = (org: Organization) => {
    setSelectedOrg(org);
    setShowDetails(true);
  };
  
  const filteredOrganizations = organizations.filter(org => {
    if (filterStatus !== 'all' && org.verificationStatus !== filterStatus) return false;
    return true;
  });
  
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };
  
  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-pulse text-gray-500">{t('organizationVerification.loading')}</div>
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
      <h2 className="text-xl font-semibold mb-4">{t('organizationVerification.title')}</h2>
      <p className="text-gray-600 mb-6">{t('organizationVerification.description')}</p>
      
      <div className="mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          {t('organizationVerification.statusFilter')}
        </label>
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
          className="px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-ios-black focus:border-ios-black"
        >
          <option value="all">{t('organizationVerification.allStatuses')}</option>
          <option value="pending">{t('organizationVerification.pendingStatus')}</option>
          <option value="verified">{t('organizationVerification.verifiedStatus')}</option>
          <option value="rejected">{t('organizationVerification.rejectedStatus')}</option>
        </select>
      </div>
      
      {filteredOrganizations.length === 0 ? (
        <div className="text-center py-12 text-gray-500">
          {t('organizationVerification.noOrganizations')}
        </div>
      ) : (
        <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
          <table className="min-w-full divide-y divide-gray-300">
            <thead className="bg-gray-50">
              <tr>
                <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                  {t('organizationVerification.organizationColumn')}
                </th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                  {t('organizationVerification.typeColumn')}
                </th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                  {t('organizationVerification.submittedColumn')}
                </th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                  {t('organizationVerification.statusColumn')}
                </th>
                <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                  <span className="sr-only">{t('organizationVerification.actionsColumn')}</span>
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {filteredOrganizations.map((org) => (
                <tr key={org.id}>
                  <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm sm:pl-6">
                    <div className="flex items-center">
                      <div className="h-10 w-10 flex-shrink-0">
                        <img className="h-10 w-10 rounded-full" src={org.logoUrl || '/images/placeholder-org.png'} alt="" />
                      </div>
                      <div className="ml-4">
                        <div className="font-medium text-gray-900">{org.name}</div>
                        <div className="text-gray-500 truncate max-w-xs">{org.description}</div>
                      </div>
                    </div>
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {org.organizationType}
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {formatDate(org.submittedAt)}
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
                      ${org.verificationStatus === 'pending' ? 'bg-yellow-100 text-yellow-800' : ''}
                      ${org.verificationStatus === 'verified' ? 'bg-green-100 text-green-800' : ''}
                      ${org.verificationStatus === 'rejected' ? 'bg-red-100 text-red-800' : ''}`
                    }>
                      {t(`organizationVerification.${org.verificationStatus}Status`)}
                    </span>
                  </td>
                  <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                    <button
                      onClick={() => handleShowDetails(org)}
                      className="text-indigo-600 hover:text-indigo-900 mr-4"
                    >
                      {t('organizationVerification.viewDetails')}
                    </button>
                    {org.verificationStatus === 'pending' && (
                      <>
                        <button
                          onClick={() => handleVerifyOrganization(org.id)}
                          className="text-green-600 hover:text-green-900 mr-3"
                        >
                          {t('organizationVerification.verify')}
                        </button>
                        <button
                          onClick={() => {
                            setSelectedOrg(org);
                            setShowDetails(true);
                            // Focus the rejection form
                            setTimeout(() => {
                              document.getElementById('rejection-reason')?.focus();
                            }, 100);
                          }}
                          className="text-red-600 hover:text-red-900"
                        >
                          {t('organizationVerification.reject')}
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
      {showDetails && selectedOrg && (
        <div className="fixed z-10 inset-0 overflow-y-auto">
          <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={() => setShowDetails(false)}></div>
            
            <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <div className="sm:flex sm:items-start">
                  <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                    <div className="flex items-center mb-4">
                      <img 
                        src={selectedOrg.logoUrl || '/images/placeholder-org.png'} 
                        alt={selectedOrg.name} 
                        className="h-12 w-12 rounded-full mr-4"
                      />
                      <h3 className="text-lg leading-6 font-medium text-gray-900">
                        {selectedOrg.name}
                      </h3>
                    </div>
                    
                    <div className="grid grid-cols-1 gap-4 mb-6">
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.description')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">{selectedOrg.description}</div>
                      </div>
                      
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.website')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">
                          <a href={selectedOrg.website} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline">
                            {selectedOrg.website}
                          </a>
                        </div>
                      </div>
                      
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.contactEmail')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">
                          <a href={`mailto:${selectedOrg.contactEmail}`} className="text-blue-600 hover:underline">
                            {selectedOrg.contactEmail}
                          </a>
                        </div>
                      </div>
                      
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.organizationType')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">{selectedOrg.organizationType}</div>
                      </div>
                      
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.registrationNumber')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">{selectedOrg.registrationNumber}</div>
                      </div>
                      
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.foundingDate')}
                        </div>
                        <div className="mt-1 text-sm text-gray-900">
                          {new Date(selectedOrg.foundingDate).toLocaleDateString()}
                        </div>
                      </div>
                      
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.operatingRegions')}
                        </div>
                        <div className="mt-1 flex flex-wrap gap-1">
                          {selectedOrg.operatingRegions.map(region => (
                            <span 
                              key={region} 
                              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                            >
                              {region}
                            </span>
                          ))}
                        </div>
                      </div>
                      
                      <div>
                        <div className="text-sm font-medium text-gray-500">
                          {t('organizationVerification.status')}
                        </div>
                        <div className="mt-1 text-sm">
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
                            ${selectedOrg.verificationStatus === 'pending' ? 'bg-yellow-100 text-yellow-800' : ''}
                            ${selectedOrg.verificationStatus === 'verified' ? 'bg-green-100 text-green-800' : ''}
                            ${selectedOrg.verificationStatus === 'rejected' ? 'bg-red-100 text-red-800' : ''}`
                          }>
                            {t(`organizationVerification.${selectedOrg.verificationStatus}Status`)}
                          </span>
                        </div>
                      </div>
                      
                      {selectedOrg.verificationStatus === 'rejected' && selectedOrg.rejectionReason && (
                        <div>
                          <div className="text-sm font-medium text-gray-500">
                            {t('organizationVerification.rejectionReason')}
                          </div>
                          <div className="mt-1 text-sm text-red-600">{selectedOrg.rejectionReason}</div>
                        </div>
                      )}
                    </div>
                    
                    {selectedOrg.verificationStatus === 'pending' && (
                      <div className="mt-4">
                        <label htmlFor="rejection-reason" className="block text-sm font-medium text-gray-700">
                          {t('organizationVerification.rejectionReason')}
                        </label>
                        <div className="mt-1">
                          <textarea
                            id="rejection-reason"
                            name="rejection-reason"
                            rows={3}
                            value={rejectionReason}
                            onChange={(e) => setRejectionReason(e.target.value)}
                            className="shadow-sm focus:ring-ios-black focus:border-ios-black block w-full sm:text-sm border-gray-300 rounded-md"
                            placeholder={t('organizationVerification.rejectionReasonPlaceholder')}
                          />
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                <button
                  type="button"
                  onClick={() => setShowDetails(false)}
                  className="ios-button-secondary w-full inline-flex justify-center sm:ml-3 sm:w-auto sm:text-sm"
                >
                  {t('organizationVerification.close')}
                </button>
                
                {selectedOrg.verificationStatus === 'pending' && (
                  <>
                    <button
                      type="button"
                      onClick={() => {
                        handleRejectOrganization(selectedOrg.id, rejectionReason);
                        setShowDetails(false);
                      }}
                      className="ios-button-danger mt-3 w-full inline-flex justify-center sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                      disabled={!rejectionReason.trim()}
                    >
                      {t('organizationVerification.reject')}
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        handleVerifyOrganization(selectedOrg.id);
                        setShowDetails(false);
                      }}
                      className="ios-button mt-3 w-full inline-flex justify-center sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                    >
                      {t('organizationVerification.verify')}
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

export default OrganizationVerificationPanel;