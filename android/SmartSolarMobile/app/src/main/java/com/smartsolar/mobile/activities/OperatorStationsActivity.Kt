package com.smartsolar.mobile.activities

import android.app.AlertDialog
import android.os.Bundle
import android.text.InputType
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.adapters.OperatorStationAdapter
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.SolarStation
import com.smartsolar.mobile.models.UpdateStationAvailabilityRequest
import com.smartsolar.mobile.utils.ApiErrorUtils
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class OperatorStationsActivity :
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
            R.layout.activity_operator_stations
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
                R.id.recyclerOperatorStations
            )

        recycler.layoutManager =
            LinearLayoutManager(this)

        progress =
            findViewById(
                R.id.progressOperatorStations
            )

        error =
            findViewById(
                R.id.textOperatorStationError
            )

        findViewById<Button>(
            R.id.buttonRefreshOperatorStations
        ).setOnClickListener {
            loadStations()
        }

        findViewById<Button>(
            R.id.buttonOperatorStationsBack
        ).setOnClickListener {
            finish()
        }

        loadStations()
    }


    private fun loadStations() {

        val session =
            databaseHelper.getSession()
                ?: return

        progress.visibility =
            View.VISIBLE

        error.text = ""

        RetrofitClient
            .apiService
            .getActiveStations(
                "Bearer ${session.token}"
            )
            .enqueue(
                object :
                    Callback<List<SolarStation>> {

                    override fun onResponse(
                        call:
                        Call<List<SolarStation>>,
                        response:
                        Response<List<SolarStation>>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (
                            response.isSuccessful &&
                            response.body() != null
                        ) {
                            val stations =
                                response.body()!!

                            recycler.adapter =
                                OperatorStationAdapter(
                                    stations
                                ) { station ->

                                    showAvailabilityDialog(
                                        station
                                    )
                                }

                            if (stations.isEmpty()) {
                                error.text =
                                    "No active stations found."
                            }

                        } else {
                            error.text =
                                "Unable to load stations."
                        }
                    }


                    override fun onFailure(
                        call:
                        Call<List<SolarStation>>,
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


    private fun showAvailabilityDialog(
        station: SolarStation
    ) {
        val input =
            EditText(this)

        input.inputType =
            InputType.TYPE_CLASS_NUMBER

        input.setText(
            station
                .availableBatterySlots
                .toString()
        )

        AlertDialog.Builder(this)
            .setTitle(
                station.name
            )
            .setMessage(
                "Enter available battery slots " +
                        "(0 - ${station.totalBatterySlots})"
            )
            .setView(input)
            .setNegativeButton(
                "Cancel",
                null
            )
            .setPositiveButton(
                "Update"
            ) { _, _ ->

                val value =
                    input.text
                        .toString()
                        .toIntOrNull()

                if (value == null) {
                    error.text =
                        "Enter a valid number."

                    return@setPositiveButton
                }

                updateAvailability(
                    station,
                    value
                )
            }
            .show()
    }


    private fun updateAvailability(
        station: SolarStation,
        value: Int
    ) {
        if (
            value < 0 ||
            value >
            station.totalBatterySlots
        ) {
            error.text =
                "Availability must be between 0 and ${station.totalBatterySlots}."

            return
        }

        val session =
            databaseHelper.getSession()
                ?: return

        progress.visibility =
            View.VISIBLE

        error.text = ""

        RetrofitClient
            .apiService
            .updateStationAvailability(
                "Bearer ${session.token}",
                station.id,
                UpdateStationAvailabilityRequest(
                    availableBatterySlots =
                        value
                )
            )
            .enqueue(
                object :
                    Callback<SolarStation> {

                    override fun onResponse(
                        call:
                        Call<SolarStation>,
                        response:
                        Response<SolarStation>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (response.isSuccessful) {

                            AlertDialog.Builder(
                                this@OperatorStationsActivity
                            )
                                .setMessage(
                                    "Station availability updated successfully."
                                )
                                .setPositiveButton(
                                    "OK"
                                ) { _, _ ->
                                    loadStations()
                                }
                                .show()

                        } else {
                            error.text =
                                ApiErrorUtils.getMessage(
                                    response,
                                    "Unable to update station availability."
                                )
                        }
                    }


                    override fun onFailure(
                        call:
                        Call<SolarStation>,
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