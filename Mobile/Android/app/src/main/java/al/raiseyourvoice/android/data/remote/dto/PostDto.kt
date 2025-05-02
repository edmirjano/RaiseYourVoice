package al.raiseyourvoice.android.data.remote.dto

import al.raiseyourvoice.android.domain.model.Location
import al.raiseyourvoice.android.domain.model.Post
import al.raiseyourvoice.android.domain.model.PostStatus
import al.raiseyourvoice.android.domain.model.PostType
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Data Transfer Object for Post entity, used for API communication
 */
data class PostDto(
    val id: String,
    val title: String,
    val content: String,
    val mediaUrls: List<String>,
    val type: String,
    val authorId: String,
    val authorName: String,
    val authorProfilePicUrl: String?,
    val createdAt: String,
    val updatedAt: String?,
    val likeCount: Int,
    val commentCount: Int,
    val isLikedByCurrentUser: Boolean,
    val tags: List<String>,
    val location: LocationDto?,
    val eventDate: String?,
    val status: String
) {
    /**
     * Maps PostDto to domain model Post
     */
    fun toDomainModel(): Post = Post(
        id = id,
        title = title,
        content = content,
        mediaUrls = mediaUrls,
        postType = PostType.valueOf(type),
        authorId = authorId,
        authorName = authorName,
        authorProfilePicUrl = authorProfilePicUrl,
        createdAt = parseDate(createdAt),
        updatedAt = updatedAt?.let { parseDate(it) },
        likeCount = likeCount,
        commentCount = commentCount,
        isLikedByCurrentUser = isLikedByCurrentUser,
        tags = tags,
        location = location?.toDomainModel(),
        eventDate = eventDate?.let { parseDate(it) },
        status = PostStatus.valueOf(status)
    )
    
    private fun parseDate(dateString: String): Date {
        // In a real app, use a proper date parser with error handling
        return try {
            SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSXXX", Locale.US).parse(dateString) ?: Date()
        } catch (e: Exception) {
            Date()
        }
    }
}

/**
 * Data Transfer Object for Location
 */
data class LocationDto(
    val latitude: Double,
    val longitude: Double,
    val address: String?,
    val city: String?,
    val country: String?
) {
    fun toDomainModel(): Location = Location(
        latitude = latitude,
        longitude = longitude,
        address = address,
        city = city,
        country = country
    )
}

/**
 * Request object for creating or updating a post
 */
data class CreatePostRequest(
    val title: String,
    val content: String,
    val mediaUrls: List<String> = emptyList(),
    val type: String,
    val tags: List<String> = emptyList(),
    val location: LocationDto? = null,
    val eventDate: String? = null
)

/**
 * Extension function to convert domain Post model to PostDto
 */
fun Post.toDto(): PostDto = PostDto(
    id = id,
    title = title,
    content = content,
    mediaUrls = mediaUrls,
    type = postType.name,
    authorId = authorId,
    authorName = authorName,
    authorProfilePicUrl = authorProfilePicUrl,
    createdAt = formatDate(createdAt),
    updatedAt = updatedAt?.let { formatDate(it) },
    likeCount = likeCount,
    commentCount = commentCount,
    isLikedByCurrentUser = isLikedByCurrentUser,
    tags = tags,
    location = location?.toDto(),
    eventDate = eventDate?.let { formatDate(it) },
    status = status.name
)

/**
 * Extension function to convert domain Location to LocationDto
 */
fun Location.toDto(): LocationDto = LocationDto(
    latitude = latitude,
    longitude = longitude,
    address = address,
    city = city,
    country = country
)

/**
 * Helper function to format Date to ISO8601 string
 */
private fun formatDate(date: Date): String {
    return SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSXXX", Locale.US).format(date)
}