package al.raiseyourvoice.android.domain.usecase.auth

import al.raiseyourvoice.android.domain.model.User
import al.raiseyourvoice.android.domain.repository.UserRepository
import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

/**
 * Use case for registering a new user
 */
class RegisterUseCase @Inject constructor(
    private val userRepository: UserRepository
) {
    suspend operator fun invoke(name: String, email: String, password: String): Flow<Resource<User>> {
        return userRepository.register(name, email, password)
    }
}