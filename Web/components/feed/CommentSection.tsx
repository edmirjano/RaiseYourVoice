import React, { useState, useEffect } from 'react';
import { useTranslation } from 'next-i18next';
import { motion, AnimatePresence } from 'framer-motion';
import { formatDistanceToNow } from 'date-fns';
import { useAuth } from '../../contexts/AuthContext';
import { getCommentsByPost, createComment, Comment } from '../../services/commentService';

interface CommentSectionProps {
  postId: string;
}

export const CommentSection: React.FC<CommentSectionProps> = ({ postId }) => {
  const { t } = useTranslation('common');
  const { user, isAuthenticated } = useAuth();
  const [comments, setComments] = useState<Comment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [newComment, setNewComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  
  useEffect(() => {
    const fetchComments = async () => {
      try {
        setLoading(true);
        const data = await getCommentsByPost(postId);
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
  
  const formatDate = (dateString: string) => {
    try {
      return formatDistanceToNow(new Date(dateString), { addSuffix: true });
    } catch (e) {
      return dateString;
    }
  };
  
  return (
    <div className="mt-6">
      <h3 className="text-lg font-semibold mb-4">{t('comments.title')}</h3>
      
      {/* Comment Form */}
      {isAuthenticated ? (
        <form onSubmit={handleSubmitComment} className="mb-6">
          <div className="mb-2">
            <textarea
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder={t('comments.placeholder')}
              className="w-full rounded-lg border border-gray-200 p-3 focus:border-ios-black focus:outline-none focus:ring-1 focus:ring-ios-black"
              rows={3}
            />
          </div>
          <div className="flex justify-end">
            <button
              type="submit"
              disabled={submitting || !newComment.trim()}
              className="ios-button"
            >
              {submitting ? t('comments.submitting') : t('comments.submit')}
            </button>
          </div>
        </form>
      ) : (
        <div className="mb-6 p-4 bg-gray-50 rounded-lg text-center">
          <p className="text-gray-600 mb-2">{t('comments.loginToComment')}</p>
          <a href="/auth/login" className="ios-button-secondary inline-block">
            {t('auth.login')}
          </a>
        </div>
      )}
      
      {/* Error Message */}
      {error && (
        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded-lg">
          {error}
        </div>
      )}
      
      {/* Comments List */}
      {loading ? (
        <div className="flex justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-ios-black"></div>
        </div>
      ) : comments.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          {t('comments.noComments')}
        </div>
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
                  <div className="h-8 w-8 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 font-semibold">
                    {comment.authorId?.charAt(0) || 'U'}
                  </div>
                  <div className="ml-3 flex-1">
                    <div className="flex items-center justify-between">
                      <div className="text-sm font-medium text-gray-900">{comment.authorId}</div>
                      <div className="text-xs text-gray-500">{formatDate(comment.createdAt)}</div>
                    </div>
                    <div className="mt-1 text-sm text-gray-700">{comment.content}</div>
                    <div className="mt-2 flex items-center space-x-4 text-xs">
                      <button className="text-gray-500 hover:text-ios-black">
                        {t('comments.like')} ({comment.likeCount || 0})
                      </button>
                      <button className="text-gray-500 hover:text-ios-black">
                        {t('comments.reply')}
                      </button>
                    </div>
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