package com.smartsolar.mobile.models

data class UserProfileResponse(
    val id: String,
    val nic: String?,
    val fullName: String,
    val email: String,
    val phoneNumber: String,
    val role: String,
    val status: String,
    val createdAtUtc: String?
)