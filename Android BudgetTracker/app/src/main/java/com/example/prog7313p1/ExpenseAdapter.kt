package com.example.prog7313p1

import android.net.Uri
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView

class ExpenseAdapter(private val expenses: List<Expense>) :
    RecyclerView.Adapter<ExpenseAdapter.ViewHolder>() {

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val amount: TextView = view.findViewById(R.id.tvAmount)
        val date: TextView = view.findViewById(R.id.tvDate)
        val notes: TextView = view.findViewById(R.id.tvNotes)
        val image: ImageView = view.findViewById(R.id.imgReceipt)
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        // Inflate list item layout for RecyclerView rows (Android Developers, 2026a).
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_expense, parent, false)
        return ViewHolder(view)
    }

    override fun getItemCount(): Int = expenses.size

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val expense = expenses[position]

        // Text data
        holder.amount.text = "R ${expense.amount}"
        holder.date.text = expense.date
        holder.notes.text = expense.notes ?: "No notes"

        // URI-based image binding from persisted receipt path (Android Developers, 2026b).
        if (!expense.receiptPath.isNullOrEmpty()) {
            try {
                val uri = Uri.parse(expense.receiptPath)
                holder.image.setImageURI(uri)
            } catch (e: Exception) {
                holder.image.setImageResource(android.R.drawable.ic_menu_report_image)
            }
        } else {
            holder.image.setImageResource(android.R.drawable.ic_menu_report_image)
        }
    }
}

/*
References (Harvard)
Android Developers (2026a) Create dynamic lists with RecyclerView. Available at: https://developer.android.com/develop/ui/views/layout/recyclerview (Accessed: 29 April 2026).
Android Developers (2026b) Displaying images with ImageView. Available at: https://developer.android.com/reference/android/widget/ImageView (Accessed: 29 April 2026).
*/