package com.smartsolar.mobile.activities

import android.app.AlertDialog
import android.content.Intent
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.smartsolar.mobile.R
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.MessageResponse
import com.smartsolar.mobile.models.UpdateProfileRequest
import com.smartsolar.mobile.models.UserProfileResponse
import com.smartsolar.mobile.models.UserSession
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class ProfileActivity :
    AppCompatActivity() {

    private lateinit var databaseHelper:
            DatabaseHelper

    private lateinit var editName:
            EditText

    private lateinit var editEmail:
            EditText

    private lateinit var editPhone:
            EditText

    private lateinit var textMessage:
            TextView

    private lateinit var progress:
            ProgressBar

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_profile
        )

        databaseHelper =
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

        editName =
            findViewById(
                R.id.editProfileName
            )

        editEmail =
            findViewById(
                R.id.editProfileEmail
            )

        editPhone =
            findViewById(
                R.id.editProfilePhone
            )

        textMessage =
            findViewById(
                R.id.textProfileMessage
            )

        progress =
            findViewById(
                R.id.progressProfile
            )

        loadProfile(
            session
        )

        findViewById<Button>(
            R.id.buttonUpdateProfile
        ).setOnClickListener {

            updateProfile(
                session
            )
        }

        findViewById<Button>(
            R.id.buttonRequestDeactivation
        ).setOnClickListener {

            confirmDeactivation(
                session
            )
        }

        findViewById<Button>(
            R.id.buttonProfileBack
        ).setOnClickListener {

            finish()
        }
    }

    private fun loadProfile(
        session: UserSession
    ) {
        progress.visibility =
            View.VISIBLE

        RetrofitClient
            .apiService
            .getMyProfile(
                "Bearer ${session.token}"
            )
            .enqueue(
                object :
                    Callback<UserProfileResponse> {

                    override fun onResponse(
                        call:
                        Call<UserProfileResponse>,
                        response:
                        Response<UserProfileResponse>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (
                            response.isSuccessful &&
                            response.body() != null
                        ) {
                            val profile =
                                response.body()!!

                            findViewById<TextView>(
                                R.id.textProfileNic
                            ).text =
                                profile.nic ?: "-"

                            editName.setText(
                                profile.fullName
                            )

                            editEmail.setText(
                                profile.email
                            )

                            editPhone.setText(
                                profile.phoneNumber
                            )

                            findViewById<TextView>(
                                R.id.textProfileStatus
                            ).text =
                                "Status: ${profile.status}"

                        } else {
                            textMessage.text =
                                "Unable to load profile."
                        }
                    }

                    override fun onFailure(
                        call:
                        Call<UserProfileResponse>,
                        throwable:
                        Throwable
                    ) {
                        progress.visibility =
                            View.GONE

                        textMessage.text =
                            "Server connection failed."
                    }
                }
            )
    }

    private fun updateProfile(
        session: UserSession
    ) {
        val name =
            editName.text
                .toString()
                .trim()

        val email =
            editEmail.text
                .toString()
                .trim()

        val phone =
            editPhone.text
                .toString()
                .trim()

        if (
            name.isEmpty() ||
            email.isEmpty() ||
            phone.isEmpty()
        ) {
            textMessage.text =
                "Please complete all fields."

            return
        }

        val request =
            UpdateProfileRequest(
                fullName = name,
                email = email,
                phoneNumber = phone
            )

        progress.visibility =
            View.VISIBLE

        RetrofitClient
            .apiService
            .updateMyProfile(
                "Bearer ${session.token}",
                request
            )
            .enqueue(
                object :
                    Callback<MessageResponse> {

                    override fun onResponse(
                        call:
                        Call<MessageResponse>,
                        response:
                        Response<MessageResponse>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (response.isSuccessful) {

                            val updatedSession =
                                UserSession(
                                    userId =
                                        session.userId,

                                    nic =
                                        session.nic,

                                    fullName =
                                        name,

                                    email =
                                        email,

                                    role =
                                        session.role,

                                    token =
                                        session.token
                                )

                            databaseHelper
                                .saveSession(
                                    updatedSession
                                )

                            textMessage.text =
                                "Profile updated successfully."

                        } else {
                            textMessage.text =
                                response
                                    .body()
                                    ?.message
                                    ?: "Unable to update profile."
                        }
                    }

                    override fun onFailure(
                        call:
                        Call<MessageResponse>,
                        throwable:
                        Throwable
                    ) {
                        progress.visibility =
                            View.GONE

                        textMessage.text =
                            "Server connection failed."
                    }
                }
            )
    }

    private fun confirmDeactivation(
        session: UserSession
    ) {
        AlertDialog.Builder(this)
            .setTitle(
                "Request Deactivation"
            )
            .setMessage(
                "Are you sure you want to request account deactivation?"
            )
            .setNegativeButton(
                "No",
                null
            )
            .setPositiveButton(
                "Yes"
            ) { _, _ ->

                requestDeactivation(
                    session
                )
            }
            .show()
    }

    private fun requestDeactivation(
        session: UserSession
    ) {
        progress.visibility =
            View.VISIBLE

        RetrofitClient
            .apiService
            .requestDeactivation(
                "Bearer ${session.token}"
            )
            .enqueue(
                object :
                    Callback<MessageResponse> {

                    override fun onResponse(
                        call:
                        Call<MessageResponse>,
                        response:
                        Response<MessageResponse>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (response.isSuccessful) {

                            AlertDialog.Builder(
                                this@ProfileActivity
                            )
                                .setTitle(
                                    "Request Submitted"
                                )
                                .setMessage(
                                    "Your deactivation request has been submitted to Backoffice."
                                )
                                .setCancelable(false)
                                .setPositiveButton(
                                    "Logout"
                                ) { _, _ ->

                                    databaseHelper
                                        .clearSession()

                                    val intent =
                                        Intent(
                                            this@ProfileActivity,
                                            LoginActivity::class.java
                                        )

                                    intent.flags =
                                        Intent.FLAG_ACTIVITY_NEW_TASK or
                                                Intent.FLAG_ACTIVITY_CLEAR_TASK

                                    startActivity(
                                        intent
                                    )

                                    finish()
                                }
                                .show()

                        } else {
                            textMessage.text =
                                "Unable to request deactivation."
                        }
                    }

                    override fun onFailure(
                        call:
                        Call<MessageResponse>,
                        throwable:
                        Throwable
                    ) {
                        progress.visibility =
                            View.GONE

                        textMessage.text =
                            "Server connection failed."
                    }
                }
            )
    }
}