package al.raiseyourvoice.android.domain.repository

import kotlinx.coroutines.flow.Flow
import al.raiseyourvoice.android.domain.util.Resource

/**
 * Base repository interface that defines common operations for data access
 */
interface BaseRepository<T, ID> {
    suspend fun getById(id: ID): Flow<Resource<T>>
    suspend fun getAll(): Flow<Resource<List<T>>>
    suspend fun create(item: T): Flow<Resource<T>>
    suspend fun update(item: T): Flow<Resource<T>>
    suspend fun delete(id: ID): Flow<Resource<Boolean>>
}