package al.raiseyourvoice.android.data.util

import al.raiseyourvoice.android.domain.util.Resource
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import retrofit2.Response
import timber.log.Timber
import java.io.IOException

/**
 * Utility class for handling network responses and converting them to Resource objects
 */
object NetworkResponseHandler {
    
    /**
     * Handles API call and transforms response to Resource
     */
    fun <T, R> handleApiCall(
        apiCall: suspend () -> Response<T>,
        transform: (T) -> R
    ): Flow<Resource<R>> = flow {
        emit(Resource.Loading())
        
        try {
            val response = apiCall()
            
            if (response.isSuccessful) {
                val body = response.body()
                if (body != null) {
                    emit(Resource.Success(transform(body)))
                } else {
                    emit(Resource.Error(Exception("Empty response body"), "Response was successful but body was null"))
                }
            } else {
                val errorMessage = response.errorBody()?.string() ?: "Unknown error occurred"
                Timber.e("API Error: ${response.code()}, $errorMessage")
                emit(Resource.Error(Exception(errorMessage), "Error ${response.code()}: $errorMessage"))
            }
        } catch (e: IOException) {
            Timber.e(e, "Network error occurred")
            emit(Resource.Error(e, "Network error occurred. Please check your connection."))
        } catch (e: Exception) {
            Timber.e(e, "Error occurred during API call")
            emit(Resource.Error(e, e.localizedMessage ?: "An unexpected error occurred"))
        }
    }
    
    /**
     * Handles API call with Response<List<T>> and transforms each item to R
     */
    fun <T, R> handleApiCallList(
        apiCall: suspend () -> Response<List<T>>,
        transform: (T) -> R
    ): Flow<Resource<List<R>>> = flow {
        emit(Resource.Loading())
        
        try {
            val response = apiCall()
            
            if (response.isSuccessful) {
                val body = response.body()
                if (body != null) {
                    emit(Resource.Success(body.map { transform(it) }))
                } else {
                    emit(Resource.Error(Exception("Empty response body"), "Response was successful but body was null"))
                }
            } else {
                val errorMessage = response.errorBody()?.string() ?: "Unknown error occurred"
                Timber.e("API Error: ${response.code()}, $errorMessage")
                emit(Resource.Error(Exception(errorMessage), "Error ${response.code()}: $errorMessage"))
            }
        } catch (e: IOException) {
            Timber.e(e, "Network error occurred")
            emit(Resource.Error(e, "Network error occurred. Please check your connection."))
        } catch (e: Exception) {
            Timber.e(e, "Error occurred during API call")
            emit(Resource.Error(e, e.localizedMessage ?: "An unexpected error occurred"))
        }
    }
    
    /**
     * Handles API call for operations that return Response<Unit>
     */
    fun handleUnitApiCall(
        apiCall: suspend () -> Response<Unit>
    ): Flow<Resource<Boolean>> = flow {
        emit(Resource.Loading())
        
        try {
            val response = apiCall()
            
            if (response.isSuccessful) {
                emit(Resource.Success(true))
            } else {
                val errorMessage = response.errorBody()?.string() ?: "Unknown error occurred"
                Timber.e("API Error: ${response.code()}, $errorMessage")
                emit(Resource.Error(Exception(errorMessage), "Error ${response.code()}: $errorMessage"))
            }
        } catch (e: IOException) {
            Timber.e(e, "Network error occurred")
            emit(Resource.Error(e, "Network error occurred. Please check your connection."))
        } catch (e: Exception) {
            Timber.e(e, "Error occurred during API call")
            emit(Resource.Error(e, e.localizedMessage ?: "An unexpected error occurred"))
        }
    }
}