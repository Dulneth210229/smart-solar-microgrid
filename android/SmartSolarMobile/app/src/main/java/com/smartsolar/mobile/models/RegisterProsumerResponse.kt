package com.smartsolar.mobile.models

data class RegisterProsumerResponse(
    val message: String,
    val userId: String,
    val nic: String?,
    val email: String,
    val role: String,
    val status: String
)