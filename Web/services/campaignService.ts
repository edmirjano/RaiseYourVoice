import apiClient, { handleApiError } from './apiClient';

export type Campaign = {
  id?: string;
  title: string;
  description: string;
  organizationId: string;
  organizationName?: string;
  goal: number;
  amountRaised: number;
  startDate: string;
  endDate: string;
  status: 'Draft' | 'PendingApproval' | 'Active' | 'Paused' | 'Completed' | 'Cancelled' | 'Rejected';
  coverImageUrl?: string;
  additionalImagesUrls?: string[];
  videoUrl?: string;
  category: string;
  location?: {
    address?: string;
    city?: string;
    country?: string;
    latitude?: number;
    longitude?: number;
  };
  isFeatured?: boolean;
  viewCount?: number;
  tags?: string[];
  updates?: Array<{
    id: string;
    title: string;
    content: string;
    postedAt: string;
    imageUrls?: string[];
  }>;
  milestones?: Array<{
    id: string;
    title: string;
    description: string;
    targetAmount: number;
    isCompleted: boolean;
    reachedAt?: string;
  }>;
  transparencyReport?: {
    id: string;
    lastUpdated: string;
    expenses: Array<{
      id: string;
      description: string;
      amount: number;
      date: string;
      category: string;
      receiptUrl?: string;
    }>;
    receiptUrls?: string[];
    auditDocumentUrl?: string;
  };
  createdAt?: string;
  updatedAt?: string;
};

/**
 * Get all campaigns with pagination
 */
