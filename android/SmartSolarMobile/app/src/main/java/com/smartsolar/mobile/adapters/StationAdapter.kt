package com.smartsolar.mobile.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.models.SolarStation

class StationAdapter(
    private val stations:
    List<SolarStation>,

    private val onStationClick:
        (SolarStation) -> Unit

) : RecyclerView.Adapter<
        StationAdapter.StationViewHolder>() {

    class StationViewHolder(
        itemView: View
    ) : RecyclerView.ViewHolder(
        itemView
    ) {

        val name:
                TextView =
            itemView.findViewById(
                R.id.textStationName
            )

        val location:
                TextView =
            itemView.findViewById(
                R.id.textStationLocation
            )

        val capacity:
                TextView =
            itemView.findViewById(
                R.id.textStationCapacity
            )

        val availability:
                TextView =
            itemView.findViewById(
                R.id.textStationAvailability
            )

        val hours:
                TextView =
            itemView.findViewById(
                R.id.textStationHours
            )

        val button:
                Button =
            itemView.findViewById(
                R.id.buttonViewSlots
            )
    }

    override fun onCreateViewHolder(
        parent: ViewGroup,
        viewType: Int
    ): StationViewHolder {

        val view =
            LayoutInflater
                .from(parent.context)
                .inflate(
                    R.layout.item_station,
                    parent,
                    false
                )

        return StationViewHolder(view)
    }

    override fun onBindViewHolder(
        holder: StationViewHolder,
        position: Int
    ) {
        val station =
            stations[position]

        holder.name.text =
            station.name

        holder.location.text =
            station.location

        holder.capacity.text =
            "Power: ${station.capacityKw} kW | Battery: ${station.batteryCapacityKwh} kWh"

        holder.availability.text =
            "Battery slots available: ${station.availableBatterySlots}/${station.totalBatterySlots}"

        holder.hours.text =
            "Hours: ${station.openingTime} - ${station.closingTime}"

        holder.button.setOnClickListener {
            onStationClick(station)
        }
    }

    override fun getItemCount():
            Int = stations.size
}