package com.smartsolar.mobile.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.models.ReservationResponse
import com.smartsolar.mobile.utils.DateTimeUtils

class OperatorReservationAdapter(
    private val reservations:
    List<ReservationResponse>,

    private val onApprove:
        (ReservationResponse) -> Unit,

    private val onCancel:
        (ReservationResponse) -> Unit

) : RecyclerView.Adapter<
        OperatorReservationAdapter.ReservationViewHolder>() {

    class ReservationViewHolder(
        itemView: View
    ) : RecyclerView.ViewHolder(itemView) {

        val station: TextView =
            itemView.findViewById(
                R.id.textOperatorReservationStation
            )

        val prosumer: TextView =
            itemView.findViewById(
                R.id.textOperatorReservationProsumer
            )

        val time: TextView =
            itemView.findViewById(
                R.id.textOperatorReservationTime
            )

        val energy: TextView =
            itemView.findViewById(
                R.id.textOperatorReservationEnergy
            )

        val type: TextView =
            itemView.findViewById(
                R.id.textOperatorReservationType
            )

        val approve: Button =
            itemView.findViewById(
                R.id.buttonOperatorApprove
            )

        val cancel: Button =
            itemView.findViewById(
                R.id.buttonOperatorCancel
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
                    R.layout.item_operator_reservation,
                    parent,
                    false
                )

        return ReservationViewHolder(view)
    }

    override fun onBindViewHolder(
        holder: ReservationViewHolder,
        position: Int
    ) {
        val reservation =
            reservations[position]

        holder.station.text =
            reservation.stationName

        holder.prosumer.text =
            "Prosumer NIC: ${reservation.prosumerNic}"

        holder.time.text =
            DateTimeUtils.formatUtcDateTime(
                reservation.startTimeUtc
            )

        holder.energy.text =
            "Energy: ${reservation.energyAmountKwh} kWh"

        holder.type.text =
            "Type: ${reservation.transferType}"

        holder.approve.setOnClickListener {
            onApprove(reservation)
        }

        holder.cancel.setOnClickListener {
            onCancel(reservation)
        }
    }

    override fun getItemCount():
            Int = reservations.size
}