export const getCampaigns = async (
  page: number = 1, 
  pageSize: number = 10,
  status?: string,
  category?: string,
  sortBy?: string,
  ascending: boolean = false
): Promise<Campaign[]> => {
  try {
    const params: any = { page, pageSize };
    if (status) params.status = status;
    if (category) params.category = category;
    if (sortBy) {
      params.sortBy = sortBy;
      params.ascending = ascending;
    }
    
    const response = await apiClient.get<Campaign[]>('/campaigns', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching campaigns:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get featured campaigns
 */
export const getFeaturedCampaigns = async (limit: number = 5): Promise<Campaign[]> => {
  try {
    const response = await apiClient.get<Campaign[]>('/campaigns/featured', {
      params: { limit }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching featured campaigns:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get campaigns by category
 */
export const getCampaignsByCategory = async (
  category: string,
  page: number = 1,
  pageSize: number = 10
): Promise<Campaign[]> => {
  try {
    const response = await apiClient.get<Campaign[]>(`/campaigns/category/${category}`, {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching campaigns for category ${category}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a campaign by ID
 */
export const getCampaignById = async (id: string): Promise<Campaign> => {
  try {
    const response = await apiClient.get<Campaign>(`/campaigns/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching campaign ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a new campaign
 */
export const createCampaign = async (campaign: Campaign): Promise<Campaign> => {
  try {
    // Handle file uploads if needed
    if (campaign.coverImageUrl && campaign.coverImageUrl instanceof File) {
      const coverImageUrl = await uploadCampaignImage(campaign.coverImageUrl);
      campaign.coverImageUrl = coverImageUrl;
    }
    
    if (campaign.additionalImagesUrls && campaign.additionalImagesUrls.some(url => url instanceof File)) {
      const filesToUpload = campaign.additionalImagesUrls.filter(url => url instanceof File) as unknown as File[];
      const existingUrls = campaign.additionalImagesUrls.filter(url => !(url instanceof File));
      
      const uploadedUrls = await Promise.all(filesToUpload.map(file => uploadCampaignImage(file)));
      campaign.additionalImagesUrls = [...existingUrls, ...uploadedUrls];
    }
    
    const response = await apiClient.post<Campaign>('/campaigns', campaign);
    return response.data;
  } catch (error) {
    console.error('Error creating campaign:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update an existing campaign
 */
export const updateCampaign = async (id: string, campaign: Campaign): Promise<Campaign> => {
  try {
    // Handle file uploads if needed (similar to createCampaign)
    if (campaign.coverImageUrl && campaign.coverImageUrl instanceof File) {
      const coverImageUrl = await uploadCampaignImage(campaign.coverImageUrl);
      campaign.coverImageUrl = coverImageUrl;
    }
    
    if (campaign.additionalImagesUrls && campaign.additionalImagesUrls.some(url => url instanceof File)) {
      const filesToUpload = campaign.additionalImagesUrls.filter(url => url instanceof File) as unknown as File[];
      const existingUrls = campaign.additionalImagesUrls.filter(url => !(url instanceof File));
      
      const uploadedUrls = await Promise.all(filesToUpload.map(file => uploadCampaignImage(file)));
      campaign.additionalImagesUrls = [...existingUrls, ...uploadedUrls];
    }
    
    const response = await apiClient.put<Campaign>(`/campaigns/${id}`, campaign);
    return response.data;
  } catch (error) {
    console.error(`Error updating campaign ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Delete a campaign
 */
export const deleteCampaign = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/campaigns/${id}`);
  } catch (error) {
    console.error(`Error deleting campaign ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Approve a campaign (admin/moderator only)
 */
export const approveCampaign = async (id: string): Promise<void> => {
  try {
    await apiClient.post(`/campaigns/${id}/approve`);
  } catch (error) {
    console.error(`Error approving campaign ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Reject a campaign (admin/moderator only)
 */
export const rejectCampaign = async (id: string, reason: string): Promise<void> => {
  try {
    await apiClient.post(`/campaigns/${id}/reject`, { reason });
  } catch (error) {
    console.error(`Error rejecting campaign ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Add an update to a campaign
 */
export const addCampaignUpdate = async (
  campaignId: string, 
  update: { title: string; content: string; imageUrls?: string[] }
): Promise<void> => {
  try {
    // Handle file uploads if needed
    if (update.imageUrls && update.imageUrls.some(url => url instanceof File)) {
      const filesToUpload = update.imageUrls.filter(url => url instanceof File) as unknown as File[];
      const existingUrls = update.imageUrls.filter(url => !(url instanceof File));
      
      const uploadedUrls = await Promise.all(filesToUpload.map(file => uploadCampaignImage(file)));
      update.imageUrls = [...existingUrls, ...uploadedUrls];
    }
    
    await apiClient.post(`/campaigns/${campaignId}/updates`, update);
  } catch (error) {
    console.error(`Error adding update to campaign ${campaignId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Add a milestone to a campaign
 */
export const addCampaignMilestone = async (
  campaignId: string, 
  milestone: { title: string; description: string; targetAmount: number }
): Promise<void> => {
  try {
    await apiClient.post(`/campaigns/${campaignId}/milestones`, milestone);
  } catch (error) {
    console.error(`Error adding milestone to campaign ${campaignId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update a campaign's transparency report
 */
export const updateTransparencyReport = async (
  campaignId: string, 
  report: Campaign['transparencyReport']
): Promise<void> => {
  try {
    await apiClient.put(`/campaigns/${campaignId}/transparency-report`, report);
  } catch (error) {
    console.error(`Error updating transparency report for campaign ${campaignId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Feature or unfeature a campaign (admin only)
 */
export const featureCampaign = async (id: string, featured: boolean): Promise<void> => {
  try {
    await apiClient.post(`/campaigns/${id}/feature`, { featured });
  } catch (error) {
    console.error(`Error ${featured ? 'featuring' : 'unfeaturing'} campaign ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Search campaigns by query
 */
export const searchCampaigns = async (query: string): Promise<Campaign[]> => {
  try {
    const response = await apiClient.get<Campaign[]>(`/campaigns/search?query=${encodeURIComponent(query)}`);
    return response.data;
  } catch (error) {
    console.error('Error searching campaigns:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get campaign statistics
 */
export const getCampaignStatistics = async (id: string): Promise<any> => {
  try {
    const response = await apiClient.get(`/campaigns/${id}/statistics`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching statistics for campaign ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get global campaign statistics
 */
export const getGlobalCampaignStatistics = async (): Promise<any> => {
  try {
    const response = await apiClient.get('/campaigns/statistics');
    return response.data;
  } catch (error) {
    console.error('Error fetching global campaign statistics:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get nearby campaigns
 */
export const getNearbyCampaigns = async (
  latitude: number,
  longitude: number,
  maxDistanceKm: number = 50,
  page: number = 1,
  pageSize: number = 10
): Promise<Campaign[]> => {
  try {
    const response = await apiClient.get<Campaign[]>('/campaigns/nearby', {
      params: {
        latitude,
        longitude,
        maxDistanceKm,
        pageNumber: page,
        pageSize
      }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching nearby campaigns:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Helper function to upload campaign images
 */
const uploadCampaignImage = async (file: File): Promise<string> => {
  try {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await apiClient.post<{ url: string }>('/media/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.data.url;
  } catch (error) {
    console.error('Error uploading campaign image:', error);
    throw new Error(handleApiError(error));
  }
};