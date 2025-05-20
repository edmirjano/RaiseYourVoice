package al.raiseyourvoice.android.ui.screen

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import al.raiseyourvoice.android.domain.usecase.auth.PasswordRecoveryUseCase
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class PasswordRecoveryViewModel @Inject constructor(
    private val passwordRecoveryUseCase: PasswordRecoveryUseCase
) : ViewModel() {
    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading

    private val _isSuccess = MutableStateFlow(false)
    val isSuccess: StateFlow<Boolean> = _isSuccess

    private val _error = MutableStateFlow<String?>(null)
    val error: StateFlow<String?> = _error

    fun recoverPassword(email: String) {
        _isLoading.value = true
        _error.value = null
        _isSuccess.value = false
        viewModelScope.launch {
            val result = passwordRecoveryUseCase(email)
            result.onSuccess {
                _isSuccess.value = true
            }.onFailure {
                _error.value = it.message
            }
            _isLoading.value = false
        }
    }
}
