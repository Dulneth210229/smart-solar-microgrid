package com.smartsolar.mobile.models

data class EnergyBookingSlot(
    val id: String,
    val stationId: String,
    val startTimeUtc: String,
    val endTimeUtc: String,
    val capacitySlots: Int,
    val isActive: Boolean,
    val createdAtUtc: String?,
    val updatedAtUtc: String?
)