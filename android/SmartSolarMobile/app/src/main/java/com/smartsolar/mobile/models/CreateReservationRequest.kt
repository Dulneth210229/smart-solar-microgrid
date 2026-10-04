package com.smartsolar.mobile.models

data class CreateReservationRequest(
    val slotId: String,
    val energyAmountKwh: Double,
    val transferType: String
)