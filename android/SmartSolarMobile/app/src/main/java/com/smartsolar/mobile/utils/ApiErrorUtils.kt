package com.smartsolar.mobile.utils

import com.google.gson.Gson
import com.smartsolar.mobile.models.MessageResponse
import retrofit2.Response

object ApiErrorUtils {

    fun getMessage(
        response: Response<*>,
        fallback: String
    ): String {

        return try {

            val errorBody =
                response
                    .errorBody()
                    ?.string()

            if (
                errorBody.isNullOrBlank()
            ) {
                fallback
            } else {

                val error =
                    Gson()
                        .fromJson(
                            errorBody,
                            MessageResponse::class.java
                        )

                error.message
                    .ifBlank {
                        fallback
                    }
            }

        } catch (_: Exception) {
            fallback
        }
    }
}