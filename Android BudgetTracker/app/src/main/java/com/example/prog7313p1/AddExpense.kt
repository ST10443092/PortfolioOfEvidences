package com.example.prog7313p1

import android.app.AlertDialog
import android.app.DatePickerDialog
import android.app.TimePickerDialog
import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.provider.MediaStore
import android.widget.*
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import java.util.Calendar
import com.google.firebase.firestore.FirebaseFirestore


class AddExpense : AppCompatActivity() {

    lateinit var btnSaveExpense: Button
    lateinit var btnAddReceipt: Button
    lateinit var spCategory: Spinner
    lateinit var tvCancel: TextView

    private var receiptUri: Uri? = null
    private val firestore by lazy { FirebaseFirestore.getInstance() }


    // Register for image picker result
    private val imagePickerLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == RESULT_OK) {
            result.data?.data?.let { uri ->
                receiptUri = uri
                btnAddReceipt.text = "Receipt Photo Selected"
                btnAddReceipt.setBackgroundColor(getColor(android.R.color.holo_green_light))
                Toast.makeText(this, "Receipt photo added", Toast.LENGTH_SHORT).show()
            }
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContentView(R.layout.activity_add_expense)

        initializeViews()
        setupDateAndTimePickers()
        setupClickListeners()
        loadCategories()
        setupWindowInsets()
    }

    private fun initializeViews() {
        btnSaveExpense = findViewById(R.id.btnSaveExpense)
        btnAddReceipt = findViewById(R.id.btnAddReceipt)
        spCategory = findViewById(R.id.spCategory)
        tvCancel = findViewById(R.id.tvCancel)
    }

    private fun setupDateAndTimePickers() {
        val etDate = findViewById<EditText>(R.id.etDate)
        val etStartTime = findViewById<EditText>(R.id.etStartTime)
        val etEndTime = findViewById<EditText>(R.id.etEndTime)

        // Date picker for structured date input (Android Developers, 2026a).
        etDate.setOnClickListener {
            showDatePicker(etDate)
        }

        // Time picker for consistent time entry (Android Developers, 2026b).
        etStartTime.setOnClickListener {
            showTimePicker(etStartTime)
        }

        // Time picker for consistent time entry (Android Developers, 2026b).
        etEndTime.setOnClickListener {
            showTimePicker(etEndTime)
        }

        // Also make them focusable=false to prevent keyboard from showing
        etDate.setRawInputType(android.text.InputType.TYPE_NULL)
        etStartTime.setRawInputType(android.text.InputType.TYPE_NULL)
        etEndTime.setRawInputType(android.text.InputType.TYPE_NULL)
    }

    private fun showDatePicker(etDate: EditText) {
        val calendar = Calendar.getInstance()
        val year = calendar.get(Calendar.YEAR)
        val month = calendar.get(Calendar.MONTH)
        val day = calendar.get(Calendar.DAY_OF_MONTH)

        DatePickerDialog(
            this,
            { _, selectedYear, selectedMonth, selectedDay ->
                val formattedDate = String.format("%04d-%02d-%02d", selectedYear, selectedMonth + 1, selectedDay)
                etDate.setText(formattedDate)
            },
            year, month, day
        ).show()
    }

    private fun showTimePicker(etTime: EditText) {
        val calendar = Calendar.getInstance()
        val hour = calendar.get(Calendar.HOUR_OF_DAY)
        val minute = calendar.get(Calendar.MINUTE)

        TimePickerDialog(
            this,
            { _, selectedHour, selectedMinute ->
                val formattedTime = String.format("%02d:%02d", selectedHour, selectedMinute)
                etTime.setText(formattedTime)
            },
            hour, minute, true
        ).show()
    }

    private fun setupClickListeners() {
        btnSaveExpense.setOnClickListener { saveExpense() }
        btnAddReceipt.setOnClickListener { openImagePicker() }
        tvCancel.setOnClickListener {
            finish() // Close the activity and go back
        }
    }

    private fun openImagePicker() {
        val intent = Intent(Intent.ACTION_PICK, MediaStore.Images.Media.EXTERNAL_CONTENT_URI)
        intent.type = "image/png"
        intent.putExtra(Intent.EXTRA_MIME_TYPES, arrayOf("image/png"))
        imagePickerLauncher.launch(intent)
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
                val categories = result.documents.mapNotNull { it.toObject(Category::class.java) }
            if (categories.isNotEmpty()) {
                val adapter = ArrayAdapter(
                    this@AddExpense,
                    android.R.layout.simple_spinner_item,
                    categories
                )
                adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
                spCategory.adapter = adapter
            } else {
                Toast.makeText(this@AddExpense, "No categories found. Please add categories first.", Toast.LENGTH_LONG).show()
            }
            }
            .addOnFailureListener {
                Toast.makeText(this@AddExpense, "Could not load categories", Toast.LENGTH_LONG).show()
            }
    }

    private fun saveExpense() {
        val etAmount = findViewById<EditText>(R.id.etAmount)
        val etDate = findViewById<EditText>(R.id.etDate)
        val etNotes = findViewById<EditText>(R.id.etNotes)
        val etStartTime = findViewById<EditText>(R.id.etStartTime)
        val etEndTime = findViewById<EditText>(R.id.etEndTime)

        val amountStr = etAmount.text.toString()
        val date = etDate.text.toString()
        val notes = etNotes.text.toString()
        val startTime = etStartTime.text.toString()
        val endTime = etEndTime.text.toString()

        // Input validation for data quality and error prevention (OWASP Foundation, 2021).
        if (amountStr.isEmpty()) {
            etAmount.error = "Amount is required"
            etAmount.requestFocus()
            return
        }

        if (date.isEmpty()) {
            etDate.error = "Date is required"
            etDate.requestFocus()
            return
        }

        if (startTime.isEmpty()) {
            etStartTime.error = "Start time is required"
            etStartTime.requestFocus()
            return
        }

        if (endTime.isEmpty()) {
            etEndTime.error = "End time is required"
            etEndTime.requestFocus()
            return
        }

        try {
            val amountValue = amountStr.toDouble()
            if (amountValue <= 0) {
                etAmount.error = "Amount must be greater than 0"
                etAmount.requestFocus()
                return
            }

            val selectedCategory = spCategory.selectedItem as? Category
            if (selectedCategory == null) {
                Toast.makeText(this, "Please select a category", Toast.LENGTH_SHORT).show()
                return
            }

            val userId = SessionManager.getUserId(this)
            if (userId == null) {
                Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
                return
            }

            val expenseDocument = firestore.collection("expenses").document()
            val expense = Expense(
                id = expenseDocument.id,
                userId = userId,
                amount = amountValue,
                date = date,
                startTime = startTime,
                endTime = endTime,
                notes = notes,
                categoryId = selectedCategory.id,
                receiptPath = receiptUri?.toString(),
                createdAt = System.currentTimeMillis()
            )

            expenseDocument.set(expense)
                .addOnSuccessListener {
                    checkGoalsAndNotify(selectedCategory.id)
                }
                .addOnFailureListener {
                    Toast.makeText(this, "Error saving expense", Toast.LENGTH_LONG).show()
                }
        } catch (e: NumberFormatException) {
            etAmount.error = "Please enter a valid number"
            etAmount.requestFocus()
        } catch (e: Exception) {
            Toast.makeText(this, "Error saving expense: ${e.message}", Toast.LENGTH_LONG).show()
        }
    }

    private fun checkGoalsAndNotify(categoryId: String) {
        val userId = SessionManager.getUserId(this)
        if (userId == null) {
            finishWithSuccess()
            return
        }

        firestore.collection("monthlyGoals")
            .whereEqualTo("userId", userId)
            .whereEqualTo("categoryId", categoryId)
            .limit(1)
            .get()
            .addOnSuccessListener { goalResult ->
                val goal = goalResult.documents.firstOrNull()?.toObject(MonthlyGoal::class.java)
                if (goal == null) {
                    finishWithSuccess()
                    return@addOnSuccessListener
                }

                firestore.collection("expenses")
                    .whereEqualTo("userId", userId)
                    .whereEqualTo("categoryId", categoryId)
                    .get()
                    .addOnSuccessListener { expenseResult ->
                        val totalSpent = expenseResult.documents
                            .mapNotNull { it.toObject(Expense::class.java) }
                            .sumOf { it.amount }

                        // Budget monitoring with threshold checks (CFPB, n.d.).
                        if (totalSpent > goal.maxSpend) {
                            showAlert("Limit Exceeded!", "You have spent R $totalSpent, which exceeds your maximum goal of R ${goal.maxSpend} for this category.")
                        } else if (totalSpent < goal.minSpend) {
                            Toast.makeText(this, "Under minimum target: R $totalSpent / R ${goal.minSpend}", Toast.LENGTH_SHORT).show()
                            finishWithSuccess()
                        } else {
                            finishWithSuccess()
                        }
                    }
                    .addOnFailureListener {
                        finishWithSuccess()
                    }
            }
            .addOnFailureListener {
                finishWithSuccess()
            }
    }

    private fun showAlert(title: String, message: String) {
        AlertDialog.Builder(this)
            .setTitle(title)
            .setMessage(message)
            .setPositiveButton("OK") { _, _ ->
                finishWithSuccess()
            }
            .setCancelable(false)
            .show()
    }

    private fun finishWithSuccess() {
        Toast.makeText(this, "Expense saved successfully!", Toast.LENGTH_SHORT).show()
        val intent = Intent(this, Home::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_CLEAR_TOP or Intent.FLAG_ACTIVITY_NEW_TASK
        startActivity(intent)
        finish()
    }

    private fun setupWindowInsets() {
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main)) { v, insets ->
            val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom)
            insets
        }
    }
}

/*
References (Harvard)
Android Developers (2026a) Pickers. Available at: https://developer.android.com/develop/ui/views/components/pickers (Accessed: 29 April 2026).
Android Developers (2026b) TimePickerDialog. Available at: https://developer.android.com/reference/android/app/TimePickerDialog (Accessed: 29 April 2026).
Firebase (2026) Cloud Firestore. Available at: https://firebase.google.com/docs/firestore (Accessed: 26 May 2026).
CFPB (n.d.) Making a budget. Available at: https://www.consumerfinance.gov/consumer-tools/budgeting/ (Accessed: 29 April 2026).
OWASP Foundation (2021) Input Validation Cheat Sheet. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html (Accessed: 29 April 2026).
*/
