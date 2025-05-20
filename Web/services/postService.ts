import apiClient, { handleApiError } from './apiClient';

export type Post = {
  id?: string;
  title: string;
  content: string;
  mediaUrls?: string[];
  postType: 'Activism' | 'Opportunity' | 'SuccessStory';
  authorId?: string;
  authorName?: string;
  authorProfilePicUrl?: string;
  likeCount?: number;
  commentCount?: number;
  isLikedByCurrentUser?: boolean;
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

/**
 * Get all posts
 */
export const getPosts = async (): Promise<Post[]> => {
  try {
    const response = await apiClient.get<Post[]>('/posts');
    return response.data;
  } catch (error) {
    console.error('Error fetching posts:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get social feed posts
 */
export const getSocialFeed = async (page: number = 1, pageSize: number = 10): Promise<Post[]> => {
  try {
    const response = await apiClient.get<Post[]>('/posts/feed', {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching social feed:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get opportunity posts
 */
export const getOpportunities = async (page: number = 1, pageSize: number = 10): Promise<Post[]> => {
  try {
    const response = await apiClient.get<Post[]>('/posts/opportunities', {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching opportunities:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get success story posts
 */
export const getSuccessStories = async (page: number = 1, pageSize: number = 10): Promise<Post[]> => {
  try {
    const response = await apiClient.get<Post[]>('/posts/success-stories', {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching success stories:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a post by ID
 */
export const getPostById = async (id: string): Promise<Post> => {
  try {
    const response = await apiClient.get<Post>(`/posts/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching post ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a new post
 */
export const createPost = async (post: Post): Promise<Post> => {
  try {
    // Handle file uploads if mediaUrls contains File objects
    if (post.mediaUrls && post.mediaUrls.some(url => url instanceof File)) {
      const uploadedUrls = await uploadPostMedia(post.mediaUrls as unknown as File[]);
      post.mediaUrls = uploadedUrls;
    }
    
    const response = await apiClient.post<Post>('/posts', post);
    return response.data;
  } catch (error) {
    console.error('Error creating post:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update an existing post
 */
export const updatePost = async (id: string, post: Post): Promise<Post> => {
  try {
    // Handle file uploads if mediaUrls contains File objects
    if (post.mediaUrls && post.mediaUrls.some(url => url instanceof File)) {
      const filesToUpload = post.mediaUrls.filter(url => url instanceof File) as unknown as File[];
      const existingUrls = post.mediaUrls.filter(url => !(url instanceof File));
      
      const uploadedUrls = await uploadPostMedia(filesToUpload);
      post.mediaUrls = [...existingUrls, ...uploadedUrls];
    }
    
    const response = await apiClient.put<Post>(`/posts/${id}`, post);
    return response.data;
  } catch (error) {
    console.error(`Error updating post ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Delete a post
 */
export const deletePost = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/posts/${id}`);
  } catch (error) {
    console.error(`Error deleting post ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Like a post
 */
export const likePost = async (id: string): Promise<void> => {
  try {
    await apiClient.post(`/posts/${id}/like`);
  } catch (error) {
    console.error(`Error liking post ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Unlike a post
 */
export const unlikePost = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/posts/${id}/like`);
  } catch (error) {
    console.error(`Error unliking post ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Search posts by query
 */
export const searchPosts = async (query: string): Promise<Post[]> => {
  try {
    const response = await apiClient.get<Post[]>(`/posts/search?query=${encodeURIComponent(query)}`);
    return response.data;
  } catch (error) {
    console.error('Error searching posts:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get posts by tag
 */
export const getPostsByTag = async (tag: string): Promise<Post[]> => {
  try {
    const response = await apiClient.get<Post[]>(`/posts/tags?tags=${encodeURIComponent(tag)}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching posts with tag ${tag}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get posts by location
 */
export const getPostsByLocation = async (
  latitude: number,
  longitude: number,
  radiusKm: number = 10
): Promise<Post[]> => {
  try {
    const response = await apiClient.get<Post[]>('/posts/location', {
      params: { lat: latitude, lng: longitude, radius: radiusKm }
    });
    return response.data;
  } catch (error) {
    console.error('Error fetching posts by location:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get upcoming events
 */
export const getUpcomingEvents = async (afterDate?: Date): Promise<Post[]> => {
  try {
    const params: any = {};
    if (afterDate) {
      params.after = afterDate.toISOString();
    }
    
    const response = await apiClient.get<Post[]>('/posts/events', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching upcoming events:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Upload media files for a post
 * This is a helper function used by createPost and updatePost
 */
const uploadPostMedia = async (files: File[]): Promise<string[]> => {
  try {
    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file);
    });
    
    const response = await apiClient.post<{ urls: string[] }>('/media/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    
    return response.data.urls;
  } catch (error) {
    console.error('Error uploading media files:', error);
    throw new Error(handleApiError(error));
  }
};