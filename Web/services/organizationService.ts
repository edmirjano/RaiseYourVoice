import apiClient from './apiClient';

export type Organization = {
  id?: string;
  name: string;
  description: string;
  logo?: string;
  website?: string;
  email?: string;
  phone?: string;
  socialMedia?: {
    facebook?: string;
    twitter?: string;
    instagram?: string;
    linkedin?: string;
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
  createdAt?: string;
  updatedAt?: string;
};

export const getOrganizations = async (): Promise<Organization[]> => {
  const response = await apiClient.get<Organization[]>('/organizations');
  return response.data;
};

export const getVerifiedOrganizations = async (): Promise<Organization[]> => {
  const response = await apiClient.get<Organization[]>('/organizations/verified');
  return response.data;
};

export const getOrganizationById = async (id: string): Promise<Organization> => {
  const response = await apiClient.get<Organization>(`/organizations/${id}`);
  return response.data;
};

export const createOrganization = async (organization: Organization): Promise<Organization> => {
  const response = await apiClient.post<Organization>('/organizations', organization);
  return response.data;
};

export const updateOrganization = async (id: string, organization: Organization): Promise<void> => {
  await apiClient.put(`/organizations/${id}`, organization);
};

export const deleteOrganization = async (id: string): Promise<void> => {
  await apiClient.delete(`/organizations/${id}`);
};