package al.raiseyourvoice.android.ui.theme

import android.app.Activity
import android.os.Build
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.dynamicDarkColorScheme
import androidx.compose.material3.dynamicLightColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat

// iOS-inspired colors as specified in the requirements
private val IOSDarkColorScheme = darkColorScheme(
    primary = Color(0xFF212124), // iOS black as primary color
    secondary = Color(0xFF007AFF), // iOS blue
    tertiary = Color(0xFFFF9500), // iOS orange
    background = Color(0xFF000000),
    surface = Color(0xFF1C1C1E),
)

private val IOSLightColorScheme = lightColorScheme(
    primary = Color(0xFF212124), // iOS black as primary color
    secondary = Color(0xFF007AFF), // iOS blue
    tertiary = Color(0xFFFF9500), // iOS orange
    background = Color(0xFFF2F2F7),
    surface = Color(0xFFFFFFFF),
)

@Composable
fun RaiseYourVoiceTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    // Dynamic color is available on Android 12+
    dynamicColor: Boolean = false,
    content: @Composable () -> Unit
) {
    val colorScheme = when {
        dynamicColor && Build.VERSION.SDK_INT >= Build.VERSION_CODES.S -> {
            val context = LocalContext.current
            if (darkTheme) dynamicDarkColorScheme(context) else dynamicLightColorScheme(context)
        }
        darkTheme -> IOSDarkColorScheme
        else -> IOSLightColorScheme
    }
    val view = LocalView.current
    if (!view.isInEditMode) {
        SideEffect {
            val window = (view.context as Activity).window
            window.statusBarColor = colorScheme.primary.toArgb()
            WindowCompat.getInsetsController(window, view).isAppearanceLightStatusBars = !darkTheme
        }
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content
    )
}