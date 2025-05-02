package al.raiseyourvoice.android.data.remote.dto

import al.raiseyourvoice.android.domain.model.NotificationSettings
import al.raiseyourvoice.android.domain.model.User
import al.raiseyourvoice.android.domain.model.UserRole
import java.util.Date

/**
 * Data Transfer Object for User entity, used for API communication
 */
data class UserDto(
    val id: String,
    val name: String,
    val email: String,
    val role: String,
    val profilePictureUrl: String?,
    val bio: String?,
    val joinDate: String,
    val lastLogin: String?,
    val preferredLanguage: String,
    val notificationSettings: NotificationSettingsDto
) {
    /**
     * Maps UserDto to domain model User
     */
    fun toDomainModel(): User = User(
        id = id,
        name = name,
        email = email,
        role = UserRole.valueOf(role),
        profilePictureUrl = profilePictureUrl,
        bio = bio,
        joinDate = parseDate(joinDate),
        lastLogin = lastLogin?.let { parseDate(it) },
        preferredLanguage = preferredLanguage,
        notificationSettings = notificationSettings.toDomainModel()
    )
    
    private fun parseDate(dateString: String): Date {
        // Simple implementation - in a real app, use proper date parsing
        return Date(dateString)
    }
}

/**
 * Data Transfer Object for NotificationSettings
 */
data class NotificationSettingsDto(
    val pushEnabled: Boolean,
    val emailEnabled: Boolean,
    val newPostNotifications: Boolean,
    val commentReplies: Boolean,
    val mentionNotifications: Boolean,
    val eventReminders: Boolean
) {
    fun toDomainModel(): NotificationSettings = NotificationSettings(
        pushEnabled = pushEnabled,
        emailEnabled = emailEnabled,
        newPostNotifications = newPostNotifications,
        commentReplies = commentReplies,
        mentionNotifications = mentionNotifications,
        eventReminders = eventReminders
    )
}

/**
 * Extension function to convert domain User model to UserDto
 */
fun User.toDto(): UserDto = UserDto(
    id = id,
    name = name,
    email = email,
    role = role.name,
    profilePictureUrl = profilePictureUrl,
    bio = bio,
    joinDate = joinDate.toString(),
    lastLogin = lastLogin?.toString(),
    preferredLanguage = preferredLanguage,
    notificationSettings = notificationSettings.toDto()
)

/**
 * Extension function to convert domain NotificationSettings to NotificationSettingsDto
 */
fun NotificationSettings.toDto(): NotificationSettingsDto = NotificationSettingsDto(
    pushEnabled = pushEnabled,
    emailEnabled = emailEnabled,
    newPostNotifications = newPostNotifications,
    commentReplies = commentReplies,
    mentionNotifications = mentionNotifications,
    eventReminders = eventReminders
)