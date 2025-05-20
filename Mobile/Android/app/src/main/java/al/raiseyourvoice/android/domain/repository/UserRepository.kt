package al.raiseyourvoice.android.domain.repository

import al.raiseyourvoice.android.domain.model.User
import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow

/**
 * Repository interface for user-related operations
 */
interface UserRepository : BaseRepository<User, String> {
    
    /**
     * Authenticate a user with email and password
     */
    suspend fun login(email: String, password: String): Flow<Resource<User>>
    
    /**
     * Register a new user
     */
    suspend fun register(name: String, email: String, password: String): Flow<Resource<User>>
    
    /**
     * Get the current authenticated user
     */
    suspend fun getCurrentUser(): Flow<Resource<User>>
    
    /**
     * Update the user's profile information
     */
    suspend fun updateProfile(user: User): Flow<Resource<User>>
    
    /**
     * Change the user's password
     */
    suspend fun changePassword(oldPassword: String, newPassword: String): Flow<Resource<Boolean>>
    
    /**
     * Request a password reset for a user
     */
    suspend fun requestPasswordReset(email: String): Result<Unit>
    
    /**
     * Log out the current user
     */
    suspend fun logout(): Flow<Resource<Boolean>>
}