package com.smartsolar.mobile.models

data class UpdateProfileRequest(
    val fullName: String,
    val email: String,
    val phoneNumber: String
)