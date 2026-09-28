package com.smartsolar.mobile.activities

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
import com.smartsolar.mobile.models.AuthResponse
import com.smartsolar.mobile.models.LoginRequest
import com.smartsolar.mobile.models.UserSession
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class LoginActivity :
    AppCompatActivity() {

    private lateinit var databaseHelper:
            DatabaseHelper

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(
            savedInstanceState
        )

        setContentView(
            R.layout.activity_login
        )

        databaseHelper =
            DatabaseHelper(this)

        val existingSession =
            databaseHelper.getSession()

        if (existingSession != null) {
            navigateByRole(
                existingSession.role
            )

            return
        }

        val editIdentifier =
            findViewById<EditText>(
                R.id.editIdentifier
            )

        val editPassword =
            findViewById<EditText>(
                R.id.editPassword
            )

        val buttonLogin =
            findViewById<Button>(
                R.id.buttonLogin
            )

        val progressLogin =
            findViewById<ProgressBar>(
                R.id.progressLogin
            )

        val textError =
            findViewById<TextView>(
                R.id.textError
            )

        buttonLogin.setOnClickListener {

            val identifier =
                editIdentifier.text
                    .toString()
                    .trim()

            val password =
                editPassword.text
                    .toString()

            if (
                identifier.isEmpty() ||
                password.isEmpty()
            ) {
                textError.text =
                    "Please enter your NIC/email and password."

                return@setOnClickListener
            }

            textError.text = ""

            progressLogin.visibility =
                View.VISIBLE

            buttonLogin.isEnabled =
                false

            val request =
                LoginRequest(
                    identifier,
                    password
                )

            RetrofitClient
                .apiService
                .login(request)
                .enqueue(
                    object :
                        Callback<AuthResponse> {

                        override fun onResponse(
                            call:
                            Call<AuthResponse>,
                            response:
                            Response<AuthResponse>
                        ) {
                            progressLogin.visibility =
                                View.GONE

                            buttonLogin.isEnabled =
                                true

                            if (
                                response.isSuccessful &&
                                response.body() != null
                            ) {
                                val auth =
                                    response.body()!!

                                if (
                                    auth.role !=
                                    "PROSUMER" &&
                                    auth.role !=
                                    "GRID_OPERATOR"
                                ) {
                                    textError.text =
                                        "Only Prosumer and Grid Operator accounts can use the mobile application."

                                    return
                                }

                                val session =
                                    UserSession(
                                        userId =
                                            auth.userId,

                                        nic =
                                            auth.nic,

                                        fullName =
                                            auth.fullName,

                                        email =
                                            auth.email,

                                        role =
                                            auth.role,

                                        token =
                                            auth.token
                                    )

                                databaseHelper
                                    .saveSession(
                                        session
                                    )

                                navigateByRole(
                                    auth.role
                                )
                            } else {
                                textError.text =
                                    if (
                                        response.code() ==
                                        401
                                    ) {
                                        "Invalid credentials or account is not active."
                                    } else {
                                        "Login failed. Please try again."
                                    }
                            }
                        }

                        override fun onFailure(
                            call:
                            Call<AuthResponse>,
                            throwable:
                            Throwable
                        ) {
                            progressLogin.visibility =
                                View.GONE

                            buttonLogin.isEnabled =
                                true

                            textError.text =
                                "Unable to connect to the server: ${throwable.message}"
                        }
                    }
                )
        }
    }

    private fun navigateByRole(
        role: String
    ) {
        val intent =
            when (role) {

                "PROSUMER" ->
                    Intent(
                        this,
                        ProsumerDashboardActivity::class.java
                    )

                "GRID_OPERATOR" ->
                    Intent(
                        this,
                        OperatorDashboardActivity::class.java
                    )

                else ->
                    null
            }

        if (intent != null) {
            startActivity(intent)
            finish()
        }
    }
}