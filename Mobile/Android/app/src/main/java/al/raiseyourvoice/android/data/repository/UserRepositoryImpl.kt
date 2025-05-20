package al.raiseyourvoice.android.data.repository

import al.raiseyourvoice.android.data.remote.UserApiService
import al.raiseyourvoice.android.data.remote.dto.LoginRequest
import al.raiseyourvoice.android.data.remote.dto.RegisterRequest
import al.raiseyourvoice.android.data.remote.dto.toDto
import al.raiseyourvoice.android.data.util.NetworkResponseHandler
import al.raiseyourvoice.android.domain.model.User
import al.raiseyourvoice.android.domain.repository.UserRepository
import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

/**
 * Implementation of the UserRepository interface that interacts with the backend API
 */
class UserRepositoryImpl @Inject constructor(
    private val userApiService: UserApiService
) : UserRepository {
    
    override suspend fun getById(id: String): Flow<Resource<User>> {
        return NetworkResponseHandler.handleApiCall(
            apiCall = { userApiService.getUserById(id) },
            transform = { it.toDomainModel() }
        )
    }
    
    override suspend fun getAll(): Flow<Resource<List<User>>> {
        return NetworkResponseHandler.handleApiCallList(
            apiCall = { userApiService.getAllUsers() },
            transform = { it.toDomainModel() }
        )
    }
    
    override suspend fun create(item: User): Flow<Resource<User>> {
        // In most cases, users are created via the register method
        // This is just a placeholder implementation
        throw UnsupportedOperationException("Users are created through the register method")
    }
    
    override suspend fun update(item: User): Flow<Resource<User>> {
        return NetworkResponseHandler.handleApiCall(
            apiCall = { userApiService.updateUser(item.toDto()) },
            transform = { it.toDomainModel() }
        )
    }
    
    override suspend fun delete(id: String): Flow<Resource<Boolean>> {
        // User deletion may be performed by admins only and is not supported in this implementation
        throw UnsupportedOperationException("User deletion is not supported in this implementation")
    }
    
    override suspend fun login(email: String, password: String): Flow<Resource<User>> {
        return NetworkResponseHandler.handleApiCall(
            apiCall = { userApiService.login(LoginRequest(email, password)) },
            transform = { it.toDomainModel() }
        )
    }
    
    override suspend fun register(name: String, email: String, password: String): Flow<Resource<User>> {
        return NetworkResponseHandler.handleApiCall(
            apiCall = { 
                userApiService.register(
                    RegisterRequest(
                        name = name,
                        email = email,
                        password = password,
                        confirmPassword = password
                    )
                ) 
            },
            transform = { it.toDomainModel() }
        )
    }
    
    override suspend fun getCurrentUser(): Flow<Resource<User>> {
        return NetworkResponseHandler.handleApiCall(
            apiCall = { userApiService.getCurrentUser() },
            transform = { it.toDomainModel() }
        )
    }
    
    override suspend fun updateProfile(user: User): Flow<Resource<User>> {
        return update(user)
    }
    
    override suspend fun changePassword(oldPassword: String, newPassword: String): Flow<Resource<Boolean>> {
        return NetworkResponseHandler.handleUnitApiCall(
            apiCall = {
                userApiService.changePassword(
                    mapOf(
                        "oldPassword" to oldPassword,
                        "newPassword" to newPassword
                    )
                )
            }
        )
    }
    
    override suspend fun requestPasswordReset(email: String): Result<Unit> {
        return try {
            userApiService.requestPasswordReset(mapOf("email" to email))
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    override suspend fun logout(): Flow<Resource<Boolean>> {
        return NetworkResponseHandler.handleUnitApiCall(
            apiCall = { userApiService.logout() }
        )
    }
}