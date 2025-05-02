package al.raiseyourvoice.android.domain.repository

import al.raiseyourvoice.android.domain.model.Post
import al.raiseyourvoice.android.domain.model.PostType
import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow
import java.util.Date

/**
 * Repository interface for post-related operations
 */
interface PostRepository : BaseRepository<Post, String> {
    
    /**
     * Get posts filtered by type
     */
    suspend fun getPostsByType(type: PostType): Flow<Resource<List<Post>>>
    
    /**
     * Get posts created by a specific user
     */
    suspend fun getPostsByUser(userId: String): Flow<Resource<List<Post>>>
    
    /**
     * Get posts filtered by tags
     */
    suspend fun getPostsByTags(tags: List<String>): Flow<Resource<List<Post>>>
    
    /**
     * Get posts created near a specific location
     */
    suspend fun getPostsByLocation(latitude: Double, longitude: Double, radiusKm: Double): Flow<Resource<List<Post>>>
    
    /**
     * Get posts for upcoming events
     */
    suspend fun getUpcomingEvents(afterDate: Date = Date()): Flow<Resource<List<Post>>>
    
    /**
     * Like or unlike a post
     */
    suspend fun toggleLike(postId: String): Flow<Resource<Boolean>>
    
    /**
     * Search posts by keyword
     */
    suspend fun searchPosts(query: String): Flow<Resource<List<Post>>>
}