package com.example.prog7313p1
import android.widget.ArrayAdapter
import android.os.Bundle
import android.widget.Button
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import android.widget.Spinner
import android. widget.EditText
import android.widget.Toast
import com.google.firebase.firestore.FirebaseFirestore

class AddCategory : AppCompatActivity() {

    lateinit var btnSaveCategory: Button
    private lateinit var firestore: FirebaseFirestore
    lateinit var spCategoryType: Spinner
    lateinit var etCategoryName: EditText
    lateinit var etCategoryDescription: EditText

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContentView(R.layout.activity_add_category)

        etCategoryName = findViewById(R.id.etCategoryName)
        etCategoryDescription = findViewById(R.id.etCategoryDescription)
        btnSaveCategory = findViewById(R.id.btnSaveCategory)
        spCategoryType = findViewById(R.id.spCategoryType)

        firestore = FirebaseFirestore.getInstance()

        // Predefined category options for controlled user input (Kotlin, n.d.).
        val types = arrayOf("Income", "Expense")

        val adapter = ArrayAdapter(
            this,
            android.R.layout.simple_spinner_item,
            types
        )

        // Spinner drop-down rendering pattern (Android Developers, 2026a).
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        spCategoryType.adapter = adapter

        btnSaveCategory.setOnClickListener {

            val name = etCategoryName.text.toString().trim()
            val description = etCategoryDescription.text.toString().trim()
            val type = spCategoryType.selectedItem.toString()

            // Required field validation for category creation (OWASP Foundation, 2021).
            if (name.isEmpty()) {
                etCategoryName.error = "Enter category name"
                return@setOnClickListener
            }

            val userId = SessionManager.getUserId(this)
            if (userId == null) {
                Toast.makeText(this, "Please log in again", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            val categoryDocument = firestore.collection("categories").document()
            val category = Category(
                id = categoryDocument.id,
                userId = userId,
                name = name,
                description = description,
                type = type
            )

            categoryDocument.set(category)
                .addOnSuccessListener {
                    Toast.makeText(this@AddCategory, "Category Saved", Toast.LENGTH_SHORT).show()
                    finish()
                }
                .addOnFailureListener {
                    Toast.makeText(this@AddCategory, "Category could not be saved", Toast.LENGTH_SHORT).show()
                }
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
Android Developers (2026a) Spinner. Available at: https://developer.android.com/develop/ui/views/components/spinner (Accessed: 29 April 2026).
Firebase (2026) Cloud Firestore. Available at: https://firebase.google.com/docs/firestore (Accessed: 26 May 2026).
Kotlin (n.d.) Arrays. Available at: https://kotlinlang.org/docs/arrays.html (Accessed: 29 April 2026).
OWASP Foundation (2021) Input Validation Cheat Sheet. Available at: https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html (Accessed: 29 April 2026).
*/
