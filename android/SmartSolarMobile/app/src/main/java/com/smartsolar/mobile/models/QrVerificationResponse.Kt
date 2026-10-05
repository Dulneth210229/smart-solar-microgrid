package com.smartsolar.mobile.models

data class QrVerificationResponse(
    val valid: Boolean,
    val reservationId: String,
    val prosumerNic: String,
    val stationName: String,
    val startTimeUtc: String,
    val endTimeUtc: String,
    val energyAmountKwh: Double,
    val transferType: String,
    val status: String
)