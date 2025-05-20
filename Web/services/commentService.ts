import apiClient from './apiClient';

export type Comment = {
  id?: string;
  postId: string;
  authorId?: string;
  content: string;
  createdAt: string;
  updatedAt?: string;
  likeCount?: number;
  parentCommentId?: string;
};

export const getCommentsByPost = async (postId: string): Promise<Comment[]> => {
  const response = await apiClient.get<Comment[]>(`/comments/post/${postId}`);
  return response.data;
};

export const getComment = async (id: string): Promise<Comment> => {
  const response = await apiClient.get<Comment>(`/comments/${id}`);
  return response.data;
};

export const createComment = async (postId: string, content: string): Promise<Comment> => {
  const response = await apiClient.post<Comment>('/comments', {
    postId,
    content
  });
  return response.data;
};

export const updateComment = async (id: string, content: string): Promise<void> => {
  await apiClient.put(`/comments/${id}`, {
    id,
    content
  });
};

export const deleteComment = async (id: string): Promise<void> => {
  await apiClient.delete(`/comments/${id}`);
};

export const likeComment = async (id: string): Promise<void> => {
  await apiClient.post(`/comments/${id}/like`);
};

export const getReplies = async (commentId: string): Promise<Comment[]> => {
  const response = await apiClient.get<Comment[]>(`/comments/${commentId}/replies`);
  return response.data;
};

export const createReply = async (commentId: string, content: string): Promise<Comment> => {
  const response = await apiClient.post<Comment>(`/comments/${commentId}/replies`, {
    content
  });
  return response.data;
};