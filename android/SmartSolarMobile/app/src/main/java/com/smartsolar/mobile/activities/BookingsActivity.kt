package com.smartsolar.mobile.activities

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.EditText
import android.widget.ProgressBar
import android.widget.Spinner
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.adapters.ReservationAdapter
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.ReservationActionResponse
import com.smartsolar.mobile.models.ReservationResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.smartsolar.mobile.utils.ApiErrorUtils

class BookingsActivity :
    AppCompatActivity() {

    private lateinit var databaseHelper:
            DatabaseHelper

    private lateinit var recycler:
            RecyclerView

    private lateinit var progress:
            ProgressBar

    private lateinit var error:
            TextView

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_bookings
        )

        databaseHelper =
            DatabaseHelper(this)

        recycler =
            findViewById(
                R.id.recyclerBookings
            )

        recycler.layoutManager =
            LinearLayoutManager(this)

        progress =
            findViewById(
                R.id.progressBookings
            )

        error =
            findViewById(
                R.id.textBookingsError
            )

        val statusSpinner =
            findViewById<Spinner>(
                R.id.spinnerBookingStatus
            )

        val statuses =
            listOf(
                "ALL",
                "PENDING",
                "APPROVED",
                "CANCELLED",
                "COMPLETED"
            )

        statusSpinner.adapter =
            ArrayAdapter(
                this,
                android.R.layout.simple_spinner_dropdown_item,
                statuses
            )

        findViewById<Button>(
            R.id.buttonCurrentBookings
        ).setOnClickListener {
            loadCurrentBookings()
        }

        findViewById<Button>(
            R.id.buttonPendingBookings
        ).setOnClickListener {
            searchBookings(
                "PENDING",
                null
            )
        }

        findViewById<Button>(
            R.id.buttonHistoryBookings
        ).setOnClickListener {
            loadHistory()
        }

        findViewById<Button>(
            R.id.buttonSearchBookings
        ).setOnClickListener {

            val search =
                findViewById<EditText>(
                    R.id.editBookingSearch
                )
                    .text
                    .toString()
                    .trim()

            val selectedStatus =
                statusSpinner
                    .selectedItem
                    .toString()

            searchBookings(
                if (
                    selectedStatus ==
                    "ALL"
                ) {
                    null
                } else {
                    selectedStatus
                },

                if (
                    search.isEmpty()
                ) {
                    null
                } else {
                    search
                }
            )
        }

        findViewById<Button>(
            R.id.buttonBookingsBack
        ).setOnClickListener {
            finish()
        }

        loadCurrentBookings()

        findViewById<Button>(
            R.id.buttonRefreshBookings
        ).setOnClickListener {

            loadCurrentBookings()
        }
    }

    override fun onResume() {
        super.onResume()

        if (
            ::recycler.isInitialized &&
            ::databaseHelper.isInitialized
        ) {
            loadCurrentBookings()
        }
    }

    private fun token():
            String? {

        return databaseHelper
            .getSession()
            ?.token
    }

    private fun loadCurrentBookings() {

        val token =
            token() ?: return

        showLoading()

        RetrofitClient
            .apiService
            .getCurrentReservations(
                "Bearer $token"
            )
            .enqueue(
                reservationListCallback()
            )
    }

    private fun loadHistory() {

        val token =
            token() ?: return

        showLoading()

        RetrofitClient
            .apiService
            .getReservationHistory(
                "Bearer $token"
            )
            .enqueue(
                reservationListCallback()
            )
    }

    private fun searchBookings(
        status: String?,
        search: String?
    ) {
        val token =
            token() ?: return

        showLoading()

        RetrofitClient
            .apiService
            .searchReservations(
                authorization =
                    "Bearer $token",

                status = status,
                search = search
            )
            .enqueue(
                reservationListCallback()
            )
    }

    private fun reservationListCallback():
            Callback<List<ReservationResponse>> {

        return object :
            Callback<List<ReservationResponse>> {

            override fun onResponse(
                call:
                Call<List<ReservationResponse>>,
                response:
                Response<List<ReservationResponse>>
            ) {
                progress.visibility =
                    View.GONE

                if (
                    response.isSuccessful &&
                    response.body() != null
                ) {
                    displayReservations(
                        response.body()!!
                    )

                } else {
                    error.text =
                        "Unable to load bookings."
                }
            }

            override fun onFailure(
                call:
                Call<List<ReservationResponse>>,
                throwable:
                Throwable
            ) {
                progress.visibility =
                    View.GONE

                error.text =
                    "Unable to connect to the server."
            }
        }
    }

    private fun displayReservations(
        reservations:
        List<ReservationResponse>
    ) {
        error.text =
            if (reservations.isEmpty()) {
                "No bookings found."
            } else {
                ""
            }

        recycler.adapter =
            ReservationAdapter(
                reservations = reservations,

                onEdit = { reservation ->

                    val intent =
                        Intent(
                            this,
                            EditReservationActivity::class.java
                        )

                    intent.putExtra(
                        "reservationId",
                        reservation.id
                    )

                    intent.putExtra(
                        "stationId",
                        reservation.stationId
                    )

                    intent.putExtra(
                        "stationName",
                        reservation.stationName
                    )

                    intent.putExtra(
                        "slotId",
                        reservation.slotId
                    )

                    intent.putExtra(
                        "energyAmount",
                        reservation.energyAmountKwh
                    )

                    intent.putExtra(
                        "transferType",
                        reservation.transferType
                    )

                    startActivity(intent)
                },

                onCancel = { reservation ->

                    confirmCancellation(
                        reservation
                    )
                }
            )
    }

    private fun confirmCancellation(
        reservation:
        ReservationResponse
    ) {
        AlertDialog.Builder(this)
            .setTitle(
                "Cancel Reservation"
            )
            .setMessage(
                "Are you sure you want to cancel this reservation?"
            )
            .setNegativeButton(
                "No",
                null
            )
            .setPositiveButton(
                "Yes"
            ) { _, _ ->

                cancelReservation(
                    reservation.id
                )
            }
            .show()
    }

    private fun cancelReservation(
        id: String
    ) {
        val token =
            token() ?: return

        showLoading()

        RetrofitClient
            .apiService
            .cancelReservation(
                "Bearer $token",
                id
            )
            .enqueue(
                object :
                    Callback<ReservationActionResponse> {

                    override fun onResponse(
                        call:
                        Call<ReservationActionResponse>,
                        response:
                        Response<ReservationActionResponse>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (
                            response.isSuccessful
                        ) {
                            AlertDialog.Builder(
                                this@BookingsActivity
                            )
                                .setMessage(
                                    "Reservation cancelled successfully."
                                )
                                .setPositiveButton(
                                    "OK"
                                ) { _, _ ->
                                    loadCurrentBookings()
                                }
                                .show()
                        } else {
                            error.text =
                                ApiErrorUtils.getMessage(
                                    response,
                                    "Unable to cancel reservation."
                                )
                        }
                    }

                    override fun onFailure(
                        call:
                        Call<ReservationActionResponse>,
                        throwable:
                        Throwable
                    ) {
                        progress.visibility =
                            View.GONE

                        error.text =
                            "Unable to connect to the server."
                    }
                }
            )
    }

    private fun showLoading() {
        progress.visibility =
            View.VISIBLE

        error.text = ""
    }
}