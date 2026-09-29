package com.smartsolar.mobile.models

data class ReservationResponse(
    val id: String,
    val prosumerNic: String,
    val stationId: String,
    val stationName: String,
    val slotId: String,
    val startTimeUtc: String,
    val endTimeUtc: String,
    val energyAmountKwh: Double,
    val transferType: String,
    val status: String,
    val qrToken: String?,
    val createdAtUtc: String,
    val updatedAtUtc: String,
    val completedAtUtc: String?
)