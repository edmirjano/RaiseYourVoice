package al.raiseyourvoice.android.domain.usecase.auth

import al.raiseyourvoice.android.domain.model.User
import al.raiseyourvoice.android.domain.repository.UserRepository
import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

/**
 * Use case for authenticating a user with email and password
 */
class LoginUseCase @Inject constructor(
    private val userRepository: UserRepository
) {
    suspend operator fun invoke(email: String, password: String): Flow<Resource<User>> {
        return userRepository.login(email, password)
    }
}