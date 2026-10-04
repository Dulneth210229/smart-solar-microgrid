package com.smartsolar.mobile.adapters

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.smartsolar.mobile.R
import com.smartsolar.mobile.models.EnergyBookingSlot
import com.smartsolar.mobile.utils.DateTimeUtils

class BookingSlotAdapter(
    private val slots:
    List<EnergyBookingSlot>,

    private val onBook:
        (EnergyBookingSlot) -> Unit

) : RecyclerView.Adapter<
        BookingSlotAdapter.SlotViewHolder>() {

    class SlotViewHolder(
        itemView: View
    ) : RecyclerView.ViewHolder(
        itemView
    ) {

        val start:
                TextView =
            itemView.findViewById(
                R.id.textSlotStart
            )

        val end:
                TextView =
            itemView.findViewById(
                R.id.textSlotEnd
            )

        val capacity:
                TextView =
            itemView.findViewById(
                R.id.textSlotCapacity
            )

        val book:
                Button =
            itemView.findViewById(
                R.id.buttonBookSlot
            )
    }

    override fun onCreateViewHolder(
        parent: ViewGroup,
        viewType: Int
    ): SlotViewHolder {

        val view =
            LayoutInflater
                .from(parent.context)
                .inflate(
                    R.layout.item_booking_slot,
                    parent,
                    false
                )

        return SlotViewHolder(view)
    }

    override fun onBindViewHolder(
        holder: SlotViewHolder,
        position: Int
    ) {
        val slot =
            slots[position]

        holder.start.text =
            "Start: ${
                DateTimeUtils.formatUtcDateTime(
                    slot.startTimeUtc
                )
            }"

        holder.end.text =
            "End: ${
                DateTimeUtils.formatUtcDateTime(
                    slot.endTimeUtc
                )
            }"

        holder.capacity.text =
            "Reservation capacity: ${slot.capacitySlots}"

        holder.book.setOnClickListener {
            onBook(slot)
        }
    }

    override fun getItemCount():
            Int = slots.size
}