package com.smartsolar.mobile.activities

import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.adapters.BookingSlotAdapter
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.EnergyBookingSlot
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class BookingSlotsActivity :
    AppCompatActivity() {

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_booking_slots
        )

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
            ) ?: "Solar Station"

        findViewById<TextView>(
            R.id.textSlotStationName
        ).text =
            stationName

        findViewById<Button>(
            R.id.buttonSlotsBack
        ).setOnClickListener {
            finish()
        }

        val databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (session == null) {
            finish()
            return
        }

        val recycler =
            findViewById<RecyclerView>(
                R.id.recyclerSlots
            )

        recycler.layoutManager =
            LinearLayoutManager(this)

        val progress =
            findViewById<ProgressBar>(
                R.id.progressSlots
            )

        val error =
            findViewById<TextView>(
                R.id.textSlotError
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
                            val slots =
                                response.body()!!

                            if (slots.isEmpty()) {
                                error.text =
                                    "No booking slots are currently available."
                            }

                            recycler.adapter =
                                BookingSlotAdapter(
                                    slots
                                ) { slot ->

                                    val bookingIntent =
                                        Intent(
                                            this@BookingSlotsActivity,
                                            CreateReservationActivity::class.java
                                        )

                                    bookingIntent.putExtra(
                                        "slotId",
                                        slot.id
                                    )

                                    bookingIntent.putExtra(
                                        "stationName",
                                        stationName
                                    )

                                    bookingIntent.putExtra(
                                        "startTime",
                                        slot.startTimeUtc
                                    )

                                    bookingIntent.putExtra(
                                        "endTime",
                                        slot.endTimeUtc
                                    )

                                    startActivity(
                                        bookingIntent
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
}