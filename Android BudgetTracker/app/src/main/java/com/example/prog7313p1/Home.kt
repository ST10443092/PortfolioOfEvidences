package com.example.prog7313p1

import android.app.DatePickerDialog
import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.widget.*
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import com.google.firebase.firestore.FirebaseFirestore
import java.util.Calendar
import java.util.Locale

class Home : AppCompatActivity() {

    lateinit var btnAddExpense: Button
    lateinit var btnAddCategory: Button
    lateinit var btnViewExpenses: Button
    lateinit var btnSetMonthlyGoal: Button
    lateinit var btnViewSummary: Button

    private lateinit var firestore: FirebaseFirestore
    private lateinit var etGraphStartDate: EditText
    private lateinit var etGraphEndDate: EditText

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContentView(R.layout.activity_home)

        // Buttons
        btnAddExpense = findViewById(R.id.btnAddExpense)
        btnAddCategory = findViewById(R.id.btnAddCategory)
        btnViewExpenses = findViewById(R.id.btnViewExpenses)
        btnSetMonthlyGoal = findViewById(R.id.btnViewMonthlyGoals)
        btnViewSummary = findViewById(R.id.btnViewSummary)
        etGraphStartDate = findViewById(R.id.etGraphStartDate)
        etGraphEndDate = findViewById(R.id.etGraphEndDate)

        firestore = FirebaseFirestore.getInstance()

        etGraphStartDate.setOnClickListener { showDatePicker(etGraphStartDate) }
        etGraphEndDate.setOnClickListener { showDatePicker(etGraphEndDate) }

        // Clicks
        btnAddExpense.setOnClickListener {
            // Navigation to features through explicit intents (Android Developers, 2026a).
            startActivity(Intent(this, AddExpense::class.java))
        }

        btnAddCategory.setOnClickListener {
            // Navigation to features through explicit intents (Android Developers, 2026a).
            startActivity(Intent(this, AddCategory::class.java))
        }

        btnViewExpenses.setOnClickListener {
            // Navigation to features through explicit intents (Android Developers, 2026a).
            startActivity(Intent(this, ViewExpenses::class.java))
        }

        btnSetMonthlyGoal.setOnClickListener {
            // Navigation to features through explicit intents (Android Developers, 2026a).
            startActivity(Intent(this, SetMonthlyGoal::class.java))
        }

        btnViewSummary.setOnClickListener {
            // Navigation to features through explicit intents (Android Developers, 2026a).
            startActivity(Intent(this, ViewSummary::class.java))
        }

