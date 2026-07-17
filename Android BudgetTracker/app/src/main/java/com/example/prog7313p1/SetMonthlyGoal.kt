package com.example.prog7313p1

import android.app.DatePickerDialog
import android.content.Intent
import android.os.Bundle
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.google.firebase.firestore.FirebaseFirestore
import java.util.*

class SetMonthlyGoal : AppCompatActivity() {

    private lateinit var spinnerCategory: Spinner
    private lateinit var etMinGoal: EditText
    private lateinit var etMaxGoal: EditText
    private lateinit var etStartDate: EditText
    private lateinit var etEndDate: EditText
    private lateinit var btnSaveGoal: Button
    private lateinit var btnViewAllGoals: Button
    private lateinit var firestore: FirebaseFirestore
    private var categories: List<Category> = listOf()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_set_monthly_goal)

        firestore = FirebaseFirestore.getInstance()
        spinnerCategory = findViewById(R.id.spinnerCategory)
        etMinGoal = findViewById(R.id.etMinGoal)
        etMaxGoal = findViewById(R.id.etMaxGoal)
        etStartDate = findViewById(R.id.etStartDate)
        etEndDate = findViewById(R.id.etEndDate)
        btnSaveGoal = findViewById(R.id.btnSaveGoal)
        btnViewAllGoals = findViewById(R.id.btnViewAllGoals)

        loadCategories()

        etStartDate.setOnClickListener { showDatePicker(etStartDate) }
        etEndDate.setOnClickListener { showDatePicker(etEndDate) }

        btnSaveGoal.setOnClickListener {
            saveGoal()
        }

        btnViewAllGoals.setOnClickListener {
            val intent = Intent(this, ViewMonthlyGoals::class.java)
            startActivity(intent)
        }
    }

    private fun showDatePicker(editText: EditText) {
        val calendar = Calendar.getInstance()
        val year = calendar.get(Calendar.YEAR)
        val month = calendar.get(Calendar.MONTH)
        val day = calendar.get(Calendar.DAY_OF_MONTH)

        // Date picker supports consistent ISO-like date capture (Android Developers, 2026a).
        DatePickerDialog(
            this,
            { _, selectedYear, selectedMonth, selectedDay ->
                val formattedDate = String.format("%04d-%02d-%02d", selectedYear, selectedMonth + 1, selectedDay)
                editText.setText(formattedDate)
            },
            year, month, day
        ).show()
    }

    private fun loadCategories() {
        val userId = SessionManager.getUserId(this)
        if (userId == null) {
            Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
            return
        }

        firestore.collection("categories")
            .whereEqualTo("userId", userId)
            .get()
            .addOnSuccessListener { result ->
                categories = result.documents.mapNotNull { it.toObject(Category::class.java) }
                if (categories.isNotEmpty()) {
                    val adapter = ArrayAdapter(this@SetMonthlyGoal, android.R.layout.simple_spinner_item, categories)
                    adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
                    spinnerCategory.adapter = adapter
                } else {
                    Toast.makeText(this@SetMonthlyGoal, "No categories found. Please add categories first.", Toast.LENGTH_LONG).show()
                }
            }
            .addOnFailureListener {
                Toast.makeText(this@SetMonthlyGoal, "Could not load categories", Toast.LENGTH_SHORT).show()
            }
    }

    private fun saveGoal() {
        val selectedCategory = spinnerCategory.selectedItem as? Category
        val minStr = etMinGoal.text.toString()
        val maxStr = etMaxGoal.text.toString()
        val startStr = etStartDate.text.toString()
        val endStr = etEndDate.text.toString()

        // Validate required budgeting inputs before save (CFPB, n.d.; OWASP Foundation, 2021).
        if (selectedCategory == null || minStr.isEmpty() || maxStr.isEmpty() || startStr.isEmpty() || endStr.isEmpty()) {
            Toast.makeText(this, "Please fill all fields", Toast.LENGTH_SHORT).show()
            return
        }

        val min = minStr.toDoubleOrNull() ?: 0.0
        val max = maxStr.toDoubleOrNull() ?: 0.0

        val userId = SessionManager.getUserId(this)
        if (userId == null) {
            Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
            return
        }

        val goalDocument = firestore.collection("monthlyGoals").document()
        val goal = MonthlyGoal(
                id = goalDocument.id,
                userId = userId,
                categoryId = selectedCategory.id,
                minSpend = min,
                maxSpend = max,
                startDate = startStr,
                endDate = endStr
            )

        goalDocument.set(goal)
            .addOnSuccessListener {
                Toast.makeText(this@SetMonthlyGoal, "Goal Saved!", Toast.LENGTH_SHORT).show()
                finish()
            }
            .addOnFailureListener {
                Toast.makeText(this@SetMonthlyGoal, "Goal could not be saved", Toast.LENGTH_SHORT).show()
            }
    }
}

/*
References (Harvard)
Android Developers (2026a) Pickers. Available at: https://developer.android.com/develop/ui/views/components/pickers (Accessed: 29 April 2026).
Firebase (2026) Cloud Firestore. Available at: https://firebase.google.com/docs/firestore (Accessed: 26 May 2026).
CFPB (n.d.) Making a budget. Available at: https://www.consumerfinance.gov/consumer-tools/budgeting/ (Accessed: 29 April 2026).
OWASP Foundation (2021) Input Validation Cheat Sheet. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html (Accessed: 29 April 2026).
*/
