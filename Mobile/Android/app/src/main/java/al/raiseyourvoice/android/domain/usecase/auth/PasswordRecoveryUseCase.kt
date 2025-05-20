package al.raiseyourvoice.android.domain.usecase.auth

import al.raiseyourvoice.android.domain.repository.UserRepository
import javax.inject.Inject

class PasswordRecoveryUseCase @Inject constructor(
    private val userRepository: UserRepository
) {
    suspend operator fun invoke(email: String): Result<Unit> {
        return try {
            userRepository.requestPasswordReset(email)
            Result.success(Unit)
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}
