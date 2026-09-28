package com.smartsolar.mobile.activities

import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.smartsolar.mobile.R
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.ProsumerDashboardResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ProsumerDashboardActivity :
    AppCompatActivity() {

    private lateinit var databaseHelper:
            DatabaseHelper

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_prosumer_dashboard
        )

        databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (
            session == null ||
            session.role != "PROSUMER"
        ) {
            returnToLogin()
            return
        }

        findViewById<TextView>(
            R.id.textWelcome
        ).text =
            "Welcome, ${session.fullName}"

        findViewById<Button>(
            R.id.buttonProfile
        ).setOnClickListener {

            startActivity(
                Intent(
                    this,
                    ProfileActivity::class.java
                )
            )
        }

        findViewById<Button>(
            R.id.buttonLogout
        ).setOnClickListener {

            returnToLogin()
        }

        loadDashboard(
            session.token
        )
    }

    override fun onResume() {
        super.onResume()

        val session =
            databaseHelper.getSession()

        if (session != null) {
            findViewById<TextView>(
                R.id.textWelcome
            ).text =
                "Welcome, ${session.fullName}"

            loadDashboard(
                session.token
            )
        }
    }

    private fun loadDashboard(
        token: String
    ) {
        val progress =
            findViewById<ProgressBar>(
                R.id.progressDashboard
            )

        val errorText =
            findViewById<TextView>(
                R.id.textDashboardError
            )

        progress.visibility =
            View.VISIBLE

        errorText.text = ""

        RetrofitClient
            .apiService
            .getProsumerDashboard(
                "Bearer $token"
            )
            .enqueue(
                object :
                    Callback<ProsumerDashboardResponse> {

                    override fun onResponse(
                        call:
                        Call<ProsumerDashboardResponse>,
                        response:
                        Response<ProsumerDashboardResponse>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (
                            response.isSuccessful &&
                            response.body() != null
                        ) {
                            val dashboard =
                                response.body()!!

                            findViewById<TextView>(
                                R.id.textCurrentCount
                            ).text =
                                dashboard
                                    .currentReservations
                                    .toString()

                            findViewById<TextView>(
                                R.id.textPendingCount
                            ).text =
                                dashboard
                                    .pendingReservations
                                    .toString()

                            findViewById<TextView>(
                                R.id.textApprovedCount
                            ).text =
                                dashboard
                                    .approvedFutureReservations
                                    .toString()

                        } else if (
                            response.code() == 401
                        ) {
                            returnToLogin()
                        } else {
                            errorText.text =
                                "Unable to load dashboard."
                        }
                    }

                    override fun onFailure(
                        call:
                        Call<ProsumerDashboardResponse>,
                        throwable:
                        Throwable
                    ) {
                        progress.visibility =
                            View.GONE

                        errorText.text =
                            "Server connection failed."
                    }
                }
            )
    }

    private fun returnToLogin() {

        databaseHelper.clearSession()

        val intent =
            Intent(
                this,
                LoginActivity::class.java
            )

        intent.flags =
            Intent.FLAG_ACTIVITY_NEW_TASK or
                    Intent.FLAG_ACTIVITY_CLEAR_TASK

        startActivity(intent)

        finish()
    }
}