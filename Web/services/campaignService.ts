import apiClient from './apiClient';

export type Campaign = {
  id: string;
  title: string;
  description: string;
  organizationId: string;
  organizationName?: string;
  goal: number;
  amountRaised: number;
  startDate: string;
  endDate: string;
  status: string;
  coverImageUrl?: string;
  additionalImagesUrls?: string[];
  videoUrl?: string;
  category: string;
  updates?: Array<{
    id: string;
    title: string;
    content: string;
    postedAt: string;
  }>;
  milestones?: Array<{
    id: string;
    title: string;
    description: string;
    targetAmount: number;
    isCompleted: boolean;
  }>;
};

export const getCampaigns = async (): Promise<Campaign[]> => {
  const response = await apiClient.get<Campaign[]>('/campaigns');
  return response.data;
};

export const getFeaturedCampaigns = async (): Promise<Campaign[]> => {
  const response = await apiClient.get<Campaign[]>('/campaigns/featured');
  return response.data;
};

export const getCampaignsByCategory = async (category: string): Promise<Campaign[]> => {
  const response = await apiClient.get<Campaign[]>(`/campaigns/category/${category}`);
  return response.data;
};

export const getCampaignById = async (id: string): Promise<Campaign> => {
  const response = await apiClient.get<Campaign>(`/campaigns/${id}`);
  return response.data;
};

export const createCampaign = async (campaign: Campaign): Promise<Campaign> => {
  const response = await apiClient.post<Campaign>('/campaigns', campaign);
  return response.data;
};

export const updateCampaign = async (id: string, campaign: Campaign): Promise<void> => {
  await apiClient.put(`/campaigns/${id}`, campaign);
};

export const deleteCampaign = async (id: string): Promise<void> => {
  await apiClient.delete(`/campaigns/${id}`);
};

export const getCampaignStatistics = async (id: string): Promise<any> => {
  const response = await apiClient.get(`/campaigns/${id}/statistics`);
  return response.data;
};

export const addCampaignUpdate = async (campaignId: string, update: { title: string; content: string }): Promise<void> => {
  await apiClient.post(`/campaigns/${campaignId}/updates`, update);
};

export const addCampaignMilestone = async (campaignId: string, milestone: { title: string; description: string; targetAmount: number }): Promise<void> => {
  await apiClient.post(`/campaigns/${campaignId}/milestones`, milestone);
};