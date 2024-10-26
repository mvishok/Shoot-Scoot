package com.mvishok.shootscoot;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.widget.Button;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

public class MainActivity extends AppCompatActivity {

    private static final int REQUEST_CODE = 100; // Request code for permissions
    private TextView connectionDetailsTextView; // TextView to display connection details

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        connectionDetailsTextView = findViewById(R.id.connectionDetailsTextView); // Initialize TextView
        // Button to start the service


        // Check and request permissions
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_MEDIA_IMAGES) != PackageManager.PERMISSION_GRANTED ||
                ContextCompat.checkSelfPermission(this, Manifest.permission.READ_MEDIA_VIDEO) != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(this,
                    new String[]{Manifest.permission.READ_MEDIA_IMAGES, Manifest.permission.READ_MEDIA_VIDEO},
                    REQUEST_CODE);
        } else {
            // Permissions are already granted, start the service and show connection details
            startFileReceiverService();
            showConnectionDetails();
        }

    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == REQUEST_CODE) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                // Permission granted, start the service and show connection details
                startFileReceiverService();
                showConnectionDetails();
            } else {
                // Permission denied, handle accordingly
                // You might want to show a message to the user
            }
        }
    }

    private void startFileReceiverService() {
        // Start the FileReceiverService
        Intent serviceIntent = new Intent(this, FileReceiverService.class);
        startService(serviceIntent);
    }

    private void showConnectionDetails() {
        FileReceiverService fileReceiverService = new FileReceiverService();
        String ipAddress = fileReceiverService.getLocalIpAddress(); // Get local IP address

        String connectionDetails = "IP Address: " + ipAddress + "\nPort: 12345"; // Set port number
        connectionDetailsTextView.setText(connectionDetails); // Display in TextView
    }
}