package com.example.prog7313p1

data class Expense(
    val id: String = "",
    val userId: String = "",
    val amount: Double = 0.0,
    val date: String = "",
    val startTime: String = "",
    val endTime: String = "",
    val notes: String? = null,
    val categoryId: String = "",
    val receiptPath: String? = null,
    val createdAt: Long = 0L
)
