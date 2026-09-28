package com.smartsolar.mobile.activities

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.smartsolar.mobile.R
import com.smartsolar.mobile.database.DatabaseHelper

class ProsumerDashboardActivity :
    AppCompatActivity() {

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(
            savedInstanceState
        )

        setContentView(
            R.layout.activity_prosumer_dashboard
        )

        val databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (
            session == null ||
            session.role != "PROSUMER"
        ) {
            returnToLogin(
                databaseHelper
            )

            return
        }

        val textWelcome =
            findViewById<TextView>(
                R.id.textWelcome
            )

        textWelcome.text =
            "Welcome, ${session.fullName}"

        findViewById<Button>(
            R.id.buttonLogout
        ).setOnClickListener {

            returnToLogin(
                databaseHelper
            )
        }
    }

    private fun returnToLogin(
        databaseHelper:
        DatabaseHelper
    ) {
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