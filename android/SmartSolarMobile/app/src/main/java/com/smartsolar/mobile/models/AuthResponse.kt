package com.smartsolar.mobile.models

data class AuthResponse(
    val token: String,
    val userId: String,
    val fullName: String,
    val role: String,
    val status: String,
    val nic: String?,
    val email: String
)