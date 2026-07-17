package com.example.prog7313p1

import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.TextView
import android.widget.Toast
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import com.google.firebase.firestore.FirebaseFirestore

class Register : AppCompatActivity() {

    lateinit var btnRegister: Button
    lateinit var etFullName: EditText
    lateinit var etEmail: EditText
    lateinit var etUsername: EditText
    lateinit var etPassword: EditText
    lateinit var etConfirmPassword: EditText
    lateinit var tvBackToLogin: TextView

    private lateinit var firestore: FirebaseFirestore

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContentView(R.layout.activity_register)

        btnRegister = findViewById(R.id.btnRegister)
        etFullName = findViewById(R.id.etFullName)
        etEmail = findViewById(R.id.etEmail)
        etUsername = findViewById(R.id.etUsername)
        etPassword = findViewById(R.id.etPassword)
        etConfirmPassword = findViewById(R.id.etConfirmPassword)
        tvBackToLogin = findViewById(R.id.tvBackToLogin)

        firestore = FirebaseFirestore.getInstance()

        btnRegister.setOnClickListener {

            val fullName = etFullName.text.toString().trim()
            val email = etEmail.text.toString().trim()
            val username = etUsername.text.toString().trim()
            val password = etPassword.text.toString().trim()
            val confirmPassword = etConfirmPassword.text.toString().trim()

            if (fullName.isEmpty() || email.isEmpty() || username.isEmpty() || password.isEmpty() || confirmPassword.isEmpty()) {
                // Basic validation for required registration fields (OWASP Foundation, 2021).
                Toast.makeText(this, "Please fill in all fields", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            if (password != confirmPassword) {
                // Confirm-password matching reduces common sign-up errors (OWASP Foundation, 2021).
                Toast.makeText(this, "Passwords do not match", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            firestore.collection("users")
                .whereEqualTo("email", email)
                .get()
                .addOnSuccessListener { result ->
                    if (!result.isEmpty) {
                        Toast.makeText(this@Register, "Email already registered", Toast.LENGTH_SHORT).show()
                    } else {
                        val userDocument = firestore.collection("users").document()
                        val newUser = User(
                            id = userDocument.id,
                            fullName = fullName,
                            email = email,
                            username = username,
                            password = password
                        )

                        userDocument.set(newUser)
                            .addOnSuccessListener {
                                Toast.makeText(this@Register, "Registration successful", Toast.LENGTH_SHORT).show()
                                // Post-registration navigation with explicit intent (Android Developers, 2026).
                                val intent = Intent(this@Register, Login::class.java)
                                startActivity(intent)
                                finish()
                            }
                            .addOnFailureListener {
                                Toast.makeText(this@Register, "Registration failed", Toast.LENGTH_SHORT).show()
                            }
                    }
                }
                .addOnFailureListener {
                    Toast.makeText(this@Register, "Could not check email", Toast.LENGTH_SHORT).show()
                }
        }

        tvBackToLogin.setOnClickListener {
            val intent = Intent(this, Login::class.java)
            startActivity(intent)
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
