package com.smartsolar.mobile.models

data class ProsumerDashboardResponse(
    val currentReservations: Int,
    val pendingReservations: Int,
    val approvedFutureReservations: Int,
    val completedReservations: Int,
    val cancelledReservations: Int
)