        // Insets
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main)) { v, insets ->
            val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom)
            insets
        }
    }

    override fun onResume() {
        super.onResume()
        if (::firestore.isInitialized) {
            loadHomeData()
        }
    }

    private fun loadHomeData() {
        val tvTotalSpent = findViewById<TextView>(R.id.tvTotalSpent)
        val graph = findViewById<CategorySpendGraphView>(R.id.categorySpendGraph)

        val tvAmount = findViewById<TextView>(R.id.tvRecentAmount)
        val tvDate = findViewById<TextView>(R.id.tvRecentDate)
        val tvNotes = findViewById<TextView>(R.id.tvRecentNotes)
        val img = findViewById<ImageView>(R.id.imgRecentReceipt)
        val startDate = etGraphStartDate.text.toString()
        val endDate = etGraphEndDate.text.toString()

        val userId = SessionManager.getUserId(this)
        if (userId == null) {
            showDashboardError(tvTotalSpent, graph, tvAmount, tvDate, tvNotes, img)
            Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
            return
        }

        firestore.collection("categories")
            .whereEqualTo("userId", userId)
            .get()
            .addOnSuccessListener { categoryResult ->
                val categories = categoryResult.documents
                    .mapNotNull { it.toObject(Category::class.java) }
                    .associateBy { it.id }

                firestore.collection("monthlyGoals")
                    .whereEqualTo("userId", userId)
                    .get()
                    .addOnSuccessListener { goalResult ->
                        val goals = goalResult.documents
                            .mapNotNull { it.toObject(MonthlyGoal::class.java) }

                        firestore.collection("expenses")
                            .whereEqualTo("userId", userId)
                            .get()
                            .addOnSuccessListener { expenseResult ->
                                val allExpenses = expenseResult.documents
                                    .mapNotNull { it.toObject(Expense::class.java) }
                                val expenses = filterExpensesByDate(allExpenses, startDate, endDate)

                                val spendingExpenses = expenses
                                    .filter { !categories[it.categoryId]?.type.equals("Income", ignoreCase = true) }
                                val expenseTotal = spendingExpenses.sumOf { it.amount }

                                tvTotalSpent.text = formatCurrency(expenseTotal)
                                graph.setData(buildGraphData(spendingExpenses, categories, goals, startDate, endDate))

                                showRecentExpense(allExpenses.maxByOrNull { it.createdAt }, tvAmount, tvDate, tvNotes, img)
                            }
                            .addOnFailureListener {
                                showDashboardError(tvTotalSpent, graph, tvAmount, tvDate, tvNotes, img)
                            }
                    }
                    .addOnFailureListener {
                        showDashboardError(tvTotalSpent, graph, tvAmount, tvDate, tvNotes, img)
                    }
            }
            .addOnFailureListener {
                showDashboardError(tvTotalSpent, graph, tvAmount, tvDate, tvNotes, img)
            }
    }

    private fun showDatePicker(editText: EditText) {
        val calendar = Calendar.getInstance()
        DatePickerDialog(
            this,
            { _, year, month, day ->
                val date = String.format(Locale.getDefault(), "%04d-%02d-%02d", year, month + 1, day)
                editText.setText(date)
                loadHomeData()
            },
            calendar.get(Calendar.YEAR),
            calendar.get(Calendar.MONTH),
            calendar.get(Calendar.DAY_OF_MONTH)
        ).show()
    }

    private fun filterExpensesByDate(expenses: List<Expense>, startDate: String, endDate: String): List<Expense> {
        if (startDate.isEmpty() || endDate.isEmpty()) return expenses
        return expenses.filter { it.date in startDate..endDate }
    }

    private fun buildGraphData(
        expenses: List<Expense>,
        categories: Map<String, Category>,
        goals: List<MonthlyGoal>,
        startDate: String,
        endDate: String
    ): List<CategorySpendGraphView.CategorySpendData> {
        return expenses.groupBy { it.categoryId }
            .map { (categoryId, categoryExpenses) ->
                val categoryName = categories[categoryId]?.name ?: "Unknown"
                val categoryGoals = goals.filter {
                    it.categoryId == categoryId && goalMatchesPeriod(it, startDate, endDate)
                }
                CategorySpendGraphView.CategorySpendData(
                    categoryName = categoryName,
                    spent = categoryExpenses.sumOf { it.amount },
                    minGoal = categoryGoals.maxOfOrNull { it.minSpend } ?: 0.0,
                    maxGoal = categoryGoals.maxOfOrNull { it.maxSpend } ?: 0.0
                )
            }
            .sortedByDescending { it.spent }
    }

    private fun goalMatchesPeriod(goal: MonthlyGoal, startDate: String, endDate: String): Boolean {
        if (startDate.isEmpty() || endDate.isEmpty()) return true
        return goal.startDate <= endDate && goal.endDate >= startDate
    }

    private fun showRecentExpense(
        latest: Expense?,
        tvAmount: TextView,
        tvDate: TextView,
        tvNotes: TextView,
        img: ImageView
    ) {
        if (latest != null) {
            tvAmount.text = formatCurrency(latest.amount)
            tvDate.text = latest.date
            tvNotes.text = latest.notes ?: "No notes"

            if (!latest.receiptPath.isNullOrEmpty()) {
                try {
                    val uri = Uri.parse(latest.receiptPath)
                    img.setImageURI(uri)
                } catch (e: Exception) {
                    img.setImageResource(android.R.drawable.ic_menu_report_image)
                }
            } else {
                img.setImageResource(android.R.drawable.ic_menu_report_image)
            }
        } else {
            tvAmount.text = "R 0.00"
            tvDate.text = "No expenses yet"
            tvNotes.text = ""
            img.setImageResource(android.R.drawable.ic_menu_report_image)
        }
    }

    private fun showDashboardError(
        tvTotalSpent: TextView,
        graph: CategorySpendGraphView,
        tvAmount: TextView,
        tvDate: TextView,
        tvNotes: TextView,
        img: ImageView
    ) {
        tvTotalSpent.text = "R 0.00"
        graph.setData(emptyList())
        tvAmount.text = "R 0.00"
        tvDate.text = "Could not load dashboard"
        tvNotes.text = ""
        img.setImageResource(android.R.drawable.ic_menu_report_image)
    }

    private fun formatCurrency(amount: Double): String {
        return "R %.2f".format(amount)
    }
}

/*
References (Harvard)
Android Developers (2026a) Intents and intent filters. Available at: https://developer.android.com/guide/components/intents-filters (Accessed: 29 April 2026).
Firebase (2026) Cloud Firestore. Available at: https://firebase.google.com/docs/firestore (Accessed: 26 May 2026).
*/
