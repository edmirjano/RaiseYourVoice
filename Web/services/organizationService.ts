import apiClient, { handleApiError } from './apiClient';

export type Organization = {
  id?: string;
  name: string;
  description: string;
  logoUrl?: string;
  website?: string;
  contactInfo?: {
    email: string;
    phone: string;
    contactPersonName: string;
    contactPersonRole?: string;
  };
  socialMediaLinks?: {
    facebook?: string;
    twitter?: string;
    instagram?: string;
    linkedin?: string;
    youtube?: string;
    tiktok?: string;
  };
  location?: {
    address?: string;
    city?: string;
    country?: string;
    latitude?: number;
    longitude?: number;
  };
  verificationStatus?: 'Pending' | 'Verified' | 'Rejected';
  verifiedBy?: string;
  verificationDate?: string;
  foundingDate?: string;
  missionStatement?: string;
  visionStatement?: string;
  organizationType?: string;
  operatingRegions?: string[];
  teamMembers?: Array<{
    id: string;
    name: string;
    title: string;
    bio?: string;
    photoUrl?: string;
  }>;
  pastProjects?: Array<{
    id: string;
    title: string;
    description: string;
    startDate: string;
    endDate?: string;
    impactDescription?: string;
    imageUrls?: string[];
  }>;
  impactMetrics?: {
    peopleHelped: number;
    areasCovered: number;
    volunteerHours: number;
    customMetrics?: Record<string, string>;
  };
  createdAt?: string;
  updatedAt?: string;
};

/**
 * Get all organizations
 */
export const getOrganizations = async (page: number = 1, pageSize: number = 10): Promise<Organization[]> => {
  try {
    const response = await apiClient.get<Organization[]>('/organizations', {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching organizations:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get verified organizations
 */
export const getVerifiedOrganizations = async (page: number = 1, pageSize: number = 10): Promise<Organization[]> => {
  try {
    const response = await apiClient.get<Organization[]>('/organizations/verified', {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching verified organizations:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get organization by ID
 */
export const getOrganizationById = async (id: string): Promise<Organization> => {
  try {
    const response = await apiClient.get<Organization>(`/organizations/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching organization ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a new organization
 */
export const createOrganization = async (organization: Organization): Promise<Organization> => {
  try {
    const response = await apiClient.post<Organization>('/organizations', organization);
    return response.data;
  } catch (error) {
    console.error('Error creating organization:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update an existing organization
 */
export const updateOrganization = async (id: string, organization: Organization): Promise<Organization> => {
  try {
    const response = await apiClient.put<Organization>(`/organizations/${id}`, organization);
    return response.data;
  } catch (error) {
    console.error(`Error updating organization ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Delete an organization
 */
export const deleteOrganization = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/organizations/${id}`);
  } catch (error) {
    console.error(`Error deleting organization ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Search organizations by query
 */
export const searchOrganizations = async (query: string): Promise<Organization[]> => {
  try {
    const response = await apiClient.get<Organization[]>(`/organizations/search?q=${encodeURIComponent(query)}`);
    return response.data;
  } catch (error) {
    console.error('Error searching organizations:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get campaigns by organization
 */
export const getCampaignsByOrganization = async (organizationId: string): Promise<any[]> => {
  try {
    const response = await apiClient.get(`/campaigns?organizationId=${organizationId}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching campaigns for organization ${organizationId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Verify an organization (admin/moderator only)
 */
export const verifyOrganization = async (id: string): Promise<void> => {
  try {
    await apiClient.post(`/organizations/${id}/verify`);
  } catch (error) {
    console.error(`Error verifying organization ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Reject an organization (admin/moderator only)
 */
export const rejectOrganization = async (id: string, reason: string): Promise<void> => {
  try {
    await apiClient.post(`/organizations/${id}/reject`, { reason });
  } catch (error) {
    console.error(`Error rejecting organization ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Upload organization logo
 */
export const uploadOrganizationLogo = async (id: string, file: File): Promise<string> => {
  try {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await apiClient.post<{ url: string }>(`/organizations/${id}/logo`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.data.url;
  } catch (error) {
    console.error(`Error uploading logo for organization ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Add team member to organization
 */
export const addTeamMember = async (
  organizationId: string, 
  member: { name: string; title: string; bio?: string; photoUrl?: string }
): Promise<Organization> => {
  try {
    const response = await apiClient.post<Organization>(
      `/organizations/${organizationId}/team-members`, 
      member
    );
    return response.data;
  } catch (error) {
    console.error(`Error adding team member to organization ${organizationId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Remove team member from organization
 */
export const removeTeamMember = async (organizationId: string, memberId: string): Promise<void> => {
  try {
    await apiClient.delete(`/organizations/${organizationId}/team-members/${memberId}`);
  } catch (error) {
    console.error(`Error removing team member ${memberId} from organization ${organizationId}:`, error);
    throw new Error(handleApiError(error));
  }
};