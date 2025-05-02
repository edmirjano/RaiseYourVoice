package al.raiseyourvoice.android.data.remote

import al.raiseyourvoice.android.data.remote.dto.CommentDto
import al.raiseyourvoice.android.data.remote.dto.CreateCommentRequest
import retrofit2.Response
import retrofit2.http.*

/**
 * API service for comment-related operations
 */
interface CommentApiService {
    
    @GET("posts/{postId}/comments")
    suspend fun getCommentsForPost(
        @Path("postId") postId: String
    ): Response<List<CommentDto>>
    
    @GET("comments/{commentId}")
    suspend fun getCommentById(
        @Path("commentId") commentId: String
    ): Response<CommentDto>
    
    @GET("comments/{commentId}/replies")
    suspend fun getRepliesForComment(
        @Path("commentId") commentId: String
    ): Response<List<CommentDto>>
    
    @POST("posts/{postId}/comments")
    suspend fun addComment(
        @Path("postId") postId: String,
        @Body request: CreateCommentRequest
    ): Response<CommentDto>
    
    @POST("comments/{commentId}/replies")
    suspend fun addReply(
        @Path("commentId") commentId: String,
        @Body request: CreateCommentRequest
    ): Response<CommentDto>
    
    @PUT("comments/{commentId}")
    suspend fun updateComment(
        @Path("commentId") commentId: String,
        @Body request: CreateCommentRequest
    ): Response<CommentDto>
    
    @DELETE("comments/{commentId}")
    suspend fun deleteComment(
        @Path("commentId") commentId: String
    ): Response<Unit>
    
    @POST("comments/{commentId}/like")
    suspend fun likeComment(
        @Path("commentId") commentId: String
    ): Response<Unit>
    
    @DELETE("comments/{commentId}/like")
    suspend fun unlikeComment(
        @Path("commentId") commentId: String
    ): Response<Unit>
}