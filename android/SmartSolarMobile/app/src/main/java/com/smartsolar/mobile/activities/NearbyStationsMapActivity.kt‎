package com.smartsolar.mobile.activities

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.location.Location
import android.os.Bundle
import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationServices
import com.google.android.gms.maps.CameraUpdateFactory
import com.google.android.gms.maps.GoogleMap
import com.google.android.gms.maps.OnMapReadyCallback
import com.google.android.gms.maps.SupportMapFragment
import com.google.android.gms.maps.model.BitmapDescriptorFactory
import com.google.android.gms.maps.model.LatLng
import com.google.android.gms.maps.model.LatLngBounds
import com.google.android.gms.maps.model.MarkerOptions
import com.smartsolar.mobile.R
import com.smartsolar.mobile.api.RetrofitClient
import com.smartsolar.mobile.database.DatabaseHelper
import com.smartsolar.mobile.models.SolarStation
import retrofit2.Call
import retrofit2.Callback
import retrofit2.Response
import java.util.Locale

class NearbyStationsMapActivity :
    AppCompatActivity(),
    OnMapReadyCallback {

    private var googleMap:
            GoogleMap? = null

    private lateinit var fusedLocationClient:
            FusedLocationProviderClient

    private var currentLocation:
            Location? = null

    private var stations:
            List<SolarStation> =
        emptyList()

    private val locationPermissionLauncher =
        registerForActivityResult(
            ActivityResultContracts
                .RequestMultiplePermissions()
        ) { permissions ->

            val fineGranted =
                permissions[
                    Manifest.permission
                        .ACCESS_FINE_LOCATION
                ] == true

            val coarseGranted =
                permissions[
                    Manifest.permission
                        .ACCESS_COARSE_LOCATION
                ] == true

            if (
                fineGranted ||
                coarseGranted
            ) {
                enableUserLocation()
            } else {
                findViewById<TextView>(
                    R.id.textNearestStation
                ).text =
                    "Location permission denied. You can still view solar stations."

                focusOnStations()
            }
        }


    override fun onCreate(
        savedInstanceState: Bundle?
    ) {
        super.onCreate(
            savedInstanceState
        )

        setContentView(
            R.layout
                .activity_nearby_stations_map
        )

        val databaseHelper =
            DatabaseHelper(this)

        val session =
            databaseHelper.getSession()

        if (
            session == null ||
            session.role != "PROSUMER"
        ) {
            finish()
            return
        }

        fusedLocationClient =
            LocationServices
                .getFusedLocationProviderClient(
                    this
                )

        val mapFragment =
            supportFragmentManager
                .findFragmentById(
                    R.id.map
                ) as SupportMapFragment

        mapFragment.getMapAsync(
            this
        )

        findViewById<Button>(
            R.id.buttonMapBack
        ).setOnClickListener {
            finish()
        }

        loadStations(
            session.token
        )
    }


    override fun onMapReady(
        map: GoogleMap
    ) {
        googleMap = map

        map.uiSettings
            .isZoomControlsEnabled =
            true

        map.uiSettings
            .isMapToolbarEnabled =
            true

        map.setOnMarkerClickListener {
                marker ->

            val station =
                marker.tag
                        as? SolarStation

            if (station != null) {
                showStationDetails(
                    station
                )
            }

            false
        }

        renderMapContent()

        requestLocationPermission()
    }


    private fun loadStations(
        token: String
    ) {
        RetrofitClient
            .apiService
            .getActiveStations(
                "Bearer $token"
            )
            .enqueue(
                object :
                    Callback<
                            List<SolarStation>
                            > {

                    override fun onResponse(
                        call:
                        Call<
                                List<SolarStation>
                                >,
                        response:
                        Response<
                                List<SolarStation>
                                >
                    ) {
                        if (
                            response
                                .isSuccessful &&
                            response.body() != null
                        ) {
                            stations =
                                response.body()!!

                            renderMapContent()

                            if (
                                currentLocation ==
                                null
                            ) {
                                focusOnStations()
                            }
                        } else {
                            findViewById<
                                    TextView
                                    >(
                                R.id
                                    .textNearestStation
                            ).text =
                                "Unable to load solar stations."
                        }
                    }


                    override fun onFailure(
                        call:
                        Call<
                                List<SolarStation>
                                >,
                        throwable:
                        Throwable
                    ) {
                        findViewById<
                                TextView
                                >(
                            R.id
                                .textNearestStation
                        ).text =
                            "Unable to connect to the server."
                    }
                }
            )
    }


    private fun requestLocationPermission() {

        val finePermission =
            ActivityCompat
                .checkSelfPermission(
                    this,
                    Manifest.permission
                        .ACCESS_FINE_LOCATION
                )

        val coarsePermission =
            ActivityCompat
                .checkSelfPermission(
                    this,
                    Manifest.permission
                        .ACCESS_COARSE_LOCATION
                )

        if (
            finePermission ==
            PackageManager
                .PERMISSION_GRANTED ||
            coarsePermission ==
            PackageManager
                .PERMISSION_GRANTED
        ) {
            enableUserLocation()
        } else {
            locationPermissionLauncher
                .launch(
                    arrayOf(
                        Manifest.permission
                            .ACCESS_FINE_LOCATION,

                        Manifest.permission
                            .ACCESS_COARSE_LOCATION
                    )
                )
        }
    }


    private fun enableUserLocation() {

        val finePermission =
            ActivityCompat
                .checkSelfPermission(
                    this,
                    Manifest.permission
                        .ACCESS_FINE_LOCATION
                )

        val coarsePermission =
            ActivityCompat
                .checkSelfPermission(
                    this,
                    Manifest.permission
                        .ACCESS_COARSE_LOCATION
                )

        if (
            finePermission !=
            PackageManager
                .PERMISSION_GRANTED &&
            coarsePermission !=
            PackageManager
                .PERMISSION_GRANTED
        ) {
            return
        }

        try {
            googleMap
                ?.isMyLocationEnabled =
                true

            fusedLocationClient
                .lastLocation
                .addOnSuccessListener {
                        location ->

                    if (location != null) {

                        currentLocation =
                            location

                        renderMapContent()

                        val userPosition =
                            LatLng(
                                location.latitude,
                                location.longitude
                            )

                        googleMap
                            ?.animateCamera(
                                CameraUpdateFactory
                                    .newLatLngZoom(
                                        userPosition,
                                        11f
                                    )
                            )

                        updateNearestStation()

                    } else {

                        findViewById<
                                TextView
                                >(
                            R.id
                                .textNearestStation
                        ).text =
                            "Current location is unavailable. Station markers are still shown."

                        focusOnStations()
                    }
                }

        } catch (
            _: SecurityException
        ) {
            focusOnStations()
        }
    }


    private fun renderMapContent() {

        val map =
            googleMap ?: return

        map.clear()

        stations.forEach {
                station ->

            val marker =
                map.addMarker(
                    MarkerOptions()
                        .position(
                            LatLng(
                                station.latitude,
                                station.longitude
                            )
                        )
                        .title(
                            station.name
                        )
                        .snippet(
                            station.location
                        )
                )

            marker?.tag =
                station
        }

        currentLocation
            ?.let {
                    location ->

                map.addMarker(
                    MarkerOptions()
                        .position(
                            LatLng(
                                location.latitude,
                                location.longitude
                            )
                        )
                        .title(
                            "You are here"
                        )
                        .icon(
                            BitmapDescriptorFactory
                                .defaultMarker(
                                    BitmapDescriptorFactory
                                        .HUE_AZURE
                                )
                        )
                )
            }
    }


    private fun showStationDetails(
        station: SolarStation
    ) {
        val panel =
            findViewById<
                    LinearLayout
                    >(
                R.id
                    .layoutSelectedStation
            )

        panel.visibility =
            View.VISIBLE

        findViewById<TextView>(
            R.id.textMapStationName
        ).text =
            station.name

        findViewById<TextView>(
            R.id.textMapStationDetails
        ).text =
            "${station.location}\n" +
                    "Available slots: " +
                    "${station.availableBatterySlots}/" +
                    "${station.totalBatterySlots}\n" +
                    "Hours: " +
                    "${station.openingTime} - " +
                    station.closingTime

        val distanceText =
            currentLocation
                ?.let {
                        location ->

                    val distance =
                        distanceBetween(
                            location,
                            station
                        )

                    "Distance: ${
                        formatDistance(
                            distance
                        )
                    }"
                }
                ?: "Distance unavailable"

        findViewById<TextView>(
            R.id
                .textMapStationDistance
        ).text =
            distanceText

        findViewById<Button>(
            R.id.buttonMapViewSlots
        ).setOnClickListener {

            val intent =
                Intent(
                    this,
                    BookingSlotsActivity::
                    class.java
                )

            intent.putExtra(
                "stationId",
                station.id
            )

            intent.putExtra(
                "stationName",
                station.name
            )

            startActivity(
                intent
            )
        }
    }


    private fun updateNearestStation() {

        val userLocation =
            currentLocation ?: return

        if (stations.isEmpty()) {
            findViewById<TextView>(
                R.id.textNearestStation
            ).text =
                "No active solar stations are available."

            return
        }

        val nearest =
            stations.minByOrNull {
                    station ->

                distanceBetween(
                    userLocation,
                    station
                )
            }

        if (nearest != null) {

            val distance =
                distanceBetween(
                    userLocation,
                    nearest
                )

            findViewById<TextView>(
                R.id.textNearestStation
            ).text =
                "Nearest: ${nearest.name} • ${
                    formatDistance(
                        distance
                    )
                } away"
        }
    }


    private fun distanceBetween(
        userLocation: Location,
        station: SolarStation
    ): Float {

        val results =
            FloatArray(1)

        Location.distanceBetween(
            userLocation.latitude,
            userLocation.longitude,
            station.latitude,
            station.longitude,
            results
        )

        return results[0]
    }


    private fun formatDistance(
        metres: Float
    ): String {

        return if (
            metres < 1000
        ) {
            String.format(
                Locale.getDefault(),
                "%.0f m",
                metres
            )
        } else {
            String.format(
                Locale.getDefault(),
                "%.1f km",
                metres / 1000.0
            )
        }
    }


    private fun focusOnStations() {

        val map =
            googleMap ?: return

        if (stations.isEmpty()) {

            val sriLanka =
                LatLng(
                    7.8731,
                    80.7718
                )

            map.moveCamera(
                CameraUpdateFactory
                    .newLatLngZoom(
                        sriLanka,
                        7f
                    )
            )

            return
        }

        if (stations.size == 1) {

            val station =
                stations.first()

            map.moveCamera(
                CameraUpdateFactory
                    .newLatLngZoom(
                        LatLng(
                            station.latitude,
                            station.longitude
                        ),
                        12f
                    )
            )

            return
        }

        val builder =
            LatLngBounds.Builder()

        stations.forEach {
                station ->

            builder.include(
                LatLng(
                    station.latitude,
                    station.longitude
                )
            )
        }

        val bounds =
            builder.build()

        map.setOnMapLoadedCallback {

            map.animateCamera(
                CameraUpdateFactory
                    .newLatLngBounds(
                        bounds,
                        120
                    )
            )
        }
    }
}