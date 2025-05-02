package al.raiseyourvoice.android.data.remote.dto

import al.raiseyourvoice.android.domain.model.Comment
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

/**
 * Data Transfer Object for Comment entity, used for API communication
 */
data class CommentDto(
    val id: String,
    val postId: String,
    val authorId: String,
    val authorName: String,
    val authorProfilePicUrl: String?,
    val content: String,
    val createdAt: String,
    val updatedAt: String?,
    val likeCount: Int,
    val isLikedByCurrentUser: Boolean,
    val parentCommentId: String?,
    val childCommentCount: Int
) {
    /**
     * Maps CommentDto to domain model Comment
     */
    fun toDomainModel(): Comment = Comment(
        id = id,
        postId = postId,
        authorId = authorId,
        authorName = authorName,
        authorProfilePicUrl = authorProfilePicUrl,
        content = content,
        createdAt = parseDate(createdAt),
        updatedAt = updatedAt?.let { parseDate(it) },
        likeCount = likeCount,
        isLikedByCurrentUser = isLikedByCurrentUser,
        parentCommentId = parentCommentId,
        childCommentCount = childCommentCount
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
 * Request object for creating or updating a comment
 */
data class CreateCommentRequest(
    val content: String
)

/**
 * Extension function to convert domain Comment model to CommentDto
 */
fun Comment.toDto(): CommentDto = CommentDto(
    id = id,
    postId = postId,
    authorId = authorId,
    authorName = authorName,
    authorProfilePicUrl = authorProfilePicUrl,
    content = content,
    createdAt = formatDate(createdAt),
    updatedAt = updatedAt?.let { formatDate(it) },
    likeCount = likeCount,
    isLikedByCurrentUser = isLikedByCurrentUser,
    parentCommentId = parentCommentId,
    childCommentCount = childCommentCount
)

/**
 * Helper function to format Date to ISO8601 string
 */
private fun formatDate(date: Date): String {
    return SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSXXX", Locale.US).format(date)
}