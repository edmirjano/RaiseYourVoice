package al.raiseyourvoice.android.di

import al.raiseyourvoice.android.data.repository.UserRepositoryImpl
import al.raiseyourvoice.android.domain.repository.UserRepository
import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class RepositoryModule {
    
    @Binds
    @Singleton
    abstract fun bindUserRepository(
        userRepositoryImpl: UserRepositoryImpl
    ): UserRepository
    
    // TODO: Add bindings for PostRepository and CommentRepository
}