package com.smartsolar.mobile.activities

import android.os.Bundle
import android.widget.Button
import android.widget.ImageView
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.google.zxing.BarcodeFormat
import com.journeyapps.barcodescanner.BarcodeEncoder
import com.smartsolar.mobile.R
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.utils.DateTimeUtils

class ReservationQrActivity :
    AppCompatActivity() {

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_reservation_qr
        )

        val session =
            DatabaseHelper(this)
                .getSession()

        if (
            session == null ||
            session.role != "PROSUMER"
        ) {
            finish()
            return
        }

        val qrToken =
            intent.getStringExtra(
                "qrToken"
            )

        if (
            qrToken.isNullOrBlank()
        ) {
            finish()
            return
        }

        val stationName =
            intent.getStringExtra(
                "stationName"
            ) ?: "Solar Station"

        val startTime =
            intent.getStringExtra(
                "startTime"
            ) ?: ""

        val energyAmount =
            intent.getDoubleExtra(
                "energyAmount",
                0.0
            )

        val transferType =
            intent.getStringExtra(
                "transferType"
            ) ?: "-"

        findViewById<TextView>(
            R.id.textQrStation
        ).text =
            stationName

        findViewById<TextView>(
            R.id.textQrTime
        ).text =
            DateTimeUtils
                .formatUtcDateTime(
                    startTime
                )

        findViewById<TextView>(
            R.id.textQrEnergy
        ).text =
            "Energy: $energyAmount kWh"

        findViewById<TextView>(
            R.id.textQrType
        ).text =
            "Type: $transferType"

        try {

            val barcodeEncoder =
                BarcodeEncoder()

            val bitmap =
                barcodeEncoder
                    .encodeBitmap(
                        qrToken,
                        BarcodeFormat.QR_CODE,
                        700,
                        700
                    )

            findViewById<ImageView>(
                R.id.imageReservationQr
            ).setImageBitmap(
                bitmap
            )

        } catch (
            exception: Exception
        ) {
            exception.printStackTrace()
        }

        findViewById<Button>(
            R.id.buttonQrBack
        ).setOnClickListener {
            finish()
        }
    }
}