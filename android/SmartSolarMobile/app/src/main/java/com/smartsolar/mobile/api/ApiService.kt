package com.smartsolar.mobile.api

import com.smartsolar.mobile.models.AuthResponse
import com.smartsolar.mobile.models.LoginRequest
import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.POST

interface ApiService {

    @POST("Auth/login")
    fun login(
        @Body request: LoginRequest
    ): Call<AuthResponse>
}