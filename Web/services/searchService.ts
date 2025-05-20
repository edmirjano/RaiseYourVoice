import apiClient, { handleApiError } from './apiClient';
import { Post, searchPosts } from './postService';
import { Campaign, searchCampaigns } from './campaignService';
import { Organization, searchOrganizations } from './organizationService';

export type SearchResult = {
  posts: Post[];
  campaigns: Campaign[];
  organizations: Organization[];
};

/**
 * Search across all content types
 */
export const searchAll = async (query: string): Promise<SearchResult> => {
  try {
    // Make parallel requests for better performance
    const [posts, campaigns, organizations] = await Promise.all([
      searchPosts(query),
      searchCampaigns(query),
      searchOrganizations(query)
    ]);
    
    return {
      posts,
      campaigns,
      organizations
    };
  } catch (error) {
    console.error('Error performing search:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get trending topics/tags
 */
export const getTrendingTopics = async (limit: number = 10): Promise<string[]> => {
  try {
    const response = await apiClient.get<string[]>('/search/trending-topics', {
      params: { limit }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching trending topics:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get search suggestions as user types
 */
export const getSearchSuggestions = async (query: string, limit: number = 5): Promise<string[]> => {
  try {
    const response = await apiClient.get<string[]>('/search/suggestions', {
      params: { query, limit }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching search suggestions:', error);
    throw new Error(handleApiError(error));
  }
};