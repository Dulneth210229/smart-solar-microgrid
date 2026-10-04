package com.smartsolar.mobile.activities

import android.app.AlertDialog
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.EditText
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.smartsolar.mobile.R
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.models.RegisterProsumerRequest
import com.smartsolar.mobile.models.RegisterProsumerResponse
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class RegisterActivity :
    AppCompatActivity() {

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_register
        )

        val editNic =
            findViewById<EditText>(
                R.id.editRegisterNic
            )

        val editName =
            findViewById<EditText>(
                R.id.editRegisterName
            )

        val editEmail =
            findViewById<EditText>(
                R.id.editRegisterEmail
            )

        val editPhone =
            findViewById<EditText>(
                R.id.editRegisterPhone
            )

        val editPassword =
            findViewById<EditText>(
                R.id.editRegisterPassword
            )

        val editConfirmPassword =
            findViewById<EditText>(
                R.id.editRegisterConfirmPassword
            )

        val buttonRegister =
            findViewById<Button>(
                R.id.buttonRegister
            )

        val progress =
            findViewById<ProgressBar>(
                R.id.progressRegister
            )

        val textError =
            findViewById<TextView>(
                R.id.textRegisterError
            )

        buttonRegister.setOnClickListener {

            val nic =
                editNic.text.toString().trim()

            val name =
                editName.text.toString().trim()

            val email =
                editEmail.text.toString().trim()

            val phone =
                editPhone.text.toString().trim()

            val password =
                editPassword.text.toString()

            val confirmPassword =
                editConfirmPassword.text.toString()

            textError.text = ""

            if (
                nic.isEmpty() ||
                name.isEmpty() ||
                email.isEmpty() ||
                phone.isEmpty() ||
                password.isEmpty()
            ) {
                textError.text =
                    "Please complete all fields."

                return@setOnClickListener
            }

            if (password.length < 8) {
                textError.text =
                    "Password must contain at least 8 characters."

                return@setOnClickListener
            }

            if (password != confirmPassword) {
                textError.text =
                    "Passwords do not match."

                return@setOnClickListener
            }

            progress.visibility =
                View.VISIBLE

            buttonRegister.isEnabled =
                false

            val request =
                RegisterProsumerRequest(
                    nic = nic,
                    fullName = name,
                    email = email,
                    phoneNumber = phone,
                    password = password
                )

            RetrofitClient
                .apiService
                .registerProsumer(request)
                .enqueue(
                    object :
                        Callback<RegisterProsumerResponse> {

                        override fun onResponse(
                            call:
                            Call<RegisterProsumerResponse>,
                            response:
                            Response<RegisterProsumerResponse>
                        ) {
                            progress.visibility =
                                View.GONE

                            buttonRegister.isEnabled =
                                true

                            if (
                                response.isSuccessful &&
                                response.body() != null
                            ) {
                                AlertDialog.Builder(
                                    this@RegisterActivity
                                )
                                    .setTitle(
                                        "Registration Successful"
                                    )
                                    .setMessage(
                                        "Your account has been created and is waiting for Backoffice activation."
                                    )
                                    .setCancelable(false)
                                    .setPositiveButton(
                                        "Back to Login"
                                    ) { _, _ ->
                                        finish()
                                    }
                                    .show()
                            } else {
                                textError.text =
                                    when (
                                        response.code()
                                    ) {
                                        409 ->
                                            "NIC or email already exists."

                                        400 ->
                                            "Invalid registration information."

                                        else ->
                                            "Registration failed."
                                    }
                            }
                        }

                        override fun onFailure(
                            call:
                            Call<RegisterProsumerResponse>,
                            throwable:
                            Throwable
                        ) {
                            progress.visibility =
                                View.GONE

                            buttonRegister.isEnabled =
                                true

                            textError.text =
                                "Unable to connect to the server: ${throwable.message}"
                        }
                    }
                )
        }

        findViewById<Button>(
            R.id.buttonBackToLogin
        ).setOnClickListener {

            finish()
        }
    }
}