package al.raiseyourvoice.android.ui.screen

import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import al.raiseyourvoice.android.ui.theme.RaiseYourVoiceTheme

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeScreen() {
    var currentTab by remember { mutableStateOf(HomeTab.FEED) }
    
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Raise Your Voice") },
                actions = {
                    IconButton(onClick = { /* TODO: Open search */ }) {
                        Icon(
                            imageVector = Icons.Default.Search,
                            contentDescription = "Search"
                        )
                    }
                    IconButton(onClick = { /* TODO: Open notifications */ }) {
                        Icon(
                            imageVector = Icons.Default.Notifications,
                            contentDescription = "Notifications"
                        )
                    }
                }
            )
        },
        bottomBar = {
            NavigationBar {
                HomeTab.values().forEach { tab ->
                    NavigationBarItem(
                        icon = { Icon(tab.icon, contentDescription = tab.title) },
                        label = { Text(tab.title) },
                        selected = currentTab == tab,
                        onClick = { currentTab = tab }
                    )
                }
            }
        },
        floatingActionButton = {
            if (currentTab == HomeTab.FEED) {
                FloatingActionButton(
                    onClick = { /* TODO: Open post creation screen */ }
                ) {
                    Icon(
                        imageVector = Icons.Default.Add,
                        contentDescription = "Create post"
                    )
                }
            }
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            when (currentTab) {
                HomeTab.FEED -> FeedTab()
                HomeTab.DISCOVER -> DiscoverTab()
                HomeTab.PROFILE -> ProfileTab()
            }
        }
    }
}

@Composable
fun FeedTab() {
    // Placeholder for the feed screen
    Box(modifier = Modifier.fillMaxSize()) {
        Text(
            text = "Feed Tab - Will show posts from followed users",
            modifier = Modifier.padding(16.dp)
        )
    }
}

@Composable
fun DiscoverTab() {
    // Placeholder for the discover screen
    Box(modifier = Modifier.fillMaxSize()) {
        Text(
            text = "Discover Tab - Will show trending and recommended posts",
            modifier = Modifier.padding(16.dp)
        )
    }
}

@Composable
fun ProfileTab() {
    // Placeholder for the profile screen
    Box(modifier = Modifier.fillMaxSize()) {
        Text(
            text = "Profile Tab - Will show user's profile and posts",
            modifier = Modifier.padding(16.dp)
        )
    }
}

enum class HomeTab(val title: String, val icon: ImageVector) {
    FEED("Feed", Icons.Default.Home),
    DISCOVER("Discover", Icons.Default.Explore),
    PROFILE("Profile", Icons.Default.Person)
}

@Preview(showBackground = true)
@Composable
fun HomeScreenPreview() {
    RaiseYourVoiceTheme {
        HomeScreen()
    }
}