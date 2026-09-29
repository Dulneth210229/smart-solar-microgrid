package com.smartsolar.mobile.models

data class SolarStation(
    val id: String,
    val name: String,
    val location: String,
    val latitude: Double,
    val longitude: Double,
    val capacityKw: Double,
    val batteryCapacityKwh: Double,
    val totalBatterySlots: Int,
    val availableBatterySlots: Int,
    val openingTime: String,
    val closingTime: String,
    val operatingDays: List<String>,
    val isActive: Boolean
)