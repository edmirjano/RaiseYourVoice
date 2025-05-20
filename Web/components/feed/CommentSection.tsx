import React, { useState, useEffect } from 'react';
import { useTranslation } from 'next-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { formatDistanceToNow } from 'date-fns';
import { useAuth } from '../../contexts/AuthContext';
import { getCommentsByPost, createComment, likeComment, unlikeComment, createReply, Comment } from '../../services/commentService';
import { Avatar } from '../common/Avatar/Avatar';
import { Button } from '../common/Button/Button';
import { TextArea } from '../common/Form/TextArea';
import { Loader } from '../common/Loader/Loader';
import { Alert } from '../common/Alert/Alert';
import { EmptyState } from '../common/EmptyState/EmptyState';

interface CommentSectionProps {
  postId: string;
  className?: string;
}

export const CommentSection: React.FC<CommentSectionProps> = ({ postId, className = '' }) => {
  const { t } = useTranslation('common');
  const { user, isAuthenticated } = useAuth();
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [newComment, setNewComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [replyingTo, setReplyingTo] = useState<string | null>(null);
  const [replyContent, setReplyContent] = useState('');
  const [likedComments, setLikedComments] = useState<Record<string, boolean>>({});
  const [expandedReplies, setExpandedReplies] = useState<Record<string, boolean>>({});
  
  useEffect(() => {
    const fetchComments = async () => {
      try {
        setLoading(true);
        const data = await getCommentsByPost(postId);
        
        // Initialize liked comments state
        const initialLikedComments: Record<string, boolean> = {};
        data.forEach(comment => {
          if (comment.id && comment.isLikedByCurrentUser) {
            initialLikedComments[comment.id] = true;
          }
        });
        setLikedComments(initialLikedComments);
        
        setComments(data);
        setError('');
      } catch (err) {
        console.error('Failed to fetch comments:', err);
        setError(t('errors.fetchFailed'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchComments();
  }, [postId, t]);
  
  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newComment.trim()) return;
    
    try {
      setSubmitting(true);
      const comment = await createComment(postId, newComment);
      setComments([comment, ...comments]);
      setNewComment('');
    } catch (err) {
      console.error('Failed to create comment:', err);
      setError(t('errors.commentFailed'));
    } finally {
      setSubmitting(false);
    }
  };
  
  const handleSubmitReply = async (parentId: string) => {
    if (!replyContent.trim()) return;
    
    try {
      setSubmitting(true);
      const reply = await createReply(parentId, replyContent);
      
      // Update the comments array with the new reply
      setComments(prevComments => {
        return prevComments.map(comment => {
          if (comment.id === parentId) {
            // Increment child comment count
            return {
              ...comment,
              childCommentCount: (comment.childCommentCount || 0) + 1
            };
          }
          return comment;
        });
      });
      
      // Expand replies for this comment
      setExpandedReplies(prev => ({
        ...prev,
        [parentId]: true
      }));
      
      setReplyingTo(null);
      setReplyContent('');
    } catch (err) {
      console.error('Failed to create reply:', err);
      setError(t('errors.commentFailed'));
    } finally {
      setSubmitting(false);
    }
  };
  
  const handleLikeComment = async (commentId: string) => {
    try {
      const isLiked = likedComments[commentId];
      
      if (isLiked) {
        await unlikeComment(commentId);
      } else {
        await likeComment(commentId);
      }
      
      // Update local state
      setLikedComments(prev => ({
        ...prev,
        [commentId]: !isLiked
      }));
      
      // Update comment like count
      setComments(prevComments => {
        return prevComments.map(comment => {
          if (comment.id === commentId) {
            return {
              ...comment,
              likeCount: isLiked 
                ? Math.max(0, (comment.likeCount || 0) - 1) 
                : (comment.likeCount || 0) + 1
            };
          }
          return comment;
        });
      });
    } catch (err) {
      console.error('Failed to like/unlike comment:', err);
    }
  };
  
  const toggleReplies = (commentId: string) => {
    setExpandedReplies(prev => ({
      ...prev,
      [commentId]: !prev[commentId]
    }));
  };
  
  const formatDate = (dateString: string) => {
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  return (
    <div className={`mt-6 ${className}`}>
      <h3 className="text-lg font-semibold mb-4">{t('comments.title')}</h3>
      
      {/* Comment Form */}
      {isAuthenticated ? (
        <form onSubmit={handleSubmitComment} className="mb-6">
          <div className="mb-2">
            <TextArea
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder={t('comments.placeholder')}
              className="w-full"
              rows={3}
            />
          </div>
          <div className="flex justify-end">
            <Button
              type="submit"
              disabled={submitting || !newComment.trim()}
              size="sm"
            >
              {submitting ? t('comments.submitting') : t('comments.submit')}
            </Button>
          </div>
        </form>
      ) : (
        <div className="mb-6 p-4 bg-gray-50 rounded-lg text-center">
          <p className="text-gray-600 mb-2">{t('comments.loginToComment')}</p>
          <Button 
            variant="secondary" 
            size="sm"
            onClick={() => window.location.href = '/auth/login'}
          >
            {t('auth.login')}
          </Button>
        </div>
      )}
      
      {/* Error Message */}
      {error && (
        <Alert 
          variant="error" 
          className="mb-4"
          onClose={() => setError('')}
        >
          {error}
        </Alert>
      )}
      
      {/* Comments List */}
      {loading ? (
        <div className="flex justify-center py-8">
          <Loader size="md" type="spinner" text={t('common.loading')} />
        </div>
      ) : comments.length === 0 ? (
        <EmptyState
          title={t('comments.noComments')}
          icon={
            <svg className="h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
          }
        />
      ) : (
        <div className="space-y-4">
          <AnimatePresence>
            {comments.map((comment) => (
              <motion.div
                key={comment.id}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                className="bg-white p-4 rounded-lg border border-gray-100"
              >
                <div className="flex items-start">
                  <Avatar 
                    src={comment.authorProfilePicUrl} 
                    name={comment.authorName || 'User'} 
                    size="sm" 
                  />
                  <div className="ml-3 flex-1">
                    <div className="flex items-center justify-between">
                      <div className="text-sm font-medium text-gray-900">{comment.authorName || 'Anonymous'}</div>
                      <div className="text-xs text-gray-500">{formatDate(comment.createdAt)}</div>
                    </div>
                    <div className="mt-1 text-sm text-gray-700">{comment.content}</div>
                    <div className="mt-2 flex items-center space-x-4 text-xs">
                      {isAuthenticated && (
                        <>
                          <button 
                            onClick={() => comment.id && handleLikeComment(comment.id)}
                            className={`flex items-center ${comment.id && likedComments[comment.id] ? 'text-ios-black font-medium' : 'text-gray-500'}`}
                          >
                            <svg className="h-4 w-4 mr-1" fill={comment.id && likedComments[comment.id] ? 'currentColor' : 'none'} viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={comment.id && likedComments[comment.id] ? 0 : 2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
                            </svg>
                            {comment.likeCount || 0} {t('comments.like')}
                          </button>
                          <button 
                            onClick={() => setReplyingTo(replyingTo === comment.id ? null : comment.id)}
                            className="text-gray-500 hover:text-ios-black"
                          >
                            {t('comments.reply')}
                          </button>
                        </>
                      )}
                      
                      {/* Show replies button if there are any */}
                      {comment.childCommentCount > 0 && (
                        <button 
                          onClick={() => comment.id && toggleReplies(comment.id)}
                          className="text-gray-500 hover:text-ios-black"
                        >
                          {expandedReplies[comment.id!] ? t('comments.hideReplies') : t('comments.showReplies', { count: comment.childCommentCount })}
                        </button>
                      )}
                    </div>
                    
                    {/* Reply Form */}
                    <AnimatePresence>
                      {replyingTo === comment.id && (
                        <motion.div
                          initial={{ opacity: 0, height: 0 }}
                          animate={{ opacity: 1, height: 'auto' }}
                          exit={{ opacity: 0, height: 0 }}
                          transition={{ duration: 0.2 }}
                          className="mt-3"
                        >
                          <div className="flex space-x-2">
                            <TextArea
                              value={replyContent}
                              onChange={(e) => setReplyContent(e.target.value)}
                              placeholder={t('comments.replyPlaceholder')}
                              className="flex-1"
                              rows={2}
                            />
                            <div className="flex flex-col space-y-2">
                              <Button
                                size="sm"
                                disabled={submitting || !replyContent.trim()}
                                onClick={() => comment.id && handleSubmitReply(comment.id)}
                              >
                                {submitting ? t('comments.submitting') : t('comments.reply')}
                              </Button>
                              <Button
                                size="sm"
                                variant="secondary"
                                onClick={() => setReplyingTo(null)}
                              >
                                {t('common.cancel')}
                              </Button>
                            </div>
                          </div>
                        </motion.div>
                      )}
                    </AnimatePresence>
                    
                    {/* Replies Section */}
                    <AnimatePresence>
                      {comment.id && expandedReplies[comment.id] && comment.childCommentCount > 0 && (
                        <motion.div
                          initial={{ opacity: 0, height: 0 }}
                          animate={{ opacity: 1, height: 'auto' }}
                          exit={{ opacity: 0, height: 0 }}
                          transition={{ duration: 0.3 }}
                          className="mt-3 pl-4 border-l-2 border-gray-100"
                        >
                          {/* This would be replaced with actual replies fetched from the API */}
                          <div className="py-2 text-sm text-gray-500">
                            {t('comments.loadingReplies')}
                          </div>
                        </motion.div>
                      )}
                    </AnimatePresence>
                  </div>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        </div>
      )}
    </div>
  );
};