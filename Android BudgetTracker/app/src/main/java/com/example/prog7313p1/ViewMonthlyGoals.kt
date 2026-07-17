package com.example.prog7313p1

import android.graphics.Color
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import com.google.firebase.firestore.FirebaseFirestore

class ViewMonthlyGoals : AppCompatActivity() {

    private lateinit var rvMonthlyGoals: RecyclerView
    private lateinit var firestore: FirebaseFirestore

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_view_monthly_goals)

        firestore = FirebaseFirestore.getInstance()
        rvMonthlyGoals = findViewById(R.id.rvMonthlyGoals)
        // RecyclerView list presentation for goal monitoring (Android Developers, 2026a).
        rvMonthlyGoals.layoutManager = LinearLayoutManager(this)

        loadGoals()
    }

    private fun loadGoals() {
        val userId = SessionManager.getUserId(this)
        if (userId == null) {
            Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
            return
        }

        firestore.collection("monthlyGoals")
            .whereEqualTo("userId", userId)
            .get()
            .addOnSuccessListener { goalResult ->
                val goals = goalResult.documents.mapNotNull { it.toObject(MonthlyGoal::class.java) }

                firestore.collection("categories")
                    .whereEqualTo("userId", userId)
                    .get()
                    .addOnSuccessListener { categoryResult ->
                        val categories = categoryResult.documents
                            .mapNotNull { it.toObject(Category::class.java) }
                            .associateBy { it.id }

                        firestore.collection("expenses")
                            .whereEqualTo("userId", userId)
                            .get()
                            .addOnSuccessListener { expenseResult ->
                                val expenses = expenseResult.documents.mapNotNull { it.toObject(Expense::class.java) }
                                val goalDataList = goals.map { goal ->
                                    val categoryName = categories[goal.categoryId]?.name ?: "Unknown"
                                    val totalSpent = expenses
                                        .filter { it.categoryId == goal.categoryId }
                                        .sumOf { it.amount }
                                    GoalDisplayData(goal, categoryName, totalSpent)
                                }

                rvMonthlyGoals.adapter = GoalsAdapter(goalDataList)
            }
                            .addOnFailureListener {
                                Toast.makeText(this, "Could not load expenses", Toast.LENGTH_SHORT).show()
                            }
                    }
                    .addOnFailureListener {
                        Toast.makeText(this, "Could not load categories", Toast.LENGTH_SHORT).show()
                    }
            }
            .addOnFailureListener {
                Toast.makeText(this, "Could not load goals", Toast.LENGTH_SHORT).show()
            }
    }

    data class GoalDisplayData(
        val goal: MonthlyGoal,
        val categoryName: String,
        val totalSpent: Double
    )

    class GoalsAdapter(private val goals: List<GoalDisplayData>) :
        RecyclerView.Adapter<GoalsAdapter.GoalViewHolder>() {

        class GoalViewHolder(view: View) : RecyclerView.ViewHolder(view) {
            val tvCategoryName: TextView = view.findViewById(R.id.tvCategoryName)
            val tvDateRange: TextView = view.findViewById(R.id.tvDateRange)
            val tvMinGoal: TextView = view.findViewById(R.id.tvMinGoal)
            val tvMaxGoal: TextView = view.findViewById(R.id.tvMaxGoal)
            val tvCurrentSpent: TextView = view.findViewById(R.id.tvCurrentSpent)
            val tvStatus: TextView = view.findViewById(R.id.tvStatus)
        }

        override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): GoalViewHolder {
            val view = LayoutInflater.from(parent.context)
                .inflate(R.layout.item_monthly_goal, parent, false)
            return GoalViewHolder(view)
        }

        override fun onBindViewHolder(holder: GoalViewHolder, position: Int) {
            val data = goals[position]
            holder.tvCategoryName.text = data.categoryName
            holder.tvDateRange.text = "${data.goal.startDate} to ${data.goal.endDate}"
            holder.tvMinGoal.text = "Min: R ${data.goal.minSpend}"
            holder.tvMaxGoal.text = "Max: R ${data.goal.maxSpend}"
            holder.tvCurrentSpent.text = "Current Spent: R ${data.totalSpent}"

            // Traffic-light style status for budget threshold tracking (CFPB, n.d.).
            if (data.totalSpent > data.goal.maxSpend) {
                holder.tvStatus.text = "Status: EXCEEDED"
                holder.tvStatus.setTextColor(Color.RED)
            } else if (data.totalSpent < data.goal.minSpend) {
                holder.tvStatus.text = "Status: BELOW MINIMUM"
                holder.tvStatus.setTextColor(Color.BLUE)
            } else {
                holder.tvStatus.text = "Status: WITHIN GOAL"
                holder.tvStatus.setTextColor(Color.GREEN)
            }
        }

        override fun getItemCount() = goals.size
    }
}

/*
References (Harvard)
Android Developers (2026a) Create dynamic lists with RecyclerView. Available at: https://developer.android.com/develop/ui/views/layout/recyclerview (Accessed: 29 April 2026).
Firebase (2026) Cloud Firestore. Available at: https://firebase.google.com/docs/firestore (Accessed: 26 May 2026).
CFPB (n.d.) Making a budget. Available at: https://www.consumerfinance.gov/consumer-tools/budgeting/ (Accessed: 29 April 2026).
*/
