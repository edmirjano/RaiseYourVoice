package al.raiseyourvoice.android.domain.repository

import al.raiseyourvoice.android.domain.model.Comment
import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow

/**
 * Repository interface for comment-related operations
 */
interface CommentRepository : BaseRepository<Comment, String> {
    
    /**
     * Get comments for a specific post
     */
    suspend fun getCommentsForPost(postId: String): Flow<Resource<List<Comment>>>
    
    /**
     * Get replies to a specific comment
     */
    suspend fun getRepliesForComment(commentId: String): Flow<Resource<List<Comment>>>
    
    /**
     * Add a new comment to a post
     */
    suspend fun addComment(postId: String, content: String): Flow<Resource<Comment>>
    
    /**
     * Add a reply to an existing comment
     */
    suspend fun addReply(commentId: String, content: String): Flow<Resource<Comment>>
    
    /**
     * Like or unlike a comment
     */
    suspend fun toggleLike(commentId: String): Flow<Resource<Boolean>>
}