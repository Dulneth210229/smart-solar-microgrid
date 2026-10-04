package com.smartsolar.mobile.activities

import android.app.AlertDialog
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
import com.smartsolar.mobile.models.EnergyBookingSlot
import com.smartsolar.mobile.models.ReservationActionResponse
import com.smartsolar.mobile.models.UpdateReservationRequest
import com.smartsolar.mobile.utils.DateTimeUtils
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import com.smartsolar.mobile.utils.ApiErrorUtils

class EditReservationActivity :
    AppCompatActivity() {

    private var availableSlots:
            List<EnergyBookingSlot> =
        emptyList()

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_edit_reservation
        )

        val reservationId =
            intent.getStringExtra(
                "reservationId"
            ) ?: run {
                finish()
                return
            }

        val stationId =
            intent.getStringExtra(
                "stationId"
            ) ?: run {
                finish()
                return
            }

        val stationName =
            intent.getStringExtra(
                "stationName"
            ) ?: "Station"

        val currentSlotId =
            intent.getStringExtra(
                "slotId"
            )

        val currentEnergy =
            intent.getDoubleExtra(
                "energyAmount",
                0.0
            )

        val currentType =
            intent.getStringExtra(
                "transferType"
            ) ?: "DROP_OFF"

        findViewById<TextView>(
            R.id.textEditStation
        ).text =
            stationName

        val editEnergy =
            findViewById<EditText>(
                R.id.editReservationEnergy
            )

        editEnergy.setText(
            currentEnergy.toString()
        )

        val transferSpinner =
            findViewById<Spinner>(
                R.id.spinnerEditTransferType
            )

        val types =
            listOf(
                "DROP_OFF",
                "CHARGING"
            )

        transferSpinner.adapter =
            ArrayAdapter(
                this,
                android.R.layout.simple_spinner_dropdown_item,
                types
            )

        transferSpinner.setSelection(
            types.indexOf(
                currentType
            ).coerceAtLeast(0)
        )

        findViewById<Button>(
            R.id.buttonEditReservationBack
        ).setOnClickListener {
            finish()
        }

        loadSlots(
            stationId,
            currentSlotId
        )

        findViewById<Button>(
            R.id.buttonSaveReservation
        ).setOnClickListener {

            saveReservation(
                reservationId,
                editEnergy,
                transferSpinner
            )
        }
    }

    private fun loadSlots(
        stationId: String,
        currentSlotId: String?
    ) {
        val session =
            DatabaseHelper(this)
                .getSession()
                ?: return

        val progress =
            findViewById<ProgressBar>(
                R.id.progressEditReservation
            )

        val error =
            findViewById<TextView>(
                R.id.textEditReservationError
            )

        progress.visibility =
            View.VISIBLE

        RetrofitClient
            .apiService
            .getAvailableBookingSlots(
                "Bearer ${session.token}",
                stationId
            )
            .enqueue(
                object :
                    Callback<List<EnergyBookingSlot>> {

                    override fun onResponse(
                        call:
                        Call<List<EnergyBookingSlot>>,
                        response:
                        Response<List<EnergyBookingSlot>>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (
                            response.isSuccessful &&
                            response.body() != null
                        ) {
                            availableSlots =
                                response.body()!!

                            val labels =
                                availableSlots.map {
                                    "${
                                        DateTimeUtils
                                            .formatUtcDateTime(
                                                it.startTimeUtc
                                            )
                                    } - ${
                                        DateTimeUtils
                                            .formatUtcDateTime(
                                                it.endTimeUtc
                                            )
                                    }"
                                }

                            val spinner =
                                findViewById<Spinner>(
                                    R.id.spinnerEditSlot
                                )

                            spinner.adapter =
                                ArrayAdapter(
                                    this@EditReservationActivity,
                                    android.R.layout.simple_spinner_dropdown_item,
                                    labels
                                )

                            val currentIndex =
                                availableSlots
                                    .indexOfFirst {
                                        it.id ==
                                                currentSlotId
                                    }

                            if (
                                currentIndex >= 0
                            ) {
                                spinner.setSelection(
                                    currentIndex
                                )
                            }

                        } else {
                            error.text =
                                "Unable to load booking slots."
                        }
                    }

                    override fun onFailure(
                        call:
                        Call<List<EnergyBookingSlot>>,
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

    private fun saveReservation(
        reservationId: String,
        editEnergy: EditText,
        transferSpinner: Spinner
    ) {
        val slotSpinner =
            findViewById<Spinner>(
                R.id.spinnerEditSlot
            )

        val error =
            findViewById<TextView>(
                R.id.textEditReservationError
            )

        if (
            availableSlots.isEmpty() ||
            slotSpinner.selectedItemPosition < 0
        ) {
            error.text =
                "No booking slot is selected."

            return
        }

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

            return
        }

        val selectedSlot =
            availableSlots[
                slotSpinner
                    .selectedItemPosition
            ]

        val session =
            DatabaseHelper(this)
                .getSession()
                ?: return

        val request =
            UpdateReservationRequest(
                slotId =
                    selectedSlot.id,

                energyAmountKwh =
                    energy,

                transferType =
                    transferSpinner
                        .selectedItem
                        .toString()
            )

        val progress =
            findViewById<ProgressBar>(
                R.id.progressEditReservation
            )

        progress.visibility =
            View.VISIBLE

        RetrofitClient
            .apiService
            .updateReservation(
                "Bearer ${session.token}",
                reservationId,
                request
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
                                this@EditReservationActivity
                            )
                                .setTitle(
                                    "Reservation Updated"
                                )
                                .setMessage(
                                    "Your updated reservation is pending approval."
                                )
                                .setPositiveButton(
                                    "OK"
                                ) { _, _ ->
                                    finish()
                                }
                                .show()

                        } else {
                            error.text =
                                ApiErrorUtils.getMessage(
                                    response,
                                    "Unable to modify reservation."
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