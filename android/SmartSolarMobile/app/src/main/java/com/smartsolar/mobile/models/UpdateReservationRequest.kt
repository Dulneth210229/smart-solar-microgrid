package com.smartsolar.mobile.models

data class UpdateReservationRequest(
    val slotId: String,
    val energyAmountKwh: Double,
    val transferType: String
)