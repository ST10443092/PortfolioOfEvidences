package com.example.prog7313p1

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat

import android.widget.EditText
import android.widget.TextView
import android.widget.Toast
import com.google.firebase.firestore.FirebaseFirestore
class Login : AppCompatActivity() {


    lateinit var etUsername: EditText
    lateinit var etPassword: EditText
    lateinit var btnLogin: Button
    lateinit var tvRegister: TextView

    private lateinit var firestore: FirebaseFirestore



    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContentView(R.layout.activity_login)

        etUsername = findViewById(R.id.etUsername)
        etPassword = findViewById(R.id.etPassword)
        btnLogin = findViewById(R.id.btnLogin)
        tvRegister = findViewById(R.id.tvRegister)

        firestore = FirebaseFirestore.getInstance()

        tvRegister.setOnClickListener {

            val intent = Intent(this, Register::class.java)
            startActivity(intent)

        }




        btnLogin.setOnClickListener {

            val username = etUsername.text.toString().trim()
            val password = etPassword.text.toString().trim()

            if (username.isEmpty() || password.isEmpty()) {
                // Input validation before authentication (OWASP Foundation, 2021).
                Toast.makeText(this, "Please enter username and password", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            firestore.collection("users")
                .whereEqualTo("username", username)
                .get()
                .addOnSuccessListener { result ->
                    val matchingDocument = result.documents
                        .firstOrNull { document ->
                            document.toObject(User::class.java)?.password == password
                        }
                    val matchingUser = matchingDocument?.toObject(User::class.java)

                    if (matchingUser != null) {
                        val userId = matchingUser.id.ifBlank { matchingDocument.id }
                        SessionManager.saveUserId(this@Login, userId)
                        Toast.makeText(this@Login, "Login successful", Toast.LENGTH_SHORT).show()
                        // Navigation after successful auth using explicit intents (Android Developers, 2026).
                        val intent = Intent(this@Login, Home::class.java)
                        startActivity(intent)
                        finish()
                    } else {
                        Toast.makeText(this@Login, "Invalid username or password", Toast.LENGTH_SHORT).show()
                    }
                }
                .addOnFailureListener {
                    Toast.makeText(this@Login, "Login failed", Toast.LENGTH_SHORT).show()
                }


        }




        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main)) { v, insets ->
            val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom)
            insets
        }
    }
}

/*
References (Harvard)
Android Developers (2026) Intents and intent filters. Available at: https://developer.android.com/guide/components/intents-filters (Accessed: 29 April 2026).
OWASP Foundation (2021) Authentication Cheat Sheet. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html (Accessed: 29 April 2026).
*/
