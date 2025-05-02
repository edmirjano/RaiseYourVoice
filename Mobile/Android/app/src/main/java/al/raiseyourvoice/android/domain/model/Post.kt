package al.raiseyourvoice.android.domain.model

import java.util.Date

/**
 * Domain model representing a post in the application.
 */
data class Post(
    val id: String,
    val title: String,
    val content: String,
    val mediaUrls: List<String> = emptyList(),
    val postType: PostType,
    val authorId: String,
    val authorName: String,
    val authorProfilePicUrl: String? = null,
    val createdAt: Date,
    val updatedAt: Date? = null,
    val likeCount: Int = 0,
    val commentCount: Int = 0,
    val isLikedByCurrentUser: Boolean = false,
    val tags: List<String> = emptyList(),
    val location: Location? = null,
    val eventDate: Date? = null,
    val status: PostStatus = PostStatus.PUBLISHED
)

enum class PostType {
    ACTIVISM,
    OPPORTUNITY,
    SUCCESS_STORY
}

enum class PostStatus {
    PUBLISHED,
    DRAFT,
    REMOVED
}

data class Location(
    val latitude: Double,
    val longitude: Double,
    val address: String?,
    val city: String?,
    val country: String?
)