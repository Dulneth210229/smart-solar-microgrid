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
import com.smartsolar.mobile.R
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.CreateReservationRequest
import com.smartsolar.mobile.models.ReservationResponse
import com.smartsolar.mobile.utils.DateTimeUtils
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.smartsolar.mobile.utils.ApiErrorUtils

class CreateReservationActivity :
    AppCompatActivity() {

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_create_reservation
        )

        val slotId =
            intent.getStringExtra(
                "slotId"
            ) ?: run {
                finish()
                return
            }

        val stationName =
            intent.getStringExtra(
                "stationName"
            ) ?: "Solar Station"

        val startTime =
            intent.getStringExtra(
                "startTime"
            ) ?: ""

        val endTime =
            intent.getStringExtra(
                "endTime"
            ) ?: ""

        findViewById<TextView>(
            R.id.textBookingStation
        ).text =
            stationName

        findViewById<TextView>(
            R.id.textBookingTime
        ).text =
            "${
                DateTimeUtils.formatUtcDateTime(
                    startTime
                )
            } - ${
                DateTimeUtils.formatUtcDateTime(
                    endTime
                )
            }"

        val spinner =
            findViewById<Spinner>(
                R.id.spinnerTransferType
            )

        val transferTypes =
            listOf(
                "DROP_OFF",
                "CHARGING"
            )

        spinner.adapter =
            ArrayAdapter(
                this,
                android.R.layout.simple_spinner_dropdown_item,
                transferTypes
            )

        val editEnergy =
            findViewById<EditText>(
                R.id.editEnergyAmount
            )

        val progress =
            findViewById<ProgressBar>(
                R.id.progressCreateReservation
            )

        val error =
            findViewById<TextView>(
                R.id.textCreateReservationError
            )

        val button =
            findViewById<Button>(
                R.id.buttonCreateReservation
            )

        findViewById<Button>(
            R.id.buttonCreateReservationBack
        ).setOnClickListener {
            finish()
        }

        button.setOnClickListener {

            val energy =
                editEnergy.text
                    .toString()
                    .toDoubleOrNull()

            if (
                energy == null ||
                energy <= 0
            ) {
                error.text =
                    "Enter a valid energy amount."

                return@setOnClickListener
            }

            val databaseHelper =
                DatabaseHelper(this)

            val session =
                databaseHelper.getSession()

            if (session == null) {
                finish()
                return@setOnClickListener
            }

            val request =
                CreateReservationRequest(
                    slotId = slotId,
                    energyAmountKwh = energy,
                    transferType =
                        spinner.selectedItem
                            .toString()
                )

            progress.visibility =
                View.VISIBLE

            button.isEnabled =
                false

            RetrofitClient
                .apiService
                .createReservation(
                    "Bearer ${session.token}",
                    request
                )
                .enqueue(
                    object :
                        Callback<ReservationResponse> {

                        override fun onResponse(
                            call:
                            Call<ReservationResponse>,
                            response:
                            Response<ReservationResponse>
                        ) {
                            progress.visibility =
                                View.GONE

                            button.isEnabled =
                                true

                            if (
                                response.isSuccessful &&
                                response.body() != null
                            ) {
                                AlertDialog.Builder(
                                    this@CreateReservationActivity
                                )
                                    .setTitle(
                                        "Reservation Created"
                                    )
                                    .setMessage(
                                        "Your reservation is now pending approval."
                                    )
                                    .setCancelable(false)
                                    .setPositiveButton(
                                        "My Bookings"
                                    ) { _, _ ->

                                        val bookingIntent =
                                            Intent(
                                                this@CreateReservationActivity,
                                                BookingsActivity::class.java
                                            )

                                        bookingIntent.flags =
                                            Intent.FLAG_ACTIVITY_CLEAR_TOP

                                        startActivity(
                                            bookingIntent
                                        )

                                        finish()
                                    }
                                    .show()

                            } else {
                                error.text =
                                    ApiErrorUtils.getMessage(
                                        response,
                                        "Unable to create reservation."
                                    )
                            }
                        }

                        override fun onFailure(
                            call:
                            Call<ReservationResponse>,
                            throwable:
                            Throwable
                        ) {
                            progress.visibility =
                                View.GONE

                            button.isEnabled =
                                true

                            error.text =
                                "Unable to connect to the server."
                        }
                    }
                )
        }
    }
}