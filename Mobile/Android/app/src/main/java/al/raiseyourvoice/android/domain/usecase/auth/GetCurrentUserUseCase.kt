package al.raiseyourvoice.android.domain.usecase.auth

import al.raiseyourvoice.android.domain.model.User
import al.raiseyourvoice.android.domain.repository.UserRepository
import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow
import javax.inject.Inject

/**
 * Use case for getting the current authenticated user
 */
class GetCurrentUserUseCase @Inject constructor(
    private val userRepository: UserRepository
) {
    suspend operator fun invoke(): Flow<Resource<User>> {
        return userRepository.getCurrentUser()
    }
}