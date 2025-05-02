package al.raiseyourvoice.android.data.remote

import al.raiseyourvoice.android.data.remote.dto.CreatePostRequest
import al.raiseyourvoice.android.data.remote.dto.PostDto
import retrofit2.Response
import retrofit2.http.*

/**
 * API service for post-related operations
 */
interface PostApiService {
    
    @GET("posts")
    suspend fun getAllPosts(): Response<List<PostDto>>
    
    @GET("posts/{postId}")
    suspend fun getPostById(@Path("postId") postId: String): Response<PostDto>
    
    @GET("posts/type/{type}")
    suspend fun getPostsByType(@Path("type") type: String): Response<List<PostDto>>
    
    @GET("posts/user/{userId}")
    suspend fun getPostsByUser(@Path("userId") userId: String): Response<List<PostDto>>
    
    @GET("posts/tags")
    suspend fun getPostsByTags(@Query("tags") tags: String): Response<List<PostDto>>
    
    @GET("posts/location")
    suspend fun getPostsByLocation(
        @Query("lat") latitude: Double,
        @Query("lng") longitude: Double,
        @Query("radius") radiusKm: Double
    ): Response<List<PostDto>>
    
    @GET("posts/events")
    suspend fun getUpcomingEvents(
        @Query("after") afterDate: String? = null
    ): Response<List<PostDto>>
    
    @POST("posts")
    suspend fun createPost(@Body post: CreatePostRequest): Response<PostDto>
    
    @PUT("posts/{postId}")
    suspend fun updatePost(
        @Path("postId") postId: String,
        @Body post: CreatePostRequest
    ): Response<PostDto>
    
    @DELETE("posts/{postId}")
    suspend fun deletePost(@Path("postId") postId: String): Response<Unit>
    
    @POST("posts/{postId}/like")
    suspend fun likePost(@Path("postId") postId: String): Response<Unit>
    
    @DELETE("posts/{postId}/like")
    suspend fun unlikePost(@Path("postId") postId: String): Response<Unit>
    
    @GET("posts/search")
    suspend fun searchPosts(@Query("query") query: String): Response<List<PostDto>>
}