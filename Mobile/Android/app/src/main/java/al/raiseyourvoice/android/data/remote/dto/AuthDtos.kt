package al.raiseyourvoice.android.data.remote.dto

/**
 * Request object for user login
 */
data class LoginRequest(
    val email: String,
    val password: String
)

/**
 * Request object for user registration
 */
data class RegisterRequest(
    val name: String,
    val email: String,
    val password: String,
    val confirmPassword: String
)

/**
 * Response object for authentication operations
 */
data class AuthResponse(
    val user: UserDto,
    val token: String,
    val expiresAt: String
)