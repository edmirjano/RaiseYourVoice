package al.raiseyourvoice.android.domain.model

import java.util.Date

/**
 * Domain model representing a user in the application.
 */
data class User(
    val id: String,
    val name: String,
    val email: String,
    val role: UserRole,
    val profilePictureUrl: String? = null,
    val bio: String? = null,
    val joinDate: Date,
    val lastLogin: Date? = null,
    val preferredLanguage: String = "en",
    val notificationSettings: NotificationSettings = NotificationSettings(),
)

enum class UserRole {
    ADMIN,
    MODERATOR,
    ACTIVIST,
    ORGANIZATION
}

data class NotificationSettings(
    val pushEnabled: Boolean = true,
    val emailEnabled: Boolean = true,
    val newPostNotifications: Boolean = true,
    val commentReplies: Boolean = true,
    val mentionNotifications: Boolean = true,
    val eventReminders: Boolean = true
)