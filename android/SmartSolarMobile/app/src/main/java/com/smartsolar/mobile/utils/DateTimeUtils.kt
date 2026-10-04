package com.smartsolar.mobile.utils

import java.text.SimpleDateFormat
import java.util.Locale
import java.util.TimeZone

object DateTimeUtils {

    fun formatUtcDateTime(
        value: String
    ): String {

        val patterns =
            listOf(
                "yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'",
                "yyyy-MM-dd'T'HH:mm:ss.SSSSSS'Z'",
                "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'",
                "yyyy-MM-dd'T'HH:mm:ss'Z'"
            )

        for (pattern in patterns) {

            try {
                val parser =
                    SimpleDateFormat(
                        pattern,
                        Locale.US
                    )

                parser.timeZone =
                    TimeZone.getTimeZone("UTC")

                val date =
                    parser.parse(value)

                if (date != null) {

                    val output =
                        SimpleDateFormat(
                            "dd MMM yyyy, hh:mm a",
                            Locale.getDefault()
                        )

                    output.timeZone =
                        TimeZone.getDefault()

                    return output.format(date)
                }

            } catch (_: Exception) {
            }
        }

        return value
    }
}