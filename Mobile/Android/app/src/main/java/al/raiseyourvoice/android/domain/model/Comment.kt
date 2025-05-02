package al.raiseyourvoice.android.domain.model

import java.util.Date

/**
 * Domain model representing a comment in the application.
 */
data class Comment(
    val id: String,
    val postId: String,
    val authorId: String,
    val authorName: String,
    val authorProfilePicUrl: String? = null,
    val content: String,
    val createdAt: Date,
    val updatedAt: Date? = null,
    val likeCount: Int = 0,
    val isLikedByCurrentUser: Boolean = false,
    val parentCommentId: String? = null,
    val childCommentCount: Int = 0
)