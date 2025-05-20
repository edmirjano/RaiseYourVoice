import apiClient, { handleApiError } from './apiClient';

export type Comment = {
  id?: string;
  postId: string;
  authorId?: string;
  authorName?: string;
  authorProfilePicUrl?: string;
  content: string;
  createdAt: string;
  updatedAt?: string;
  likeCount?: number;
  isLikedByCurrentUser?: boolean;
  parentCommentId?: string;
  childCommentCount?: number;
};

/**
 * Get comments for a post
 */
export const getCommentsByPost = async (postId: string, page: number = 1, pageSize: number = 20): Promise<Comment[]> => {
  try {
    const response = await apiClient.get<Comment[]>(`/comments/post/${postId}`, {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching comments for post ${postId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a specific comment by ID
 */
export const getComment = async (id: string): Promise<Comment> => {
  try {
    const response = await apiClient.get<Comment>(`/comments/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching comment ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a new comment on a post
 */
export const createComment = async (postId: string, content: string): Promise<Comment> => {
  try {
    const response = await apiClient.post<Comment>('/comments', {
      postId,
      content
    });
    return response.data;
  } catch (error) {
    console.error('Error creating comment:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Update an existing comment
 */
export const updateComment = async (id: string, content: string): Promise<Comment> => {
  try {
    const response = await apiClient.put<Comment>(`/comments/${id}`, {
      id,
      content
    });
    return response.data;
  } catch (error) {
    console.error(`Error updating comment ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Delete a comment
 */
export const deleteComment = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/comments/${id}`);
  } catch (error) {
    console.error(`Error deleting comment ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Like a comment
 */
export const likeComment = async (id: string): Promise<void> => {
  try {
    await apiClient.post(`/comments/${id}/like`);
  } catch (error) {
    console.error(`Error liking comment ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Unlike a comment
 */
export const unlikeComment = async (id: string): Promise<void> => {
  try {
    await apiClient.delete(`/comments/${id}/like`);
  } catch (error) {
    console.error(`Error unliking comment ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get replies to a comment
 */
export const getReplies = async (commentId: string, page: number = 1, pageSize: number = 10): Promise<Comment[]> => {
  try {
    const response = await apiClient.get<Comment[]>(`/comments/${commentId}/replies`, {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching replies for comment ${commentId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a reply to a comment
 */
export const createReply = async (commentId: string, content: string): Promise<Comment> => {
  try {
    const response = await apiClient.post<Comment>(`/comments/${commentId}/replies`, {
      content
    });
    return response.data;
  } catch (error) {
    console.error('Error creating reply:', error);
    throw new Error(handleApiError(error));
  }
};