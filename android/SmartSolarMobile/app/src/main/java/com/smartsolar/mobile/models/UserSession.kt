package com.smartsolar.mobile.models

data class UserSession(
    val userId: String,
    val nic: String?,
    val fullName: String,
    val email: String,
    val role: String,
    val token: String
)