package al.raiseyourvoice.android.domain.util

/**
 * A generic class that holds a value with its loading status.
 * @param <T> Type of the resource data
 */
sealed class Resource<out T> {
    data class Success<out T>(val data: T) : Resource<T>()
    data class Error(val exception: Throwable, val message: String? = null) : Resource<Nothing>()
    data class Loading<out T>(val data: T? = null) : Resource<T>()

    companion object {
        fun <T> success(data: T): Resource<T> = Success(data)
        fun error(exception: Throwable, message: String? = null): Resource<Nothing> = Error(exception, message)
        fun <T> loading(data: T? = null): Resource<T> = Loading(data)
    }
}