package al.raiseyourvoice.android

import android.app.Application
import android.os.StrictMode
import timber.log.Timber

class RaiseYourVoiceApplication : Application() {
    
    override fun onCreate() {
        super.onCreate()
        
        // Initialize Timber for logging
        if (BuildConfig.DEBUG) {
            Timber.plant(Timber.DebugTree())
            
            // Enable StrictMode in debug builds for better development practices
            StrictMode.setThreadPolicy(
                StrictMode.ThreadPolicy.Builder()
                    .detectAll()
                    .penaltyLog()
                    .build()
            )
            
            StrictMode.setVmPolicy(
                StrictMode.VmPolicy.Builder()
                    .detectLeakedSqlLiteObjects()
                    .detectLeakedClosableObjects()
                    .penaltyLog()
                    .build()
            )
        }
        
        // TODO: Initialize dependency injection
        // TODO: Initialize crash reporting
        // TODO: Initialize analytics
    }
}