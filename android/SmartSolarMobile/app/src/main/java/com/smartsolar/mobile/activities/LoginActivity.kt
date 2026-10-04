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

class LoginActivity : AppCompatActivity() {

    private lateinit var databaseHelper: DatabaseHelper

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        setContentView(R.layout.activity_login)

        // Initialize the local SQLite database helper.
        databaseHelper = DatabaseHelper(this)

        // Check whether a logged-in session already exists.
        val existingSession = databaseHelper.getSession()

        if (existingSession != null) {
            navigateByRole(existingSession.role)
            return
        }

        // Get references to the login UI components.
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

        // This is the new registration button added in Step 12.11.
        val buttonOpenRegister =
            findViewById<Button>(
                R.id.buttonOpenRegister
            )

        val progressLogin =
            findViewById<ProgressBar>(
                R.id.progressLogin
            )

        val textError =
            findViewById<TextView>(
                R.id.textError
            )

        // Handle Login button click.
        buttonLogin.setOnClickListener {

            val identifier =
                editIdentifier.text
                    .toString()
                    .trim()

            val password =
                editPassword.text
                    .toString()

            // Validate required fields before calling the API.
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
                    identifier = identifier,
                    password = password
                )

            // Call the C# Web API login endpoint using Retrofit.
            RetrofitClient
                .apiService
                .login(request)
                .enqueue(
                    object :
                        Callback<AuthResponse> {

                        override fun onResponse(
                            call: Call<AuthResponse>,
                            response: Response<AuthResponse>
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

                                // Only Prosumers and Grid Operators
                                // are allowed to use the mobile app.
                                if (
                                    auth.role != "PROSUMER" &&
                                    auth.role != "GRID_OPERATOR"
                                ) {
                                    textError.text =
                                        "Only Prosumer and Grid Operator accounts can use the mobile application."

                                    return
                                }

                                // Save authenticated user information
                                // and JWT locally in SQLite.
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

                                // Send the authenticated user
                                // to the correct role-based dashboard.
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
                            call: Call<AuthResponse>,
                            throwable: Throwable
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

        // Open the Prosumer registration screen.
        buttonOpenRegister.setOnClickListener {

            val intent =
                Intent(
                    this,
                    RegisterActivity::class.java
                )

            startActivity(intent)
        }
    }

    // Navigates the logged-in user to the correct mobile dashboard.
    private fun navigateByRole(role: String) {

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