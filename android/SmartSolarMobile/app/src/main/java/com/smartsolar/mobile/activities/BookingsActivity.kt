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
import com.smartsolar.mobile.utils.ApiErrorUtils
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

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


        /*
         * Configure the status filter spinner.
         */
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


        /*
         * Loads current active reservations.
         */
        findViewById<Button>(
            R.id.buttonCurrentBookings
        ).setOnClickListener {

            loadCurrentBookings()
        }


        /*
         * Loads only pending reservations.
         */
        findViewById<Button>(
            R.id.buttonPendingBookings
        ).setOnClickListener {

            searchBookings(
                "PENDING",
                null
            )
        }


        /*
         * Loads completed, cancelled and historical reservations.
         */
        findViewById<Button>(
            R.id.buttonHistoryBookings
        ).setOnClickListener {

            loadHistory()
        }


        /*
         * Searches reservations using the selected status
         * and optional search text.
         */
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


        /*
         * Returns to the previous screen.
         */
        findViewById<Button>(
            R.id.buttonBookingsBack
        ).setOnClickListener {

            finish()
        }


        /*
         * Refreshes booking information manually.
         */
        findViewById<Button>(
            R.id.buttonRefreshBookings
        ).setOnClickListener {

            loadCurrentBookings()
        }


        /*
         * Load current bookings when the activity first opens.
         */
        loadCurrentBookings()
    }


    /*
     * Reload booking information when returning from
     * another activity, for example after modification.
     */
    override fun onResume() {
        super.onResume()

        if (
            ::recycler.isInitialized &&
            ::databaseHelper.isInitialized
        ) {
            loadCurrentBookings()
        }
    }


    /*
     * Returns the JWT token stored in the local SQLite session.
     */
    private fun token():
            String? {

        return databaseHelper
            .getSession()
            ?.token
    }


    /*
     * Retrieves all current pending and approved reservations.
     */
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


    /*
     * Retrieves reservation history from the Web API.
     */
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


    /*
     * Searches and filters the Prosumer's reservations.
     */
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


    /*
     * Provides the common Retrofit callback used by
     * current, history and search reservation requests.
     */
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
                        ApiErrorUtils.getMessage(
                            response,
                            "Unable to load bookings."
                        )
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


    /*
     * Displays reservation information inside the RecyclerView
     * and handles Modify, Cancel and Show QR actions.
     */
    private fun displayReservations(
        reservations:
        List<ReservationResponse>
    ) {
        error.text =
            if (
                reservations.isEmpty()
            ) {
                "No bookings found."
            } else {
                ""
            }


        recycler.adapter =
            ReservationAdapter(

                reservations =
                    reservations,


                /*
                 * Opens the reservation modification screen.
                 */
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

                    startActivity(
                        intent
                    )
                },


                /*
                 * Requests confirmation before cancelling
                 * the selected reservation.
                 */
                onCancel = { reservation ->

                    confirmCancellation(
                        reservation
                    )
                },


                /*
                 * Opens the QR screen for an approved reservation.
                 */
                onShowQr = { reservation ->

                    val intent =
                        Intent(
                            this,
                            ReservationQrActivity::class.java
                        )

                    intent.putExtra(
                        "reservationId",
                        reservation.id
                    )

                    intent.putExtra(
                        "stationName",
                        reservation.stationName
                    )

                    intent.putExtra(
                        "startTime",
                        reservation.startTimeUtc
                    )

                    intent.putExtra(
                        "energyAmount",
                        reservation.energyAmountKwh
                    )

                    intent.putExtra(
                        "transferType",
                        reservation.transferType
                    )

                    intent.putExtra(
                        "qrToken",
                        reservation.qrToken
                    )

                    startActivity(
                        intent
                    )
                }
            )
    }


    /*
     * Displays a confirmation dialog before a
     * reservation cancellation is submitted.
     */
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


    /*
     * Sends the cancellation request to the central Web API.
     * The API enforces the 12-hour cancellation rule.
     */
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
                                .setTitle(
                                    "Reservation Cancelled"
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


    /*
     * Displays the loading indicator while hiding
     * any previous API error message.
     */
    private fun showLoading() {

        progress.visibility =
            View.VISIBLE

        error.text =
            ""
    }
}