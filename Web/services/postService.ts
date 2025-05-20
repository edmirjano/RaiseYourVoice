import apiClient from './apiClient';

export type Post = {
  id?: string;
  title: string;
  content: string;
  mediaUrls?: string[];
  postType: 'Activism' | 'Opportunity' | 'SuccessStory';
  authorId?: string;
  likeCount?: number;
  commentCount?: number;
  tags?: string[];
  location?: {
    address?: string;
    city?: string;
    country?: string;
    latitude?: number;
    longitude?: number;
  };
  eventDate?: string;
  status?: 'Published' | 'Draft' | 'Removed';
  createdAt?: string;
  updatedAt?: string;
};

export const getPosts = async (): Promise<Post[]> => {
  const response = await apiClient.get<Post[]>('/posts');
  return response.data;
};

export const getSocialFeed = async (): Promise<Post[]> => {
  const response = await apiClient.get<Post[]>('/posts/feed');
  return response.data;
};

export const getOpportunities = async (): Promise<Post[]> => {
  const response = await apiClient.get<Post[]>('/posts/opportunities');
  return response.data;
};

export const getSuccessStories = async (): Promise<Post[]> => {
  const response = await apiClient.get<Post[]>('/posts/success-stories');
  return response.data;
};

export const getPostById = async (id: string): Promise<Post> => {
  const response = await apiClient.get<Post>(`/posts/${id}`);
  return response.data;
};

export const createPost = async (post: Post): Promise<Post> => {
  const response = await apiClient.post<Post>('/posts', post);
  return response.data;
};

export const updatePost = async (id: string, post: Post): Promise<void> => {
  await apiClient.put(`/posts/${id}`, post);
};

export const deletePost = async (id: string): Promise<void> => {
  await apiClient.delete(`/posts/${id}`);
};

export const likePost = async (id: string): Promise<void> => {
  await apiClient.post(`/posts/${id}/like`);
};

export const searchPosts = async (query: string): Promise<Post[]> => {
  const response = await apiClient.get<Post[]>(`/posts/search?query=${encodeURIComponent(query)}`);
  return response.data;
};