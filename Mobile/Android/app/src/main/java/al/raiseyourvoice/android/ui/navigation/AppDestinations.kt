package al.raiseyourvoice.android.ui.navigation

/**
 * Object containing all navigation destinations in the app
 */
object AppDestinations {
    const val SPLASH = "splash"
    const val LOGIN = "login"
    const val REGISTER = "register"
    const val HOME = "home"
    const val PROFILE = "profile"
    const val POST_DETAIL = "post_detail"
    const val CREATE_POST = "create_post"
    const val SEARCH = "search"
    const val NOTIFICATIONS = "notifications"
    
    // Construct route with arguments
    fun postDetail(postId: String) = "$POST_DETAIL/$postId"
}