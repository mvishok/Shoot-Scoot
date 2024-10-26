package com.mvishok.shootscoot;

import android.app.Service;
import android.content.ContentValues;
import android.content.Intent;
import android.net.Uri;
import android.os.Environment;
import android.os.IBinder;
import android.provider.MediaStore;
import android.util.Log;

import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.io.IOException;

import java.net.InetAddress;
import java.net.NetworkInterface;
import java.util.Collections;
import java.util.List;

public class FileReceiverService extends Service {

    private static final int PORT = 12345; // Port number for receiving files
    private static final String TAG = "ShootScootFileReceiver";
    private ServerSocket serverSocket;

    @Override
    public void onCreate() {
        super.onCreate();
        new Thread(new FileReceiver()).start(); // Start receiving files in a new thread
        Log.d(TAG, "FileReceiverService started. Listening on port " + PORT);
    }

    private class FileReceiver implements Runnable {
        @Override
        public void run() {
            while (true) { // Loop indefinitely
                try {
                    if (serverSocket == null || serverSocket.isClosed()) {
                        serverSocket = new ServerSocket(PORT);
                        Log.d(TAG, "Server socket created. Waiting for connections...");
                    }

                    try {
                        Log.d(TAG, "Waiting for connection...");
                        Socket socket = serverSocket.accept(); // Accept incoming connections
                        Log.d(TAG, "Connection accepted from: " + socket.getInetAddress().getHostAddress());
                        InputStream inputStream = socket.getInputStream();

                        // Read filename first
                        String fileName = readFileName(inputStream);
                        Log.d(TAG, "Receiving file: " + fileName);

                        // Save the file using the received filename
                        saveFile(inputStream, fileName);
                        socket.close(); // Close the socket after handling
                    } catch (IOException e) {
                        Log.e(TAG, "Error handling connection", e);
                    }
                } catch (IOException e) {
                    Log.e(TAG, "Error creating server socket", e);
                    try {
                        // Wait before trying to reopen the socket
                        Thread.sleep(1000); // Sleep for 1 second before retrying
                    } catch (InterruptedException ie) {
                        Log.e(TAG, "Thread interrupted while sleeping", ie);
                    }
                }
            }
        }

        private String readFileName(InputStream inputStream) throws IOException {
            StringBuilder fileNameBuilder = new StringBuilder();
            int character;

            // Read until newline character is encountered
            while ((character = inputStream.read()) != -1) {
                if (character == '\n') break; // End of filename
                fileNameBuilder.append((char) character);
            }

            return fileNameBuilder.toString();
        }

        private void saveFile(InputStream inputStream, String fileName) {
            try {
                // Create a ContentValues object for the new file
                ContentValues values = new ContentValues();
                values.put(MediaStore.Images.Media.DISPLAY_NAME, fileName);
                values.put(MediaStore.Images.Media.MIME_TYPE, "image/png"); // Change as needed
                values.put(MediaStore.Images.Media.RELATIVE_PATH, Environment.DIRECTORY_PICTURES); // Save in Pictures directory

                // Insert the new file into MediaStore
                Uri uri = getContentResolver().insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);

                if (uri != null) {
                    // Open an OutputStream to write the data
                    try (OutputStream outputStream = getContentResolver().openOutputStream(uri)) {
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        while ((bytesRead = inputStream.read(buffer)) != -1) {
                            assert outputStream != null;  // Check if output stream is not null
                            outputStream.write(buffer, 0, bytesRead);
                        }
                        Log.d(TAG, "File saved: " + uri.toString());
                    }
                } else {
                    Log.e(TAG, "Failed to create new MediaStore entry.");
                }
            } catch (Exception e) {
                Log.e(TAG, "Error saving file", e);
            }
        }
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null; // We don't provide binding
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        try {
            if (serverSocket != null && !serverSocket.isClosed()) {
                serverSocket.close();
                Log.d(TAG, "Server socket closed.");
            }
        } catch (Exception e) {
            Log.e(TAG, "Error closing server socket", e);
        }
    }

    // Method to get the LAN IPv4 address
    public String getLocalIpAddress() {
        try {
            List<NetworkInterface> interfaces = Collections.list(NetworkInterface.getNetworkInterfaces());
            for (NetworkInterface networkInterface : interfaces) {
                List<InetAddress> inetAddresses = Collections.list(networkInterface.getInetAddresses());
                for (InetAddress inetAddress : inetAddresses) {
                    if (!inetAddress.isLoopbackAddress() && inetAddress instanceof java.net.Inet4Address) {
                        return inetAddress.getHostAddress(); // Return the first valid IPv4 address found
                    }
                }
            }
        } catch (Exception e) {
            Log.e(TAG, "Error getting local IP address", e);
        }
        return null; // Return null if no IP address is found
    }
}