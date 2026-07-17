package com.example.prog7313p1

import android.app.DatePickerDialog
import android.os.Bundle
import android.view.View
import android.view.ViewGroup
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.google.firebase.firestore.FirebaseFirestore
import java.util.*

class ViewSummary : AppCompatActivity() {

    private lateinit var etFilterStartDate: EditText
    private lateinit var etFilterEndDate: EditText
    private lateinit var btnApplyFilter: Button

    private lateinit var btnClearFilter: Button
    private lateinit var rvCategoryTotals: RecyclerView
    private lateinit var rvDetailedExpenses: RecyclerView
    private lateinit var firestore: FirebaseFirestore

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_view_summary)

        firestore = FirebaseFirestore.getInstance()
        etFilterStartDate = findViewById(R.id.etFilterStartDate)
        etFilterEndDate = findViewById(R.id.etFilterEndDate)
        btnApplyFilter = findViewById(R.id.btnApplyFilter)
        rvCategoryTotals = findViewById(R.id.rvCategoryTotals)
        rvDetailedExpenses = findViewById(R.id.rvDetailedExpenses)
        btnClearFilter = findViewById(R.id.btnClearFilter)

        // RecyclerViews for summary list rendering (Android Developers, 2026a).
        rvCategoryTotals.layoutManager = LinearLayoutManager(this)
        rvDetailedExpenses.layoutManager = LinearLayoutManager(this)

        etFilterStartDate.setOnClickListener { showDatePicker(etFilterStartDate) }
        etFilterEndDate.setOnClickListener { showDatePicker(etFilterEndDate) }

        btnApplyFilter.setOnClickListener { loadSummary() }
        btnClearFilter.setOnClickListener {
            etFilterStartDate.setText("")
            etFilterEndDate.setText("")
            loadSummary()
        }

        loadSummary() // Load all by default
    }

    private fun showDatePicker(editText: EditText) {
        val calendar = Calendar.getInstance()
        // Date picker supports constrained and consistent date input (Android Developers, 2026b).
        DatePickerDialog(this, { _, y, m, d ->
            val date = String.format(Locale.getDefault(), "%04d-%02d-%02d", y, m + 1, d)
            editText.setText(date)
        }, calendar.get(Calendar.YEAR), calendar.get(Calendar.MONTH), calendar.get(Calendar.DAY_OF_MONTH)).show()
    }

    private fun loadSummary() {
        val start = etFilterStartDate.text.toString()
        val end = etFilterEndDate.text.toString()

        val userId = SessionManager.getUserId(this)
        if (userId == null) {
            Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
            return
        }

        firestore.collection("expenses")
            .whereEqualTo("userId", userId)
            .get()
            .addOnSuccessListener { expenseResult ->
                var expenses = expenseResult.documents.mapNotNull { it.toObject(Expense::class.java) }

                firestore.collection("categories")
                    .whereEqualTo("userId", userId)
                    .get()
                    .addOnSuccessListener { categoryResult ->
                        val categories = categoryResult.documents
                            .mapNotNull { it.toObject(Category::class.java) }
                            .associateBy { it.id }

                        // Range filtering to support budgeting period analysis (CFPB, n.d.).
                        if (start.isNotEmpty() && end.isNotEmpty()) {
                            expenses = expenses.filter { it.date in start..end }
                        }

                        // Aggregation by category reflects common budget tracking practice (CFPB, n.d.).
                        val categoryTotals = expenses.groupBy { it.categoryId }
                            .map { (catId, list) ->
                                val name = categories[catId]?.name ?: "Unknown"
                                val total = list.sumOf { it.amount }
                                "$name: R $total"
                            }

                // Adapter for Totals
                rvCategoryTotals.adapter = SimpleListAdapter(categoryTotals)
                // Reuse existing ExpenseAdapter for details
                rvDetailedExpenses.adapter = ExpenseAdapter(expenses)
            }
                    .addOnFailureListener {
                        Toast.makeText(this, "Could not load categories", Toast.LENGTH_SHORT).show()
                    }
            }
            .addOnFailureListener {
                Toast.makeText(this, "Could not load expenses", Toast.LENGTH_SHORT).show()
            }
    }

    // Simple internal adapter for strings
    class SimpleListAdapter(private val items: List<String>) : RecyclerView.Adapter<SimpleListAdapter.VH>() {
        class VH(v: View) : RecyclerView.ViewHolder(v) { val t = v as TextView }
        override fun onCreateViewHolder(p: ViewGroup, t: Int) = VH(TextView(p.context).apply {
            textSize = 16f // Fixed: Changed from 16sp (which is XML syntax) to 16f (Kotlin float)
            setPadding(10, 10, 10, 10)
        })
        override fun onBindViewHolder(h: VH, p: Int) { h.t.text = items[p] }
        override fun getItemCount() = items.size
    }
}

/*
References (Harvard)
Android Developers (2026a) Create dynamic lists with RecyclerView. Available at: https://developer.android.com/develop/ui/views/layout/recyclerview (Accessed: 29 April 2026).
Android Developers (2026b) Pickers. Available at: https://developer.android.com/develop/ui/views/components/pickers (Accessed: 29 April 2026).
CFPB (n.d.) Making a budget. Available at: https://www.consumerfinance.gov/consumer-tools/budgeting/ (Accessed: 29 April 2026).
*/
