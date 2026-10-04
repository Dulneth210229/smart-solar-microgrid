package com.smartsolar.mobile.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.models.ReservationResponse
import com.smartsolar.mobile.utils.DateTimeUtils

class ReservationAdapter(
    private val reservations:
    List<ReservationResponse>,

    private val onEdit:
        (ReservationResponse) -> Unit,

    private val onCancel:
        (ReservationResponse) -> Unit,

    private val onShowQr:
        (ReservationResponse) -> Unit

) : RecyclerView.Adapter<
        ReservationAdapter.ReservationViewHolder>() {

    class ReservationViewHolder(itemView: View) : RecyclerView.ViewHolder(
        itemView
    ) {

        val station:
                TextView =
            itemView.findViewById(
                R.id.textReservationStation
            )

        val time:
                TextView =
            itemView.findViewById(
                R.id.textReservationTime
            )

        val energy:
                TextView =
            itemView.findViewById(
                R.id.textReservationEnergy
            )

        val type:
                TextView =
            itemView.findViewById(
                R.id.textReservationType
            )

        val status:
                TextView =
            itemView.findViewById(
                R.id.textReservationStatus
            )

        val actions:
                LinearLayout =
            itemView.findViewById(
                R.id.layoutReservationActions
            )

        val edit:
                Button =
            itemView.findViewById(
                R.id.buttonEditReservation
            )

        val cancel:
                Button =
            itemView.findViewById(
                R.id.buttonCancelReservation
            )
        val showQr:
                Button =
            itemView.findViewById(
                R.id.buttonShowQr
            )
    }

    override fun onCreateViewHolder(
        parent: ViewGroup,
        viewType: Int
    ): ReservationViewHolder {

        val view =
            LayoutInflater
                .from(parent.context)
                .inflate(
                    R.layout.item_reservation,
                    parent,
                    false
                )

        return ReservationViewHolder(
            view
        )
    }

    override fun onBindViewHolder(
        holder: ReservationViewHolder,
        position: Int
    ) {
        val reservation =
            reservations[position]

        holder.station.text =
            reservation.stationName

        holder.time.text =
            DateTimeUtils
                .formatUtcDateTime(
                    reservation.startTimeUtc
                )

        holder.energy.text =
            "Energy: ${reservation.energyAmountKwh} kWh"

        holder.type.text =
            "Type: ${reservation.transferType}"

        holder.status.text =
            "Status: ${reservation.status}"

        val editable =
            reservation.status ==
                    "PENDING" ||
                    reservation.status ==
                    "APPROVED"

        val canShowQr =
            reservation.status ==
                    "APPROVED" &&
                    !reservation.qrToken
                        .isNullOrBlank()

        holder.showQr.visibility =
            if (canShowQr) {
                View.VISIBLE
            } else {
                View.GONE
            }

        holder.showQr.setOnClickListener {

            if (canShowQr) {
                onShowQr(
                    reservation
                )
            }
        }

        holder.actions.visibility =
            if (editable) {
                View.VISIBLE
            } else {
                View.GONE
            }

        holder.edit.setOnClickListener {
            onEdit(reservation)
        }

        holder.cancel.setOnClickListener {
            onCancel(reservation)
        }
    }

    override fun getItemCount():
            Int = reservations.size
}