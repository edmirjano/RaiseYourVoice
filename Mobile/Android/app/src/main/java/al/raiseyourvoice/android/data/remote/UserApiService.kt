package al.raiseyourvoice.android.data.remote

import al.raiseyourvoice.android.data.remote.dto.LoginRequest
import al.raiseyourvoice.android.data.remote.dto.RegisterRequest
import al.raiseyourvoice.android.data.remote.dto.UserDto
import retrofit2.Response
import retrofit2.http.*

/**
 * API service for user-related operations
 */
interface UserApiService {
    
    @POST("auth/login")
    suspend fun login(@Body loginRequest: LoginRequest): Response<UserDto>
    
    @POST("auth/register")
    suspend fun register(@Body registerRequest: RegisterRequest): Response<UserDto>
    
    @GET("users/me")
    suspend fun getCurrentUser(): Response<UserDto>
    
    @GET("users/{userId}")
    suspend fun getUserById(@Path("userId") userId: String): Response<UserDto>
    
    @GET("users")
    suspend fun getAllUsers(): Response<List<UserDto>>
    
    @PUT("users")
    suspend fun updateUser(@Body user: UserDto): Response<UserDto>
    
    @POST("auth/password/change")
    suspend fun changePassword(
        @Body request: Map<String, String>
    ): Response<Unit>
    
    @POST("auth/password/reset")
    suspend fun requestPasswordReset(
        @Body request: Map<String, String>
    ): Response<Unit>
    
    @POST("auth/logout")
    suspend fun logout(): Response<Unit>
}