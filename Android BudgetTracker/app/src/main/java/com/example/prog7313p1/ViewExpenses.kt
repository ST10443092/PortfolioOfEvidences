package com.example.prog7313p1

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import android.widget.Toast
import com.google.firebase.firestore.FirebaseFirestore

class ViewExpenses : AppCompatActivity() {

    private lateinit var recyclerView: RecyclerView
    private lateinit var adapter: ExpenseAdapter
    private lateinit var firestore: FirebaseFirestore

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_view_expenses)

        recyclerView = findViewById(R.id.recyclerExpenses)
        // RecyclerView with linear layout for vertical list rendering (Android Developers, 2026a).
        recyclerView.layoutManager = LinearLayoutManager(this)

        firestore = FirebaseFirestore.getInstance()

        loadExpenses()
    }

    private fun loadExpenses() {
        val userId = SessionManager.getUserId(this)
        if (userId == null) {
            Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
            return
        }

        firestore.collection("expenses")
            .whereEqualTo("userId", userId)
            .get()
            .addOnSuccessListener { result ->
                val expenses = result.documents
                    .mapNotNull { it.toObject(Expense::class.java) }
                    .sortedByDescending { it.createdAt }
                adapter = ExpenseAdapter(expenses)
                recyclerView.adapter = adapter
            }
            .addOnFailureListener {
                Toast.makeText(this, "Could not load expenses", Toast.LENGTH_SHORT).show()
            }
    }
}

/*
References (Harvard)
Android Developers (2026a) Create dynamic lists with RecyclerView. Available at: https://developer.android.com/develop/ui/views/layout/recyclerview (Accessed: 29 April 2026).
Firebase (2026) Cloud Firestore. Available at: https://firebase.google.com/docs/firestore (Accessed: 26 May 2026).
*/
