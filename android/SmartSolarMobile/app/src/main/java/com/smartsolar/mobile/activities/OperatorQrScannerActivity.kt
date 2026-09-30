package com.smartsolar.mobile.activities

import android.app.AlertDialog
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.journeyapps.barcodescanner.ScanContract
import com.journeyapps.barcodescanner.ScanOptions
import com.smartsolar.mobile.R
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.QrTokenRequest
import com.smartsolar.mobile.models.QrVerificationResponse
import com.smartsolar.mobile.models.ReservationActionResponse
import com.smartsolar.mobile.utils.ApiErrorUtils
import com.smartsolar.mobile.utils.DateTimeUtils
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response

class OperatorQrScannerActivity :
    AppCompatActivity() {

    private lateinit var databaseHelper:
            DatabaseHelper

    private var scannedToken:
            String? = null

    private val barcodeLauncher =
        registerForActivityResult(
            ScanContract()
        ) { result ->

            if (
                result.contents != null
            ) {
                scannedToken =
                    result.contents.trim()

                verifyToken(
                    scannedToken!!
                )
            }
        }


    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_operator_qr_scanner
        )

        databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (
            session == null ||
            session.role !=
            "GRID_OPERATOR"
        ) {
            finish()
            return
        }

        findViewById<Button>(
            R.id.buttonStartQrScan
        ).setOnClickListener {

            startScanner()
        }

        findViewById<Button>(
            R.id.buttonCompleteTransfer
        ).setOnClickListener {

            val token =
                scannedToken

            if (
                !token.isNullOrBlank()
            ) {
                confirmComplete(
                    token
                )
            }
        }

        findViewById<Button>(
            R.id.buttonQrScannerBack
        ).setOnClickListener {
            finish()
        }
    }


    private fun startScanner() {

        val options =
            ScanOptions()

        options.setPrompt(
            "Scan the Prosumer transaction QR"
        )

        options.setBeepEnabled(
            true
        )

        options.setOrientationLocked(
            false
        )

        options.setDesiredBarcodeFormats(
            ScanOptions.QR_CODE
        )

        barcodeLauncher.launch(
            options
        )
    }


    private fun verifyToken(
        token: String
    ) {
        val session =
            databaseHelper
                .getSession()
                ?: return

        val progress =
            findViewById<ProgressBar>(
                R.id.progressQrVerify
            )

        val error =
            findViewById<TextView>(
                R.id.textQrVerifyError
            )

        val verifiedLayout =
            findViewById<LinearLayout>(
                R.id.layoutVerifiedReservation
            )

        progress.visibility =
            View.VISIBLE

        error.text = ""

        verifiedLayout.visibility =
            View.GONE

        val request =
            QrTokenRequest(
                qrToken = token
            )

        RetrofitClient
            .apiService
            .verifyQr(
                "Bearer ${session.token}",
                request
            )
            .enqueue(
                object :
                    Callback<QrVerificationResponse> {

                    override fun onResponse(
                        call:
                        Call<QrVerificationResponse>,
                        response:
                        Response<QrVerificationResponse>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (
                            response.isSuccessful &&
                            response.body() != null
                        ) {
                            showVerifiedReservation(
                                response.body()!!
                            )

                        } else {

                            scannedToken =
                                null

                            error.text =
                                ApiErrorUtils
                                    .getMessage(
                                        response,
                                        "QR verification failed."
                                    )
                        }
                    }

                    override fun onFailure(
                        call:
                        Call<QrVerificationResponse>,
                        throwable:
                        Throwable
                    ) {
                        progress.visibility =
                            View.GONE

                        scannedToken =
                            null

                        error.text =
                            "Unable to connect to the server."
                    }
                }
            )
    }


    private fun showVerifiedReservation(
        verification:
        QrVerificationResponse
    ) {
        findViewById<LinearLayout>(
            R.id.layoutVerifiedReservation
        ).visibility =
            View.VISIBLE

        findViewById<TextView>(
            R.id.textVerifiedProsumer
        ).text =
            "Prosumer NIC: ${verification.prosumerNic}"

        findViewById<TextView>(
            R.id.textVerifiedStation
        ).text =
            "Station: ${verification.stationName}"

        findViewById<TextView>(
            R.id.textVerifiedTime
        ).text =
            "Booking: ${
                DateTimeUtils
                    .formatUtcDateTime(
                        verification
                            .startTimeUtc
                    )
            }"

        findViewById<TextView>(
            R.id.textVerifiedEnergy
        ).text =
            "Energy: ${verification.energyAmountKwh} kWh"

        findViewById<TextView>(
            R.id.textVerifiedType
        ).text =
            "Type: ${verification.transferType}"

        findViewById<TextView>(
            R.id.textVerifiedStatus
        ).text =
            "Status: ${verification.status}"
    }


    private fun confirmComplete(
        token: String
    ) {
        AlertDialog.Builder(this)
            .setTitle(
                "Complete Transfer"
            )
            .setMessage(
                "Confirm that the energy transfer has been completed."
            )
            .setNegativeButton(
                "Cancel",
                null
            )
            .setPositiveButton(
                "Complete"
            ) { _, _ ->

                completeTransfer(
                    token
                )
            }
            .show()
    }


    private fun completeTransfer(
        token: String
    ) {
        val session =
            databaseHelper
                .getSession()
                ?: return

        val progress =
            findViewById<ProgressBar>(
                R.id.progressQrVerify
            )

        val error =
            findViewById<TextView>(
                R.id.textQrVerifyError
            )

        progress.visibility =
            View.VISIBLE

        error.text = ""

        RetrofitClient
            .apiService
            .completeTransaction(
                "Bearer ${session.token}",
                QrTokenRequest(
                    token
                )
            )
            .enqueue(
                object :
                    Callback<ReservationActionResponse> {

                    override fun onResponse(
                        call:
                        Call<ReservationActionResponse>,
                        response:
                        Response<ReservationActionResponse>
                    ) {
                        progress.visibility =
                            View.GONE

                        if (
                            response.isSuccessful &&
                            response.body() != null
                        ) {
                            scannedToken =
                                null

                            AlertDialog.Builder(
                                this@OperatorQrScannerActivity
                            )
                                .setTitle(
                                    "Transfer Completed"
                                )
                                .setMessage(
                                    "The energy transfer has been completed successfully."
                                )
                                .setCancelable(false)
                                .setPositiveButton(
                                    "OK"
                                ) { _, _ ->

                                    findViewById<
                                            LinearLayout
                                            >(
                                        R.id
                                            .layoutVerifiedReservation
                                    ).visibility =
                                        View.GONE
                                }
                                .show()

                        } else {
                            error.text =
                                ApiErrorUtils
                                    .getMessage(
                                        response,
                                        "Unable to complete the transaction."
                                    )
                        }
                    }


                    override fun onFailure(
                        call:
                        Call<ReservationActionResponse>,
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