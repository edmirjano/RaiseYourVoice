package al.raiseyourvoice.android.data.remote

import retrofit2.Response
import retrofit2.http.*

/**
 * Base API service interface defining common HTTP methods
 */
interface ApiService {
    companion object {
        const val BASE_URL = "https://api.raiseyourvoice.al/api/v1/"
        const val AUTH_HEADER = "Authorization"
    }
}