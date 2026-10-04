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
import com.smartsolar.mobile.adapters.StationAdapter
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.SolarStation
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class StationListActivity :
    AppCompatActivity() {

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_station_list
        )

        val databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (
            session == null ||
            session.role != "PROSUMER"
        ) {
            finish()
            return
        }

        val recycler =
            findViewById<RecyclerView>(
                R.id.recyclerStations
            )

        recycler.layoutManager =
            LinearLayoutManager(this)

        findViewById<Button>(
            R.id.buttonStationBack
        ).setOnClickListener {
            finish()
        }

        val progress =
            findViewById<ProgressBar>(
                R.id.progressStations
            )

        val error =
            findViewById<TextView>(
                R.id.textStationError
            )

        progress.visibility =
            View.VISIBLE

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

                            if (stations.isEmpty()) {
                                error.text =
                                    "No active solar stations are currently available."
                            }

                            recycler.adapter =
                                StationAdapter(
                                    stations
                                ) { station ->

                                    val intent =
                                        Intent(
                                            this@StationListActivity,
                                            BookingSlotsActivity::class.java
                                        )

                                    intent.putExtra(
                                        "stationId",
                                        station.id
                                    )

                                    intent.putExtra(
                                        "stationName",
                                        station.name
                                    )

                                    startActivity(
                                        intent
                                    )
                                }

                        } else {
                            error.text =
                                "Unable to load solar stations."
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
}