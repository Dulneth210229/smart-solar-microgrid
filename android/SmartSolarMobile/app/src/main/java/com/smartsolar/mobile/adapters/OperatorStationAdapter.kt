package com.smartsolar.mobile.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.models.SolarStation

class OperatorStationAdapter(
    private val stations:
    List<SolarStation>,

    private val onUpdate:
        (SolarStation) -> Unit

) : RecyclerView.Adapter<
        OperatorStationAdapter.StationViewHolder>() {

    class StationViewHolder(
        itemView: View
    ) : RecyclerView.ViewHolder(itemView) {

        val name: TextView =
            itemView.findViewById(
                R.id.textOperatorStationName
            )

        val location: TextView =
            itemView.findViewById(
                R.id.textOperatorStationLocation
            )

        val availability: TextView =
            itemView.findViewById(
                R.id.textOperatorStationAvailability
            )

        val update: Button =
            itemView.findViewById(
                R.id.buttonUpdateOperatorAvailability
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
                    R.layout.item_operator_station,
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

        holder.availability.text =
            "Available battery slots: " +
                    "${station.availableBatterySlots}/" +
                    "${station.totalBatterySlots}"

        holder.update.setOnClickListener {
            onUpdate(station)
        }
    }


    override fun getItemCount():
            Int = stations.size
}