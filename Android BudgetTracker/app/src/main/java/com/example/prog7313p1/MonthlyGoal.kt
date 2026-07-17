package com.example.prog7313p1

data class MonthlyGoal(
    val id: String = "",
    val userId: String = "",
    val categoryId: String = "",
    val minSpend: Double = 0.0,
    val maxSpend: Double = 0.0,
    val startDate: String = "",
    val endDate: String = ""
)
