package com.smartsolar.mobile.activities

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.smartsolar.mobile.R
import com.smartsolar.mobile.database.DatabaseHelper

class OperatorDashboardActivity :
    AppCompatActivity() {

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(
            savedInstanceState
        )

        setContentView(
            R.layout.activity_operator_dashboard
        )

        val databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (
            session == null ||
            session.role != "GRID_OPERATOR"
        ) {
            returnToLogin(
                databaseHelper
            )

            return
        }

        findViewById<TextView>(
            R.id.textOperatorWelcome
        ).text =
            "Welcome, ${session.fullName}"

        findViewById<Button>(
            R.id.buttonOperatorLogout
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