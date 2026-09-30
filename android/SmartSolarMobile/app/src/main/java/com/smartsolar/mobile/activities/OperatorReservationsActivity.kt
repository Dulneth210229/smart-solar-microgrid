package com.smartsolar.mobile.activities

import android.app.AlertDialog
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.adapters.OperatorReservationAdapter
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.ReservationActionResponse
import com.smartsolar.mobile.models.ReservationResponse
import com.smartsolar.mobile.utils.ApiErrorUtils
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class OperatorReservationsActivity :
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
            R.layout.activity_operator_reservations
        )

        databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (
            session == null ||
            session.role != "GRID_OPERATOR"
        ) {
            finish()
            return
        }

        recycler =
            findViewById(
                R.id.recyclerOperatorReservations
            )

        recycler.layoutManager =
            LinearLayoutManager(this)

        progress =
            findViewById(
                R.id.progressOperatorReservations
            )

        error =
            findViewById(
                R.id.textOperatorReservationError
            )

        findViewById<Button>(
            R.id.buttonRefreshOperatorReservations
        ).setOnClickListener {
            loadReservations()
        }

        findViewById<Button>(
            R.id.buttonOperatorReservationsBack
        ).setOnClickListener {
            finish()
        }

        loadReservations()
    }


    override fun onResume() {
        super.onResume()

        if (::databaseHelper.isInitialized) {
            loadReservations()
        }
    }


    private fun loadReservations() {

        val session =
            databaseHelper.getSession()
                ?: return

        progress.visibility =
            View.VISIBLE

        error.text = ""

        RetrofitClient
            .apiService
            .getPendingReservations(
                "Bearer ${session.token}"
            )
            .enqueue(
                object :
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
                            val reservations =
                                response.body()!!

                            error.text =
                                if (reservations.isEmpty()) {
                                    "No pending reservations."
                                } else {
                                    ""
                                }

                            recycler.adapter =
                                OperatorReservationAdapter(
                                    reservations,

                                    onApprove = {
                                            reservation ->

                                        confirmApprove(
                                            reservation
                                        )
                                    },

                                    onCancel = {
                                            reservation ->

                                        confirmCancel(
                                            reservation
                                        )
                                    }
                                )

                        } else {
                            error.text =
                                ApiErrorUtils.getMessage(
                                    response,
                                    "Unable to load reservations."
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
            )
    }


    private fun confirmApprove(
        reservation:
        ReservationResponse
    ) {
        AlertDialog.Builder(this)
            .setTitle(
                "Approve Reservation"
            )
            .setMessage(
                "Approve this reservation for ${reservation.prosumerNic}?"
            )
            .setNegativeButton(
                "No",
                null
            )
            .setPositiveButton(
                "Approve"
            ) { _, _ ->

                approveReservation(
                    reservation.id
                )
            }
            .show()
    }


    private fun approveReservation(
        id: String
    ) {
        val session =
            databaseHelper.getSession()
                ?: return

        progress.visibility =
            View.VISIBLE

        error.text = ""

        RetrofitClient
            .apiService
            .approveReservation(
                "Bearer ${session.token}",
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

                        if (response.isSuccessful) {

                            AlertDialog.Builder(
                                this@OperatorReservationsActivity
                            )
                                .setMessage(
                                    "Reservation approved successfully."
                                )
                                .setPositiveButton(
                                    "OK"
                                ) { _, _ ->
                                    loadReservations()
                                }
                                .show()

                        } else {
                            error.text =
                                ApiErrorUtils.getMessage(
                                    response,
                                    "Unable to approve reservation."
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


    private fun confirmCancel(
        reservation:
        ReservationResponse
    ) {
        AlertDialog.Builder(this)
            .setTitle(
                "Cancel Reservation"
            )
            .setMessage(
                "Cancel this reservation?"
            )
            .setNegativeButton(
                "No",
                null
            )
            .setPositiveButton(
                "Cancel"
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
        val session =
            databaseHelper.getSession()
                ?: return

        progress.visibility =
            View.VISIBLE

        error.text = ""

        RetrofitClient
            .apiService
            .cancelReservation(
                "Bearer ${session.token}",
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

                        if (response.isSuccessful) {

                            AlertDialog.Builder(
                                this@OperatorReservationsActivity
                            )
                                .setMessage(
                                    "Reservation cancelled successfully."
                                )
                                .setPositiveButton(
                                    "OK"
                                ) { _, _ ->
                                    loadReservations()
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
}