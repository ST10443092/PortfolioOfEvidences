package com.example.prog7313p1

import android.os.Bundle
import android.widget.Button
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import android.content.Intent

class MainActivity : AppCompatActivity() {

    lateinit var btnStart: Button
    lateinit var btnRegister: Button






    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContentView(R.layout.activity_main)

        btnStart = findViewById(R.id.btnStart)
        btnRegister = findViewById(R.id.btnRegister)




        btnStart.setOnClickListener {

            // Screen navigation using explicit intents (Android Developers, 2026b).
            val intent = Intent(this, Login::class.java)
            startActivity(intent)

        }
        btnRegister.setOnClickListener {

            // Screen navigation using explicit intents (Android Developers, 2026b).
            val intent = Intent(this, Register::class.java)
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
Android Developers (2026a) Introduction to activities. Available at: https://developer.android.com/guide/components/activities/intro-activities (Accessed: 29 April 2026).
Android Developers (2026b) Intents and intent filters. Available at: https://developer.android.com/guide/components/intents-filters (Accessed: 29 April 2026).
*/