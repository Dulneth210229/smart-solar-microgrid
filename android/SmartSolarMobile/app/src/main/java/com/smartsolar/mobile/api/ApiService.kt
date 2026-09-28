package com.smartsolar.mobile.api

import com.smartsolar.mobile.models.AuthResponse
import com.smartsolar.mobile.models.LoginRequest
import com.smartsolar.mobile.models.MessageResponse
import com.smartsolar.mobile.models.ProsumerDashboardResponse
import com.smartsolar.mobile.models.RegisterProsumerRequest
import com.smartsolar.mobile.models.RegisterProsumerResponse
import com.smartsolar.mobile.models.UpdateProfileRequest
import com.smartsolar.mobile.models.UserProfileResponse
import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.PATCH
import retrofit2.http.POST
import retrofit2.http.PUT

interface ApiService {

    @POST("Auth/login")
    fun login(
        @Body request: LoginRequest
    ): Call<AuthResponse>


    @POST("Auth/register-prosumer")
    fun registerProsumer(
        @Body request: RegisterProsumerRequest
    ): Call<RegisterProsumerResponse>


    @GET("Users/me")
    fun getMyProfile(
        @Header("Authorization")
        authorization: String
    ): Call<UserProfileResponse>


    @PUT("Users/me")
    fun updateMyProfile(
        @Header("Authorization")
        authorization: String,

        @Body
        request: UpdateProfileRequest
    ): Call<MessageResponse>


    @PATCH("Users/me/request-deactivation")
    fun requestDeactivation(
        @Header("Authorization")
        authorization: String
    ): Call<MessageResponse>


    @GET("Dashboard/prosumer")
    fun getProsumerDashboard(
        @Header("Authorization")
        authorization: String
    ): Call<ProsumerDashboardResponse>
}