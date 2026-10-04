package com.smartsolar.mobile.models

data class RegisterProsumerRequest(
    val nic: String,
    val fullName: String,
    val email: String,
    val phoneNumber: String,
    val password: String
)