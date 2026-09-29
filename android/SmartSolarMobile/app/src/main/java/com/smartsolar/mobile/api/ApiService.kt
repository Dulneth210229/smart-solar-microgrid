package com.smartsolar.mobile.api

import com.smartsolar.mobile.models.AuthResponse
import com.smartsolar.mobile.models.CreateReservationRequest
import com.smartsolar.mobile.models.EnergyBookingSlot
import com.smartsolar.mobile.models.LoginRequest
import com.smartsolar.mobile.models.MessageResponse
import com.smartsolar.mobile.models.ProsumerDashboardResponse
import com.smartsolar.mobile.models.RegisterProsumerRequest
import com.smartsolar.mobile.models.RegisterProsumerResponse
import com.smartsolar.mobile.models.ReservationActionResponse
import com.smartsolar.mobile.models.ReservationResponse
import com.smartsolar.mobile.models.SolarStation
import com.smartsolar.mobile.models.UpdateProfileRequest
import com.smartsolar.mobile.models.UpdateReservationRequest
import com.smartsolar.mobile.models.UserProfileResponse
import retrofit2.Call
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.PATCH
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query

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


    @GET("Stations")
    fun getActiveStations(
        @Header("Authorization")
        authorization: String
    ): Call<List<SolarStation>>


    @GET("BookingSlots/station/{stationId}")
    fun getAvailableBookingSlots(
        @Header("Authorization")
        authorization: String,

        @Path("stationId")
        stationId: String
    ): Call<List<EnergyBookingSlot>>


    @POST("Reservations")
    fun createReservation(
        @Header("Authorization")
        authorization: String,

        @Body
        request: CreateReservationRequest
    ): Call<ReservationResponse>


    @GET("Reservations/my")
    fun getMyReservations(
        @Header("Authorization")
        authorization: String
    ): Call<List<ReservationResponse>>


    @GET("Reservations/my/current")
    fun getCurrentReservations(
        @Header("Authorization")
        authorization: String
    ): Call<List<ReservationResponse>>


    @GET("Reservations/my/history")
    fun getReservationHistory(
        @Header("Authorization")
        authorization: String
    ): Call<List<ReservationResponse>>


    @GET("Reservations/my/search")
    fun searchReservations(
        @Header("Authorization")
        authorization: String,

        @Query("status")
        status: String?,

        @Query("search")
        search: String?,

        @Query("fromDate")
        fromDate: String? = null,

        @Query("toDate")
        toDate: String? = null
    ): Call<List<ReservationResponse>>


    @GET("Reservations/{id}")
    fun getReservation(
        @Header("Authorization")
        authorization: String,

        @Path("id")
        id: String
    ): Call<ReservationResponse>


    @PUT("Reservations/{id}")
    fun updateReservation(
        @Header("Authorization")
        authorization: String,

        @Path("id")
        id: String,

        @Body
        request: UpdateReservationRequest
    ): Call<ReservationActionResponse>


    @PATCH("Reservations/{id}/cancel")
    fun cancelReservation(
        @Header("Authorization")
        authorization: String,

        @Path("id")
        id: String
    ): Call<ReservationActionResponse>
}