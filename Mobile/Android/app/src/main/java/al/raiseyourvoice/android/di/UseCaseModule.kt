package al.raiseyourvoice.android.di

import al.raiseyourvoice.android.domain.repository.UserRepository
import al.raiseyourvoice.android.domain.usecase.auth.GetCurrentUserUseCase
import al.raiseyourvoice.android.domain.usecase.auth.LoginUseCase
import al.raiseyourvoice.android.domain.usecase.auth.RegisterUseCase
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object UseCaseModule {
    
    @Provides
    @Singleton
    fun provideLoginUseCase(userRepository: UserRepository): LoginUseCase {
        return LoginUseCase(userRepository)
    }
    
    @Provides
    @Singleton
    fun provideRegisterUseCase(userRepository: UserRepository): RegisterUseCase {
        return RegisterUseCase(userRepository)
    }
    
    @Provides
    @Singleton
    fun provideGetCurrentUserUseCase(userRepository: UserRepository): GetCurrentUserUseCase {
        return GetCurrentUserUseCase(userRepository)
    }
    
    // TODO: Add more use case providers as they are implemented
}