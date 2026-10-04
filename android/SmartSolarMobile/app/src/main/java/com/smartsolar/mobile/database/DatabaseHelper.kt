package com.smartsolar.mobile.database

import android.content.ContentValues
import android.content.Context
import android.database.sqlite.SQLiteDatabase
import android.database.sqlite.SQLiteOpenHelper
import com.smartsolar.mobile.models.UserSession

class DatabaseHelper(
    context: Context
) : SQLiteOpenHelper(
    context,
    DATABASE_NAME,
    null,
    DATABASE_VERSION
) {

    companion object {
        private const val DATABASE_NAME =
            "smart_solar_local.db"

        private const val DATABASE_VERSION = 1

        private const val TABLE_SESSION =
            "user_session"

        private const val COL_ID =
            "id"

        private const val COL_USER_ID =
            "user_id"

        private const val COL_NIC =
            "nic"

        private const val COL_FULL_NAME =
            "full_name"

        private const val COL_EMAIL =
            "email"

        private const val COL_ROLE =
            "role"

        private const val COL_TOKEN =
            "token"
    }

    override fun onCreate(
        db: SQLiteDatabase
    ) {
        val createSessionTable = """
            CREATE TABLE $TABLE_SESSION (
                $COL_ID INTEGER PRIMARY KEY,
                $COL_USER_ID TEXT NOT NULL,
                $COL_NIC TEXT,
                $COL_FULL_NAME TEXT NOT NULL,
                $COL_EMAIL TEXT NOT NULL,
                $COL_ROLE TEXT NOT NULL,
                $COL_TOKEN TEXT NOT NULL
            )
        """.trimIndent()

        db.execSQL(createSessionTable)
    }

    override fun onUpgrade(
        db: SQLiteDatabase,
        oldVersion: Int,
        newVersion: Int
    ) {
        db.execSQL(
            "DROP TABLE IF EXISTS $TABLE_SESSION"
        )

        onCreate(db)
    }

    fun saveSession(
        session: UserSession
    ) {
        val db =
            writableDatabase

        db.delete(
            TABLE_SESSION,
            null,
            null
        )

        val values =
            ContentValues().apply {

                put(
                    COL_ID,
                    1
                )

                put(
                    COL_USER_ID,
                    session.userId
                )

                put(
                    COL_NIC,
                    session.nic
                )

                put(
                    COL_FULL_NAME,
                    session.fullName
                )

                put(
                    COL_EMAIL,
                    session.email
                )

                put(
                    COL_ROLE,
                    session.role
                )

                put(
                    COL_TOKEN,
                    session.token
                )
            }

        db.insert(
            TABLE_SESSION,
            null,
            values
        )

        db.close()
    }

    fun getSession():
            UserSession? {

        val db =
            readableDatabase

        val cursor =
            db.query(
                TABLE_SESSION,
                null,
                null,
                null,
                null,
                null,
                null
            )

        var session:
                UserSession? = null

        if (cursor.moveToFirst()) {

            session =
                UserSession(
                    userId =
                        cursor.getString(
                            cursor.getColumnIndexOrThrow(
                                COL_USER_ID
                            )
                        ),

                    nic =
                        cursor.getString(
                            cursor.getColumnIndexOrThrow(
                                COL_NIC
                            )
                        ),

                    fullName =
                        cursor.getString(
                            cursor.getColumnIndexOrThrow(
                                COL_FULL_NAME
                            )
                        ),

                    email =
                        cursor.getString(
                            cursor.getColumnIndexOrThrow(
                                COL_EMAIL
                            )
                        ),

                    role =
                        cursor.getString(
                            cursor.getColumnIndexOrThrow(
                                COL_ROLE
                            )
                        ),

                    token =
                        cursor.getString(
                            cursor.getColumnIndexOrThrow(
                                COL_TOKEN
                            )
                        )
                )
        }

        cursor.close()
        db.close()

        return session
    }

    fun clearSession() {

        val db =
            writableDatabase

        db.delete(
            TABLE_SESSION,
            null,
            null
        )

        db.close()
    }
}